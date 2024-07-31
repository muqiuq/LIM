using Azure.Identity;
using LIM.EntityServices.Helpers;
using LIM.Helpers;
using LIM.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace LIM.EntityServices
{
    public class ListGraphService
    {
        DateTimeOffset lastSeenModifiedDateTime = DateTimeOffset.MinValue;

        public ListGraphService(IConfigurationRoot configuration, LimSettings userSettings)
        {
            Configuration = configuration;
            UserSettings = userSettings;
            SiteId = UserSettings.SharePointUrl;
        }

        public IConfigurationRoot Configuration { get; }
        public LimSettings UserSettings { get; }

        public string SiteId { get; private set; }

        public object NormalizeValue(object inValue)
        {
            object value = inValue;
            if (inValue is IEnumerable<string>)
            {
                value = string.Join("\n", ((IEnumerable<string>)inValue));
            }
            return value;
        }

        public async Task<bool> CreateNewItem<T>(String TableName, T entity) where T : IEntity, new()
        {
            var gc = GetGraphServiceClientApplication();

            var site = await gc.Sites[SiteId].GetAsync();

            var properties = typeof(T).GetProperties();
            var fieldsToUpdate = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<MsListColumn>();
                if (attribute != null && property.Name.ToLower() != "id")
                {
                    var value = property.GetValue(entity);
                    if (value != null)
                    {
                        fieldsToUpdate[attribute.Name] = NormalizeValue(value);
                    }
                }
            }

            if (fieldsToUpdate.Any())
            {
                var requestBody = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = fieldsToUpdate
                    }
                };
                var updateResult = await gc.Sites[site.Id].Lists[TableName].Items.PostAsync(requestBody);
                if (updateResult == null) return false;
                entity.Id = updateResult.Id;
                return true;
            }
            return false;
        }

        public async Task<int> UploadNewItems<T>(EntityManager<T> manager) where T : IEntity, new()
        {
            var gc = GetGraphServiceClientApplication();

            var site = await gc.Sites[SiteId].GetAsync();

            var uploadItemChanges = 0;

            var changedItems = manager.GetLocalyNewEntries();

            foreach (var item in changedItems)
            {
                var properties = typeof(T).GetProperties();
                var fieldsToUpdate = new Dictionary<string, object>();

                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<MsListColumn>();
                    if (attribute != null && property.Name.ToLower() != "id")
                    {
                        var value = property.GetValue(item);
                        if (value != null)
                        {
                            fieldsToUpdate[attribute.Name] = NormalizeValue(value);
                        }
                    }
                }

                if (fieldsToUpdate.Any())
                {
                    var requestBody = new ListItem
                    {
                        Fields = new FieldValueSet
                        {
                            AdditionalData = fieldsToUpdate
                        }
                    };
                    uploadItemChanges++;
                    var updateResult = await gc.Sites[site.Id].Lists[manager.TableName].Items.PostAsync(requestBody);
                    item.Id = updateResult.Id;
                    manager.SetUpdated(item, false);
                }

            }
            return uploadItemChanges;
        }

        public async Task<int> UploadLocalChanges<T>(EntityManager<T> manager) where T : IEntity, new()
        {
            var gc = GetGraphServiceClientApplication();

            var site = await gc.Sites[SiteId].GetAsync();

            var uploadItemChanges = 0;

            var changedItems = manager.GetLocalyChangedEntries();

            foreach (var item in changedItems)
            {
                var properties = typeof(T).GetProperties();
                var fieldsToUpdate = new Dictionary<string, object>();
                var graphItemInList = await gc.Sites[site.Id].Lists[manager.TableName].Items[item.Id].GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.QueryParameters.Expand = new string[] { "fields($select=*)" };
                });

                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<MsListColumn>();
                    if (attribute != null)
                    {

                        if (graphItemInList.Fields.AdditionalData.TryGetValue(attribute.Name, out var graphFieldValue))
                        {
                            if (!property.GetValue(item).Equals(graphFieldValue))
                            {
                                fieldsToUpdate[attribute.Name] = NormalizeValue(property.GetValue(item));
                            }
                        }
                        else if(property.Name != "Id")
                        {
                            var value = property.GetValue(item);
                            if (value != null)
                            {
                                fieldsToUpdate[attribute.Name] = NormalizeValue(value);
                            }
                        }
                    }
                }

                if (fieldsToUpdate.Any())
                {
                    var requestBody = new FieldValueSet
                    {
                        AdditionalData = fieldsToUpdate
                    };
                    uploadItemChanges++;
                    var updateResult = await gc.Sites[site.Id].Lists[manager.TableName].Items[item.Id].Fields.PatchAsync(requestBody);
                    manager.SetUpdated(item, false);
                    manager.SetRequiresLogEntry(item, true);
                }

            }
            return uploadItemChanges;
        }

        public async Task UpdateColumnInfo<T>(EntityManager<T> manager) where T: IEntity, new()
        {
            var gc = GetGraphServiceClientApplication();

            var selectedSite = await gc.Sites[SiteId].GetAsync();

            var myList = await gc.Sites[selectedSite.Id].Lists[manager.TableName].GetAsync();

            if (myList.LastModifiedDateTime != null && lastSeenModifiedDateTime >= myList.LastModifiedDateTime)
            {
                Debug.WriteLine("No change. No Update (columns)");
                return;
            }

            var graphColumns = await gc.Sites[selectedSite.Id].Lists[manager.TableName].Columns.GetAsync();

            foreach (var column in graphColumns.Value)
            {
                if(column.Choice != null)
                {
                    manager.AddOrUpdateChoices(column.Name, column.Choice.Choices);
                }
            }

        }

        public async Task<ColumnDefinitionCollectionResponse?> GetColumnInfo(string tableName)
        {
            var gc = GetGraphServiceClientApplication();

            var selectedSite = await gc.Sites[SiteId].GetAsync();

            var myList = await gc.Sites[selectedSite.Id].Lists[tableName].GetAsync();

            var graphColumns = await gc.Sites[selectedSite.Id].Lists[tableName].Columns.GetAsync();

            return graphColumns;
        }

        public async Task GetOrUpdateManager<T>(EntityManager<T> manager) where T : IEntity, new()
        {
            var gc = GetGraphServiceClientApplication();

            var selectedSite = await gc.Sites[SiteId].GetAsync();

            var myList = await gc.Sites[selectedSite.Id].Lists[manager.TableName].GetAsync();

            if (myList.LastModifiedDateTime != null && lastSeenModifiedDateTime >= myList.LastModifiedDateTime)
            {
                Debug.WriteLine("No change. No Update");
                return;
            }

            var graphItemInList = await gc.Sites[selectedSite.Id].Lists[manager.TableName].Items.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Expand = new string[] { "fields($select=*)" };
            });

            var serverSidePresentIds = new List<string>();

            foreach (var graphItem in graphItemInList.Value)
            {
                var item = new T();
                var properties = typeof(T).GetProperties();

                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<MsListColumn>();
                    if (attribute != null && graphItem.Fields != null)
                    {
                        var columnName = attribute.Name;
                        if (columnName.ToLower() == "id")
                        {
                            property.SetValue(item, graphItem.Fields.Id);
                        }
                        else if (graphItem.Fields.AdditionalData.TryGetValue(columnName, out var value))
                        {
                            var propertyType = property.PropertyType;

                            if (propertyType == typeof(string))
                            {
                                property.SetValue(item, value as string);
                            }
                            else if (propertyType == typeof(decimal))
                            {
                                property.SetValue(item, Convert.ToDecimal(value));
                            }
                            else if (propertyType == typeof(List<string>))
                            {
                                property.SetValue(item, (value as string)?.Split("\n").ToList());
                            }
                            else if (propertyType == typeof(ObservableCollection<string>))
                            {
                                property.SetValue(item, new ObservableCollection<string>((value as string)?.Split("\n").ToList()));
                            }
                            else
                            {
                                property.SetValue(item, Convert.ChangeType(value, propertyType));
                            }
                        }
                    }
                }
                item.WebUrl = graphItem.WebUrl.Substring(0, graphItem.WebUrl.LastIndexOf('/')) + $"/DispForm.aspx?ID={item.Id}";
                serverSidePresentIds.Add(item.Id);
                manager.AddOrUpdate(item);
            }
            manager.DeleteAllExcept(serverSidePresentIds);
            lastSeenModifiedDateTime = myList.LastModifiedDateTime.Value;
            Debug.WriteLine("Loaded");
        }


        public async Task<ListItemCollectionResponse?> GetEntriesForTable(string tableName)
        {
            var gc = GetGraphServiceClientApplication();

            var selectedSite = await gc.Sites[SiteId].GetAsync();

            var myList = await gc.Sites[selectedSite.Id].Lists[tableName].GetAsync();

            var graphItemInList = await gc.Sites[selectedSite.Id].Lists[tableName].Items.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Expand = new string[] { "fields($select=*)" };
            });
           
            return graphItemInList;
        }

        private GraphServiceClient GetGraphServiceClientApplication()
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,

            };

            //// https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
            Configuration["AZURE_TENANT_ID"], Configuration["AZURE_CLIENT_ID"], Configuration["AZURE_CLIENT_SECRET"], options);

            //var token = clientSecretCredential.GetToken(new TokenRequestContext(scopes: scopes));
            //Console.WriteLine(token);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            return graphClient;
        }
    }
}

using LIM.EntityServices.Helpers;
using LIM.Models;
using Microsoft.Graph.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace LIM.EntityServices
{
    public class EntityManager<T> : IDictionary<string, T>, INotifyCollectionChanged, INotifyPropertyChanged where T : IEntity
    {
        private List<EntityStateWrapper<T>> Items = new List<EntityStateWrapper<T>>();
        private readonly object _syncRoot = new object();

        private Dictionary<string, List<string>> Choices = new Dictionary<string, List<string>>();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool Changed { get; private set; } = true;

        public EntityManager(string tableName, string fileName)
        {
            TableName = tableName;
            FileName = fileName;
        }

        public List<T> GetLocalyChangedEntries()
        {
            return Items.Where(i => i.Updated && i.Entity.Id != IEntity.NEW_ID_STR).Select(i => i.Entity).ToList();
        }

        public List<T> GetEntriesThatRequireLogEntries()
        {
            return Items.Where(i => i.RequiresLogEntry).Select(i => i.Entity).ToList();
        }

        public List<T> GetLocalyNewEntries()
        {
            return Items.Where(i => i.Updated && i.Entity.Id == IEntity.NEW_ID_STR).Select(i => i.Entity).ToList();
        }

        public void SetUpdated(T entity, bool value)
        {
            Items.Single(i => i.Entity.Equals(entity)).Updated = value;
            if (value)
            {
                NotifyChange(null);
            }
        }

        internal bool IsUpdated(InventoryItem inventoryItem)
        {
            if (!Items.Any(i => i.Entity.Equals(inventoryItem))) return false;
            return Items.Single(i => i.Entity.Equals(inventoryItem)).Updated;
        }


        public void SetRequiresLogEntry(T entity, bool value)
        {
            Items.Single(i => i.Entity.Equals(entity)).RequiresLogEntry = value;
        }



        public bool TryLock(T entity)
        {
            var item = Items.SingleOrDefault(i => i.Entity.Equals(entity));
            if (item == null) return false;
            item.LockedSince = DateTime.Now;
            return true;
        }

        public bool TryRelease(T entity)
        {
            var item = Items.SingleOrDefault(i => i.Entity.Equals(entity));
            if (item == null) return false;
            item.LockedSince = null;
            return true;
        }

        public void Save()
        {
            lock (_syncRoot)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var saveData = new EntityManagerData<T>()
                {
                    Items = Items,
                    Choices = Choices
                };

                var json = JsonSerializer.Serialize(saveData, options);
                File.WriteAllText(FileName, json);
                Changed = false;
            }
        }

        public bool TryLoad()
        {
            lock (_syncRoot)
            {
                if (!File.Exists(FileName))
                {
                    return false;
                }

                try
                {
                    var json = File.ReadAllText(FileName);
                    if (string.IsNullOrEmpty(json))
                    {
                        return false;
                    }

                    var loadedData = JsonSerializer.Deserialize<EntityManagerData<T>>(json, new JsonSerializerOptions()
                    {
                        IncludeFields = false
                    });
                    Items = loadedData.Items;
                    Choices = loadedData.Choices;
                    return true;
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is UnauthorizedAccessException || ex is IOException || ex is JsonException)
                {
                    return false;
                }
            }
        }

        private void NotifyChange(object sender)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                CollectionChanged?.Invoke(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
            Changed = true;
        }

        internal void AddOrUpdate(T item, bool changeSourceIsLocal = false)
        {
            lock (_syncRoot)
            {
                if(item.Id == IEntity.NEW_ID_STR && changeSourceIsLocal)
                {
                    Items.Add(new EntityStateWrapper<T>(item));
                    NotifyChange(item);
                    return;
                }
                var existingItemWrapper = Items.FirstOrDefault(i => i.Entity.Id == item.Id);
                if (existingItemWrapper == null)
                {
                    Items.Add(new EntityStateWrapper<T>(item));
                    NotifyChange(item);
                }
                else
                {
                    var existingItem = existingItemWrapper.Entity;

                    if ((existingItemWrapper.LockedSince != null &&
                        (DateTime.Now - existingItemWrapper.LockedSince) < TimeSpan.FromMinutes(30)) 
                        || existingItemWrapper.Updated)
                    {
                        // Could be still open or just updated. Not updating
                        return;
                    }

                    var updated = false;

                    foreach (var property in typeof(T).GetProperties())
                    {
                        var newValue = property.GetValue(item);
                        var oldValue = property.GetValue(existingItem);

                        if (!Equals(oldValue, newValue))
                        {
                            property.SetValue(existingItem, newValue);
                            updated = true;
                        }
                    }

                    if (updated)
                    {
                        if(changeSourceIsLocal) existingItemWrapper.Updated = true;
                        NotifyChange(item);
                    }
                }
            }
        }

        internal void DeleteAllExcept(List<string> serverSidePresentIds)
        {
            Items.Where(d => !serverSidePresentIds.Contains(d.Entity.Id) && d.Entity.Id != IEntity.NEW_ID_STR).ToList().ForEach(x => Items.Remove(x));
            NotifyChange(this);
        }

        internal void AddOrUpdateChoices(string? name, List<string>? choices)
        {
            if (name == null || choices == null) return;
            if (Choices.ContainsKey(name)) Choices[name] = choices;
            else Choices.Add(name, choices);
        }

        internal List<string> GetChoices(string v)
        {
            if (!Choices.ContainsKey(v)) return new List<string>();
            return Choices[v];
        }


        #region IDictionary

        public string TableName { get; }
        public string FileName { get; }

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public ICollection Keys => Items.Select(i => i.Entity.Id).ToList();

        public ICollection Values => Items.Select(i => i.Entity).ToList();

        public int Count => Items.Count;

        public bool IsSynchronized => false;

        public object SyncRoot => _syncRoot;

        ICollection<string> IDictionary<string, T>.Keys => throw new NotImplementedException();

        ICollection<T> IDictionary<string, T>.Values => throw new NotImplementedException();

        public T this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Clear()
        {
            Items.Clear();
        }

        public bool Contains(object key)
        {
            return Items.Any(i => i.Entity.Id == (string)key);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return (IDictionaryEnumerator)Items.Select(i => i.Entity).GetEnumerator();
        }


        

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.Select(i => i.Entity).GetEnumerator();
        }

        public void Add(string key, T value)
        {
            if (key != value.Id) throw new ArgumentException("Key id must match value");
            AddOrUpdate(value);
        }

        public bool ContainsKey(string key)
        {
            return Items.Any(i => i.Entity.Id.Equals(key));
        }

        public bool Remove(string key)
        {
            return Items.Remove(Items.Single(s => s.Entity.Id == key));
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
        {
            var item = Items.SingleOrDefault(s => s.Entity.Id == key);
            value = default(T);
            if (item == null) return false;
            value = item.Entity;
            return true;
        }

        public void Add(KeyValuePair<string, T> item)
        {
            if (item.Key != item.Value.Id) throw new ArgumentException("Key id must match value");
            AddOrUpdate(item.Value);
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return Items.Any(i => i.Entity.Id == item.Key);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            return Items.Remove(Items.Single(s => s.Entity.Id == item.Key));
        }

        IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
        {
            return Items.Select(x => new KeyValuePair<string, T>(x.Entity.Id, x.Entity)).GetEnumerator();
        }

        
        #endregion
    }
}

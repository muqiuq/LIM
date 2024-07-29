using LIM.EntityServices;
using LIM.EntityServices;
using LIM.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace LIM.DebugConsole
{
    internal class Program
    {
        private static IConfigurationRoot AppConfig;

        static void Main(string[] args)
        {
            AppConfig = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            //var listGraphService = new LIM.EntityServices.ListGraphService(AppConfig);

            string fileName = "inventoryItems.json";

            //var manager = new EntityManager<InventoryItem>("IT Lagerliste");
            //manager.TryLoad(fileName);

            //var task = listGraphService.GetOrUpdateManager(manager);
            //task.Wait();

            //manager.Save(fileName);

            //Console.WriteLine(manager);

            
        }
    }
}

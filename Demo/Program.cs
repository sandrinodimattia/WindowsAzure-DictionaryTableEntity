using System;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace DictionaryEntityDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            string accountName = "myaccount";
            string accountKey = "mykey";

            CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), false);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CreateCustomers(tableClient);
            CreateCustomerMetadata(tableClient);
            ReadCustomerMetadata(tableClient);

            Console.Read();
        }

        static void CreateCustomers(CloudTableClient tableClient)
        {
            Console.WriteLine("Creating customers...");

            CloudTable customersTable = tableClient.GetTableReference("customers");
            customersTable.CreateIfNotExists();

            customersTable.Execute(TableOperation.Insert(new Customer()
            {
                PartitionKey = "",
                RowKey = "MSFT",
                Name = "Microsoft"
            }));

            customersTable.Execute(TableOperation.Insert(new Customer()
            {
                PartitionKey = "",
                RowKey = "GOOG",
                Name = "Google"
            }));

            Console.WriteLine("Done. Press ENTER to create the customer metadata.");
            Console.ReadLine();
        }

        static void CreateCustomerMetadata(CloudTableClient tableClient)
        {
            Console.WriteLine("Creating customers metadata...");

            CloudTable customersMetadataTable = tableClient.GetTableReference("customersmetadata");
            customersMetadataTable.CreateIfNotExists();

            var msftAddress1 = new DictionaryTableEntity();
            msftAddress1.PartitionKey = "MSFT";
            msftAddress1.RowKey = "ADDRESS-" + Guid.NewGuid().ToString("N").ToUpper();
            msftAddress1.Add("city", "Seattle");
            msftAddress1.Add("street", "111 South Jackson");

            var msftWebsite1 = new DictionaryTableEntity();
            msftWebsite1.PartitionKey = "MSFT";
            msftWebsite1.RowKey = "WEBSITE-" + Guid.NewGuid().ToString("N").ToUpper();
            msftWebsite1.Add("url", "http://www.microsoft.com");

            var msftWebsite2 = new DictionaryTableEntity();
            msftWebsite2.PartitionKey = "MSFT";
            msftWebsite2.RowKey = "WEBSITE-" + Guid.NewGuid().ToString("N").ToUpper();
            msftWebsite2.Add("url", "http://www.windowsazure.com");

            var batch = new TableBatchOperation();
            batch.Add(TableOperation.Insert(msftAddress1));
            batch.Add(TableOperation.Insert(msftWebsite1));
            batch.Add(TableOperation.Insert(msftWebsite2));
            customersMetadataTable.ExecuteBatch(batch);

            Console.WriteLine("Done. Press ENTER to read the customer metadata.");
            Console.ReadLine();
        }

        static void ReadCustomerMetadata(CloudTableClient tableClient)
        {
            Console.WriteLine("Reading customers metadata:");

            CloudTable customersMetadataTable = tableClient.GetTableReference("customersmetadata");
            customersMetadataTable.CreateIfNotExists();

            TableQuery<DictionaryTableEntity> query =
                new TableQuery<DictionaryTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "MSFT"));

            var microsoftMetadata = customersMetadataTable.ExecuteQuery(query);
            foreach (var record in microsoftMetadata)
            {
                Console.WriteLine("Processing: {0}", record.RowKey);

                if (record.RowKey.StartsWith("ADDRESS"))
                {
                    Console.WriteLine(" > This is an address");
                    Console.WriteLine("   - Street: {0}", record["street"].StringValue);
                    Console.WriteLine("   - City: {0}", record["city"].StringValue);
                }
                else if (record.RowKey.StartsWith("WEBSITE"))
                {
                    Console.WriteLine(" > This is an website");
                    Console.WriteLine("   - Url: {0}", record["url"].StringValue);
                }

                Console.WriteLine(" > All properties");
                foreach (var property in record)
                    Console.WriteLine("   - {0}: {1}", property.Key, property.Value.StringValue);
            }

            Console.ReadLine();
        }
    }
}

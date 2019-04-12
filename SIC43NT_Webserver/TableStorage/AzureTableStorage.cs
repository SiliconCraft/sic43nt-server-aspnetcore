using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIC43NT_Webserver.TableStorage
{
    public interface IAzureTableStorage
    {
        //void CreateBook(EmployeeInfoRecord bk);
        TagAccessRec GetTagAccessRec(string pKey, string rKey);
        void UpdateTagAccessRec(TagAccessRec tar);
    }

    public class AzureTableStorage : IAzureTableStorage
    {
        //2.
        CloudStorageAccount storageAccount;
        //3.
        CloudTableClient tableClient;
        //4.
        IConfiguration configs;
        //5.
        private CloudTable tableSIC43NT;

        public AzureTableStorage(IConfiguration c)
        {
            this.configs = c;
            if (c != null)
            {
                var connStr = c["ConnectionStrings:DefaultConnection"];
                if (connStr != null)
                {
                    storageAccount = CloudStorageAccount.Parse(connStr);
                    tableClient = storageAccount.CreateCloudTableClient();
                    CloudTable table = tableClient.GetTableReference("TableSIC43NT");
                    table.CreateIfNotExists();
                }
                else
                {
                    throw new System.ArgumentException("Connection String cannot be null", "connStr");
                }
            }
            else
            {
                throw new System.ArgumentException("Connection String cannot be null", "c");
            }
        }
        //6.
        public TagAccessRec GetTagAccessRec(string pKey, string rKey)
        {
            tableSIC43NT = tableClient.GetTableReference("TableSIC43NT");
            TagAccessRec entity = null;

            TableOperation tableOperation = TableOperation.Retrieve<TagAccessRec>(pKey, rKey);
            entity = tableSIC43NT.Execute(tableOperation).Result as TagAccessRec;
            return entity;

            /*
            Random rnd = new Random();
            empy_rec.EmployeeID = rnd.Next(100);
            empy_rec.RowKey = empy_rec.EmployeeID.ToString();
            empy_rec.PartitionKey = empy_rec.CompanyID.ToString();
            CloudTable table = tableClient.GetTableReference("Book");
            TableOperation insertOperation = TableOperation.Insert(empy_rec);
            table.Execute(insertOperation);
            */
            //return new TagAccessRec();
        }

        public void UpdateTagAccessRec(TagAccessRec tar)
        {
            tableSIC43NT = tableClient.GetTableReference("TableSIC43NT");
            TagAccessRec updateEntity;
            TableOperation tableOperation = TableOperation.Retrieve<TagAccessRec>(tar.PartitionKey, tar.RowKey);
            updateEntity = tableSIC43NT.Execute(tableOperation).Result as TagAccessRec;


            if (updateEntity != null)
            {
                //Change the description
                updateEntity.TimeStampServer = updateEntity.TimeStampServer + 10;

                // Create the InsertOrReplace TableOperation
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(updateEntity);

                // Execute the operation.
                tableSIC43NT.Execute(insertOrReplaceOperation);
                //Console.WriteLine("Entity was updated.");
            }

        }

    }
}

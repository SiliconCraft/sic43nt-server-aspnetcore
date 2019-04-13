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
        bool UpdateTagAccessRec(TagAccessRec tar);
        bool ConnectionReady();
    }

    public class AzureTableStorage : IAzureTableStorage
    {
        private CloudStorageAccount storageAccount;
        private CloudTableClient tableClient;
        private IConfiguration configs;
        private CloudTable tableSIC43NT;
        private bool connection_ready = false;

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
                    try
                    {
                        table.CreateIfNotExists();
                        connection_ready = true;
                    }
                    catch (Exception ex)
                    {
                        connection_ready = false;
                    }

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
        
        public bool ConnectionReady()
        {
            return connection_ready;
        }

        // Read Tag record from Azure Table
        public TagAccessRec GetTagAccessRec(string pKey, string rKey)
        {
            tableSIC43NT = tableClient.GetTableReference("TableSIC43NT");
            TagAccessRec entity = null;

            TableOperation tableOperation = TableOperation.Retrieve<TagAccessRec>(pKey, rKey);
            entity = tableSIC43NT.Execute(tableOperation).Result as TagAccessRec;
            return entity;
        }

        // Update Tag record into Azure Table
        public bool UpdateTagAccessRec(TagAccessRec tar)
        {
            tableSIC43NT = tableClient.GetTableReference("TableSIC43NT");
            TagAccessRec updateEntity;
            TableOperation tableOperation = TableOperation.Retrieve<TagAccessRec>(tar.PartitionKey, tar.RowKey);
            updateEntity = tableSIC43NT.Execute(tableOperation).Result as TagAccessRec;

            // Confirm the existing of TagAccessRecord in TableSIC43NT
            if (updateEntity != null)
            {
                // Updated content with input (tar object)
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(tar);
                try
                {
                    tableSIC43NT.Execute(insertOrReplaceOperation);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

    }
}

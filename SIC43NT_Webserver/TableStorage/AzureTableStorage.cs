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
                        bool tbl_created = table.CreateIfNotExists();
                        if (tbl_created == true)
                        {
                            initial_TagAccessRec();
                        }
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
        
        private void initial_TagAccessRec()
        {
            // Insert first record random value.
            Random rand = new Random();
            byte[] rand_bytes = new byte[4];
            rand.NextBytes(rand_bytes);

            TagAccessRec tar = new TagAccessRec();
            tar.PartitionKey = "DemoSection";
            tar.RowKey = "39493_" +
                        rand_bytes[0].ToString("X2") +
                        rand_bytes[1].ToString("X2") +
                        rand_bytes[2].ToString("X2") +
                        rand_bytes[3].ToString("X2");
            tar.RollingCodeFailCount = 0;
            tar.TimeStampFailCount = 0;
            tar.SuccessCount = 0;
            tar.RollingCodeFailLastDateTime = DateTime.Now;
            tar.TimeStampFailLastDateTime = DateTime.Now;
            tar.SuccessLastDateTime = DateTime.Now;
            tar.SecretKey = "FFFFFF39493000000000";
            tar.TimeStampServer = 0;
            tar.RollingCodeServer = "0";
            InsertTagAccessRec(tar);
        }

        public bool ConnectionReady()
        {
            return connection_ready;
        }

        // Read Tag record from Azure Table
        public bool InsertTagAccessRec(TagAccessRec tar)
        {
            tableSIC43NT = tableClient.GetTableReference("TableSIC43NT");
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(tar);

            // Execute the operation.
            TableResult result = tableSIC43NT.Execute(insertOrMergeOperation);
            TagAccessRec insertedCustomer = result.Result as TagAccessRec;
            return true;
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

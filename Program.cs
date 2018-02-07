using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace DataFactory
{
    class Program
    {
        static void Main(string[] args)
        {   
            // Set variables
            string tenantID = "da67ef1b-ca59-4db2-9a8c-aa8d94617a16";
            string applicationId = "b7fd00c9-74b7-40db-9505-f8a899663e59";
            string authenticationKey = "phBWjqjTfa9qHv0WbozCfMwHlD5SFbD7ugL2R/JbqZY=";
            string subscriptionId = "61727f05-e5e1-4271-b29c-66b7d81a7729";
            string resourceGroup = "dilipazureprojectresourcegroup";
            string region = "EAST US";
            string dataFactoryName = "dilipdatafactoryy";

            // Specify the source Azure Blob information
            string storageAccount = "dilipazurestorageaccount";
            string storageKey = "usk5E1K7Sj2HfX2V/eZRq/hU5k/oZs0n6WgcixaYK+Im6IKGk0er7vFLTbv6yBe5rNdcEqqpHh1YvNUY3vugvw==";
            string inputBlobPath = "dilipazurecontainer/";
            string inputBlobName = "PATIENT.txt";

            // Specify the sink Azure SQL Database information
            string azureSqlConnString = "Server=tcp:dilipazureprojectdbserver.database.windows.net,1433;Initial Catalog=dilipazureprojectdb;Persist Security Info=False;User ID=dilreddy@dilipazureprojectdbserver.database.windows.net;Password=password143#;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";
            string azureSqlTableName = "dbo.Patient";
            string storageLinkedServiceName = "AzureStorageLinkedService";
            string sqlDbLinkedServiceName = "AzureSqlDbLinkedService";
            string blobDatasetName = "BlobDataset";
            string sqlDatasetName = "SqlDataset";
            string pipelineName = "Adfv2TutorialBlobToSqlCopyy";

            // Authenticate and create a data factory management client
            var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
            ClientCredential cc = new ClientCredential(applicationId, authenticationKey);
            AuthenticationResult result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
            ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
            var client = new DataFactoryManagementClient(cred) { SubscriptionId = subscriptionId };

            // Create a data factory
            Console.WriteLine("Creating a data factory " + dataFactoryName + "..."); Factory dataFactory = new Factory
            {
                Location = region,
                Identity = new FactoryIdentity()
            };
            client.Factories.CreateOrUpdate(resourceGroup, dataFactoryName, dataFactory); Console.WriteLine(SafeJsonConvert.SerializeObject(dataFactory, client.SerializationSettings));
            while (client.Factories.Get(resourceGroup, dataFactoryName).ProvisioningState == "PendingCreation")
            {
                System.Threading.Thread.Sleep(1000);
            }

            // Create an Azure Storage linked service
            Console.WriteLine("Creating linked service " + storageLinkedServiceName + "...");
            LinkedServiceResource storageLinkedService = new LinkedServiceResource(
            new AzureStorageLinkedService
            {
                ConnectionString = new SecureString("DefaultEndpointsProtocol=https;AccountName=" + storageAccount + ";AccountKey=" + storageKey)
            }
            );
            client.LinkedServices.CreateOrUpdate(resourceGroup, dataFactoryName, storageLinkedServiceName, storageLinkedService);
            Console.WriteLine(SafeJsonConvert.SerializeObject(storageLinkedService, client.SerializationSettings));

            // Create an Azure SQL Database linked service
            Console.WriteLine("Creating linked service " + sqlDbLinkedServiceName + "...");
            LinkedServiceResource sqlDbLinkedService = new LinkedServiceResource(
            new AzureSqlDatabaseLinkedService
            {
                ConnectionString = new SecureString(azureSqlConnString)
            }
            );
            client.LinkedServices.CreateOrUpdate(resourceGroup, dataFactoryName, sqlDbLinkedServiceName, sqlDbLinkedService);
            Console.WriteLine(SafeJsonConvert.SerializeObject(sqlDbLinkedService, client.SerializationSettings));

            // Create a Azure Blob dataset
            Console.WriteLine("Creating dataset " + blobDatasetName + "...");
            DatasetResource blobDataset = new DatasetResource(
            new AzureBlobDataset
            {
                LinkedServiceName = new LinkedServiceReference
                {
                    ReferenceName = storageLinkedServiceName
                },
                FolderPath = inputBlobPath,
                FileName = inputBlobName,
                Format = new TextFormat { ColumnDelimiter = "|" },
                Structure = new List<DatasetDataElement>
            {

                               new DatasetDataElement
            {
                Name = "ENTERPRISEID",
                Type = "String"
            },
            new DatasetDataElement
            {
                Name = "LAST_NAME",
                Type = "String"
            },
            new DatasetDataElement
            {
                Name = "FIRST_NAME",
                Type = "String"
            },
            new DatasetDataElement
            {
                Name = "DOB",
                Type = "DATE"
            },
            new DatasetDataElement
            {
                Name = "GENDER",
                Type = "String"
            },
            new DatasetDataElement
            {
                Name = "SSN",
                Type = "String"
            },
            new DatasetDataElement
            {
                Name = "ADDRESS1",
                Type = "String"
            },
            new DatasetDataElement
            {
                Name = "ZIP",
                Type = "String"
            },
            new DatasetDataElement
            {
                Name = "MRN",
                Type = "String"
            },
            new DatasetDataElement
            {
                Name = "CITY",
                Type = "String"
            },
            new DatasetDataElement
            {
                Name = "STATE",
                Type = "String"
            },
            new DatasetDataElement
            {
                Name = "PHONE",
                Type = "String"
            },
            new DatasetDataElement
            {
                Name = "EMAIL",
                Type = "String"
            },
            new DatasetDataElement
            {
                Name = "PRVDR_ID",
                Type = "String"
            }

            }
            }
            );
            client.Datasets.CreateOrUpdate(resourceGroup, dataFactoryName, blobDatasetName, blobDataset);
            Console.WriteLine(SafeJsonConvert.SerializeObject(blobDataset, client.SerializationSettings));

            // Create a Azure SQL Database dataset
            Console.WriteLine("Creating dataset " + sqlDatasetName + "...");
            DatasetResource sqlDataset = new DatasetResource(
            new AzureSqlTableDataset
            {
                LinkedServiceName = new LinkedServiceReference
                {
                    ReferenceName = sqlDbLinkedServiceName
                },
                TableName = azureSqlTableName
            }
            );
            client.Datasets.CreateOrUpdate(resourceGroup, dataFactoryName, sqlDatasetName, sqlDataset);
            Console.WriteLine(SafeJsonConvert.SerializeObject(sqlDataset, client.SerializationSettings));

            // Create a pipeline with copy activity
            Console.WriteLine("Creating pipeline " + pipelineName + "...");
            PipelineResource pipeline = new PipelineResource
            {
                Activities = new List<Activity>
                {
                new CopyActivity
                {
                Name = "CopyFromBlobToSQL", Inputs = new List<DatasetReference>
                {
                new DatasetReference()
                {
                ReferenceName = blobDatasetName
                }
                },
                Outputs = new List<DatasetReference>
                {
                new DatasetReference
                {
                ReferenceName = sqlDatasetName
                }
                },
                Source = new BlobSource { }, Sink = new SqlSink { }
                }
                }
            };
            client.Pipelines.CreateOrUpdate(resourceGroup, dataFactoryName, pipelineName, pipeline);
            Console.WriteLine(SafeJsonConvert.SerializeObject(pipeline, client.SerializationSettings));
            // Create a pipeline run
            Console.WriteLine("Creating pipeline run...");
            CreateRunResponse runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(resourceGroup, dataFactoryName, pipelineName).Result.Body;
            Console.WriteLine("Pipeline run ID: " + runResponse.RunId);

            // Monitor the pipeline run
            Console.WriteLine("Checking pipeline run status...");
            PipelineRun pipelineRun;
            while (true)
            {
                pipelineRun = client.PipelineRuns.Get(resourceGroup, dataFactoryName, runResponse.RunId);
                Console.WriteLine("Status: " + pipelineRun.Status);
                if (pipelineRun.Status == "InProgress")
                    System.Threading.Thread.Sleep(15000);
                else
                    break;
            }

            // Check the copy activity run details
            Console.WriteLine("Checking copy activity run details...");
            List<ActivityRun> activityRuns = client.ActivityRuns.ListByPipelineRun(
            resourceGroup, dataFactoryName, runResponse.RunId, DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(10)).ToList();
            if (pipelineRun.Status == "Succeeded")
            {
                Console.WriteLine(activityRuns.First().Output);
            }
            else
                Console.WriteLine(activityRuns.First().Error);
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}


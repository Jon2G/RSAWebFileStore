using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebClient.Models;

namespace WebClient.Clients
{
    public class DynamoClient
    {
        private bool IsInited = false;
        private readonly IDynamoDBContext DDBContext;
        private readonly IAmazonDynamoDB AmazonDynamoDB;

        public DynamoClient(IAmazonDynamoDB amazonDynamoDB)
        {
            AmazonDynamoDB = amazonDynamoDB;
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(DFile)] = new Amazon.Util.TypeMapping(typeof(DFile), nameof(DFile));
            var client = (AmazonDynamoDBClient)AmazonDynamoDB;
            DDBContext = new DynamoDBContext(client);
        }
        private async Task Init()
        {
            var request = new ListTablesRequest
            {
                Limit = 1
            };
            var response = await AmazonDynamoDB.ListTablesAsync(request);
            var results = response.TableNames;
            if (!results.Contains(nameof(ClientKey)))
            {
                var createRequest = new CreateTableRequest
                {
                    TableName = nameof(ClientKey),
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                        {
                            AttributeName = nameof(DFile.Id),
                            AttributeType =ScalarAttributeType.S
                        }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = nameof(DFile.Id),
                            KeyType =KeyType.HASH //Partition key,
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 2,
                        WriteCapacityUnits = 2
                    }
                };
                await AmazonDynamoDB.CreateTableAsync(createRequest);
                IsInited = true;
            }
        }
        public async Task<bool> Save(ClientKey key)
        {
            try
            {
                if (!IsInited)
                {
                    await Init();
                }
                await this.DDBContext.SaveAsync(key);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}

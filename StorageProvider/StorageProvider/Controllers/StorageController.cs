using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Kit;
using Kit.Services.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StorageProvider.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StorageProvider.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StorageController : ControllerBase
    {
        IDynamoDBContext DDBContext { get; set; }
        private readonly ILogger<StorageController> _logger;
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private bool IsInited = false;
        public StorageController(ILogger<StorageController> logger, IAmazonDynamoDB amazonDynamoDb)
        {
            _logger = logger;
            _amazonDynamoDb = amazonDynamoDb;
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(DFile)] = new Amazon.Util.TypeMapping(typeof(DFile), nameof(DFile));
            var client = (AmazonDynamoDBClient)_amazonDynamoDb;
            DDBContext = new DynamoDBContext(client);
        }
        private async Task Init()
        {
            var request = new ListTablesRequest
            {
                Limit = 10
            };
            var response = await _amazonDynamoDb.ListTablesAsync(request);
            var results = response.TableNames;
            if (!results.Contains(nameof(DFile)))
            {
                var createRequest = new CreateTableRequest
                {
                    TableName = nameof(DFile),
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
                await _amazonDynamoDb.CreateTableAsync(createRequest);
            }
        }
        [HttpGet]
        public Response Get()
        {
            return new Response(APIResponseResult.OK, $"Hello there it's,{Kit.DateExtensions.MexicoCityCurrentDateTime()}");
        }
        [HttpPost("Upload/{fileName}/{fileExtension}")]
        public async Task<Response<DFileInfo>> Upload([FromBody] byte[] FileData, string fileName, string fileExtension)
        {
            try
            {
                if (!IsInited)
                {
                    await Init();
                }
                if (FileData is null)
                {
                    return new Response<DFileInfo>(APIResponseResult.INVALID_REQUEST, "File data can't be empty");
                }
                double sizeInMbs = FileData.LongLength.ToSize(BytesConverter.SizeUnits.MB);
                if (sizeInMbs > 2)
                {
                    return new Response<DFileInfo>(APIResponseResult.INVALID_REQUEST, "Max file size is 2Mb");
                }
                DFile fileInfo = new DFile()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedTimestamp = DateExtensions.MexicoCityCurrentDateTime(),
                    Extension = fileExtension,
                    FileName = fileName,
                    SizeInMbs = sizeInMbs,
                    Data = FileData
                };
                _logger.LogInformation($"Saving file with id {fileInfo.Id}");
                await DDBContext.SaveAsync(fileInfo);

                var response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = fileInfo.Id.ToString(),
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
                if (response.StatusCode != (int)HttpStatusCode.OK)
                {
                    var responseResult = new ResponseResult((HttpStatusCode)response.StatusCode, "An unexpected error has ocurred, if you keep seeing this please raise a ticket", "");
                    return responseResult.ToResponse<Response<DFileInfo>>();
                }
                return new Response<DFileInfo>(APIResponseResult.OK, "DONE", fileInfo);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Unexpected error");
                return new Response<DFileInfo>(APIResponseResult.INTERNAL_ERROR, ex.Message);
            }
        }
        [HttpGet("Download/{fileId}")]
        public async Task<Response<DFile>> Download(string fileId)
        {
            try
            {
                if (!IsInited)
                {
                    await Init();
                }

                if (string.IsNullOrEmpty(fileId))
                {
                    return new Response<DFile>(APIResponseResult.INVALID_REQUEST, "Id of file can´t be empty");
                }

                DFile file = await DDBContext.LoadAsync<DFile>(fileId);
                if (file is null)
                {
                    return new Response<DFile>(APIResponseResult.NOT_READ, "File not found!");
                }

                return new Response<DFile>(APIResponseResult.OK, "Done", file);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Unexpected error");
                return new Response<DFile>(APIResponseResult.INTERNAL_ERROR, ex.Message);
            }
        }
    }
}

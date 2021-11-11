using System;
using Amazon.DynamoDBv2.DataModel;

namespace WStorageProvider.Models
{
    [DynamoDBTable("DFile")]
    public class DFileInfo
    {
        [DynamoDBHashKey]
        public string Id { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public double SizeInMbs { get; set; }
        public DFileInfo()
        {

        }
    }
}

using Amazon.DynamoDBv2.DataModel;
using System;
using System.IO;

namespace StorageProvider.Models
{
    [DynamoDBTable("DFile")]
    public class DFile : DFileInfo
    {
        public byte[] Data { get; set; }
        public DFile()
        {

        }
    }
}

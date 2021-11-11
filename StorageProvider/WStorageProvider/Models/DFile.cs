using Amazon.DynamoDBv2.DataModel;

namespace WStorageProvider.Models
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

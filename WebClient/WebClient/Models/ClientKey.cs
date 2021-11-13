using Amazon.DynamoDBv2.DataModel;

namespace WebClient.Models
{
    [DynamoDBTable("ClientKey")]
    public class ClientKey
    {
        [DynamoDBHashKey]
        public string Id { get; set; }
        public string Key { get; set; }
        public ClientKey()
        {

        }
    }
}

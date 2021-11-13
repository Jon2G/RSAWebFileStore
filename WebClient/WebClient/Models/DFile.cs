using System;

namespace WebClient.Models
{
    public class DFile
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public double SizeInMbs { get; set; }

        public DFile()
        {

        }
    }
}

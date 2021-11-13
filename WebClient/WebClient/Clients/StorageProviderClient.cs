using Kit;
using Kit.Services.Web;
using System.Threading.Tasks;
using WebClient.Models;

namespace WebClient.Clients
{
    public class StorageProviderClient
    {
        private const string URL = "http://wstorageprovider-prod.us-east-2.elasticbeanstalk.com/Storage";
        public static async Task<DFile> Upload(DFileData file)
        {
            WebService service = new WebService(URL);
            ResponseResult result = await service.PostAsBody(file.Data, "Upload", file.FileName, file.Extension??".txt");
            Response<DFile> response = result.ToResponse<Response<DFile>>();
            if (response.ResponseResult == APIResponseResult.OK)
            {
                return response.Extra;
            }
            return null;
        }
        public static async Task<DFileData> Download(string Id)
        {
            WebService service = new WebService(URL);
            ResponseResult result = await service.GET("Download", Id);
            Response<DFileData> response = result.ToResponse<Response<DFileData>>();
            if (response.ResponseResult == APIResponseResult.OK)
            {
                return response.Extra;
            }
            return null;
        }
    }
}
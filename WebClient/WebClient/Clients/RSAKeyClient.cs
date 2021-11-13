using Kit.Services.Web;
using System.Threading.Tasks;
using Kit;
using System.Security.Cryptography;
using WebClient.Models;
using System;
using Newtonsoft.Json.Linq;

namespace WebClient.Clients
{
    public class RSAKeyClient
    {
        private const string Url = "http://rsaregister-prod.us-east-1.elasticbeanstalk.com/api/Register/";
        public RSAKeyClient()
        {

        }
        public static async Task<ClientKey> GetPrivateKeys(string id)
        {
            WebService request = new WebService(Url);
            var response = await request.GET("SendKeys",id);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Response))
            {
                JObject result = JObject.Parse(response.Response);
                ClientKey clientKey = new ClientKey()
                {
                    Id = result["id"].Value<string>(),
                    Key = result["key"].Value<string>()
                };
                return clientKey;
            }
            return null;
        }
        public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        public static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }
        }
        public static async Task<bool> Encrypt(DFileData file, ClientKey key)
        {
            try
            {
                await Task.Yield();
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.FromXmlString(key.Key);
                file.Data = RSAEncrypt(file.Data, provider.ExportParameters(false), false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static async Task<bool> Decrypt(DFileData file, ClientKey key)
        {
            try
            {
                await Task.Yield();
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.FromXmlString(key.Key);
                file.Data = RSADecrypt(file.Data, provider.ExportParameters(true), false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static async Task<ClientKey> Register()
        {
            WebService request = new WebService(Url);
            var response = await request.GET("RegisterClient");
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Response))
            {
                JObject result = JObject.Parse(response.Response);
                ClientKey clientKey = new ClientKey()
                {
                    Id = result["id"].Value<string>(),
                    Key = result["key"].Value<string>()
                };
                return clientKey;
            }
            return null;
        }

    }
}

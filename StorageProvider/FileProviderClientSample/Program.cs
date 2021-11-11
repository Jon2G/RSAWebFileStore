using FileProviderClientSample.Models;
using Kit;
using Kit.Services.Web;
using Syroot.Windows.IO;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileProviderClientSample
{
    internal class Program
    {
        private const string URL = "https://bald6s2l1a.execute-api.us-east-2.amazonaws.com/Prod/Storage";
        static void Main(string[] args)
        {
            ShowMenu().Wait();
        }
        private static async Task ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("1) Subir un archivo");
            Console.WriteLine("2) Descargar un archivo");
            Console.WriteLine("3) Salir");
            int seleccion = Convert.ToInt32(Console.ReadLine());
            switch (seleccion)
            {
                case 1:
                    await SubirArchivo(SeleccionarArchivo());
                    await ShowMenu();
                    break;
                case 2:
                    Console.Clear();
                    Console.WriteLine("Ingrese el id de archivo:");
                    await DescargarArchivo(Console.ReadLine());
                    await ShowMenu();
                    break;
                case 3:
                    return;
            }
        }
        private static async Task SubirArchivo(FileInfo file)
        {
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fileStream.Position = 0;
                    await fileStream.CopyToAsync(memoryStream);
                    WebService service = new WebService(URL);
                    ResponseResult result = await service.PostAsBody(memoryStream.ToArray(), "Upload", file.Name, file.Extension);
                    var response = result.ToResponse<Response<DFile>>();
                    if (response.ResponseResult == APIResponseResult.OK)
                    {
                        Console.WriteLine($"OK Subido correctamente - [{response.Extra.Id}]");
                        Console.ReadKey();
                        return;
                    }
                    else
                    {
                        Console.WriteLine(response.Message.ToString());
                        Console.ReadKey();
                    }
                }
            }
        }
        private static async Task DescargarArchivo(string fileId)
        {
            Console.Clear();
            WebService service = new WebService(URL);
            ResponseResult result = await service.GET("Download", fileId);
            var response = result.ToResponse<Response<DFile>>();
            if (response.ResponseResult == APIResponseResult.OK)
            {
                DFile dfile = response.Extra;
                FileInfo file = new FileInfo(Path.Combine(KnownFolders.Downloads.Path, dfile.FileName));
                using (FileStream fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (MemoryStream ms = new MemoryStream(dfile.Data))
                    {
                        await ms.CopyToAsync(fs);
                    }
                }
                Console.WriteLine($"OK salvado en descargas - [{response.Extra.Id}]");
                Console.ReadKey();
                return;
            }
            else
            {
                Console.WriteLine(response.ToString());
                Console.ReadKey();
            }
        }
        private static FileInfo SeleccionarArchivo()
        {
            Console.Clear();
            Console.WriteLine("Ingrese la ruta completa del archivo:");
            string path = Console.ReadLine();
            FileInfo file = new FileInfo(path);
            if (!file.Exists)
            {
                Console.WriteLine($"No se encontro el archivo:{file.FullName}");
                Console.ReadKey();
                return SeleccionarArchivo();
            }
            return file;
        }
    }
}

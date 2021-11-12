using GoldenToolKit;
using System;
using System.Threading.Tasks;

namespace RegisterClientSample
{
    internal class Program
    {
        private const string Url = "http://rsaregister-prod.us-east-1.elasticbeanstalk.com/api/Register/";
        static void Main(string[] args)
        {
            Menu().Wait();
        }
        private static async Task Menu()
        {
            int opcion;
            Console.Clear();
            Console.WriteLine("1) Resgistrar usuario");
            Console.WriteLine("2) Obtener llave");
            Console.WriteLine("3) Salir");
            opcion = Convert.ToInt32(Console.ReadLine());
            switch (opcion)
            {
                case 1: 
                await Register();
                    await Menu();
                    break;
                case 2:
                    await GetPrivateKeys();
                    await Menu();
                    break;
                case 3:
                return;
            }      
        }

        public static async Task GetPrivateKeys()
        {
            Console.Clear();
            Console.WriteLine("Ingrese su id de cliente:");
            string id=Console.ReadLine();
            string callUrl= $"{Url}SendKeys/{id}";
            WebRequest request= new WebRequest();
            string result=await request.ExecuteMethodAsync(callUrl);
            Console.WriteLine(result);
            Console.ReadKey();
        }

        public static async Task Register()
        {
            Console.Clear();
            Console.WriteLine("Regisrando cliente ...");
            string callUrl= $"{Url}RegisterClient";
            WebRequest request= new WebRequest();
            string result=await request.ExecuteMethodAsync(callUrl);
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}

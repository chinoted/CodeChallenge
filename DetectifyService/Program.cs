using System;
using System.ServiceModel;
using DetectifyService.Services;

namespace DetectifyService
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(ScreenshotService)))
            {
                host.Open();
                Console.WriteLine("Server is open...");
                Console.WriteLine("<Press enter to close server>");
                Console.ReadLine();
            }
        }
    }
}

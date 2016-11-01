using System;
using System.IO;
using System.ServiceModel;
using DetectifyServiceLibrary;

namespace DetectifyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var channelFactory = new ChannelFactory<IScreenshotService>("DetectifyClientConfig");
            var proxy = channelFactory.CreateChannel();

            while (true)
            {
                Console.WriteLine("Press S and Enter to Save a screenshot from a URL");
                Console.WriteLine("Press D and Enter to Download saved screenshots");
                var input = Console.ReadLine();
                switch (input.ToUpper())
                {
                    case "S":
                        SaveScreenshot(proxy);
                        break;
                    case "D":
                        DownloadFiles(proxy);
                        break;
                    default:
                        Console.WriteLine("Your input has no command");
                        break;
                }
            }
        }

        private static void SaveScreenshot(IScreenshotService proxy)
        {
            Console.WriteLine("Enter your desired URL:s separated with ';' (eg. www.web.com;www.mail.com)");
            var input = Console.ReadLine();
            proxy.SaveScreenshots(input);
            Console.WriteLine("Your screenshots was saved.");
        }

        private static void DownloadFiles(IScreenshotService proxy)
        {
            var localFileName = "";
            var validPath = false;

            while (!validPath)
            {
                Console.WriteLine("Enter your desired path to save the screenshots: ");
                localFileName = Console.ReadLine();
                try
                {
                    Directory.CreateDirectory(localFileName);
                    validPath = true;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine("The path you entered is not valid");
                    validPath = false;
                }
            }
            var request = new FileDownloadMessage();
            request.FileMetaData = new FileMetaData(localFileName, "", "");

            Console.WriteLine("Do you want to download all screenshots? Y/N");
            var input = Console.ReadLine();
            var downloadAll = input.ToUpper() == "Y";
            var url = "";
            if (!downloadAll)
            {
                Console.WriteLine("Write the URL of which you want to download: ");
                url = Console.ReadLine();
                request.FileMetaData.Url = url;
            }

            try
            {
                using (FileDownloadReturnMessage response = downloadAll ? proxy.DownloadAllFiles(request) : proxy.DownloadFile(request))
                {
                    if (response != null && response.FileByteStream != null && !String.IsNullOrWhiteSpace(response.DownloadedFileMetadata.LocalFileName))
                    {
                        SaveFile(response.FileByteStream, response.DownloadedFileMetadata.LocalFileName);
                        Console.WriteLine("The download was successful.");
                    }
                    else
                    {
                        Console.WriteLine("Couldn't find the specified url");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured with the download.");
            }
        }

        private static void SaveFile(Stream saveFile, string localFilePath)
        {
            const int bufferSize = 65536;

            using (var outfile = new FileStream(localFilePath, FileMode.Create))
            {
                var buffer = new byte[bufferSize];
                var bytesRead = saveFile.Read(buffer, 0, bufferSize);

                while (bytesRead > 0)
                {
                    outfile.Write(buffer, 0, bytesRead);
                    bytesRead = saveFile.Read(buffer, 0, bufferSize);
                }
            }
        }
    }
}

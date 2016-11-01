using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using DetectifyServiceLibrary;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using DetectifyService.Interfaces;
using DetectifyService.Models;
using DetectifyService.Repository;
using System.IO.Compression;

namespace DetectifyService.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ScreenshotService" in both code and config file together.
    public class ScreenshotService : IScreenshotService
    {
        private readonly IUrlRepository _repository;

        public ScreenshotService()
        {
            _repository = new UrlXmlRepository();
        }

        public void SaveScreenshots(string urls)
        {
            var urlsList = new List<Url>();
            var urlNames = urls.Split(';');
            foreach (var url in urlNames)
            {
                var fileName = new String(url.Substring(0, url.Length > 30 ? 30 : url.Length).Where(Char.IsLetter).ToArray()) + ".jpg";
                var filePath = string.Format(ConfigurationManager.AppSettings["DownloadPath"], Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName) + fileName;
                urlsList.Add(new Url
                {
                    UrlName = url,
                    ScreenshotFileName = fileName,
                    ScreenshotFilePath = filePath,
                    LastUpdated = DateTime.Now
                });
                ThreadStart objThreadStart = delegate
                {
                    var bmp = GenerateScreenshot(url, 1920, 1080);
                    bmp.Save(filePath, ImageFormat.Jpeg);
                };
                var thread = new Thread(objThreadStart);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }

            _repository.AddUrls(urlsList);
        }

        public FileDownloadReturnMessage DownloadFile(FileDownloadMessage request)
        {
            var localFileName = request.FileMetaData.LocalFileName;
            var url = request.FileMetaData.Url;
            var fileName = _repository.GetUrlByName(url)?.ScreenshotFileName;

            if (String.IsNullOrWhiteSpace(fileName))
            {
                Console.WriteLine($"No data found for: {fileName}");
                return new FileDownloadReturnMessage(new FileMetaData("", "", ""), new MemoryStream());
            }

            var serverFileName = string.Format(ConfigurationManager.AppSettings["DownloadPath"], Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName) + fileName;
            localFileName = $"{localFileName.TrimEnd('\\')}\\{fileName}";

            try
            {
                var fs = new FileStream(serverFileName, FileMode.Open);

                return new FileDownloadReturnMessage(new FileMetaData(localFileName, serverFileName, url), fs);
            }
            catch (IOException e)
            {
                Console.WriteLine("An error occurred in DownloadFile()");
                Console.WriteLine($"Error: {e.Message}");
                throw new IOException();
            }
        }

        public FileDownloadReturnMessage DownloadAllFiles(FileDownloadMessage request)
        {
            var localFileName = request.FileMetaData.LocalFileName;

            try
            {
                var urls = _repository.GetAllUrls();
                var fileList = new Dictionary<string, byte[]>();
                foreach (var url in urls)
                {
                    fileList.Add(url.ScreenshotFileName, File.ReadAllBytes(url.ScreenshotFilePath));
                }
                var filePath = string.Format(ConfigurationManager.AppSettings["ZipPath"], Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName);
                var fileName = CompressToZip(fileList, filePath);
                var serverFileName = filePath + fileName;
                localFileName = $"{localFileName.TrimEnd('\\')}\\{fileName}";

                var fs = new FileStream(serverFileName, FileMode.Open);

                return new FileDownloadReturnMessage(new FileMetaData(localFileName, serverFileName, ""), fs);
            }
            catch (IOException e)
            {
                Console.WriteLine("An error occurred in DownloadAllFiles()");
                Console.WriteLine($"Error: {e.Message}");
                throw new IOException();
            }
        }

        private Bitmap GenerateScreenshot(string url, int width, int height)
        {
            var wb = new WebBrowser();
            wb.ScrollBarsEnabled = false;
            wb.ScriptErrorsSuppressed = true;

            wb.Width = width;
            wb.Height = height;

            wb.Navigate(url);

            while (wb.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }

            var bitmap = new Bitmap(wb.Width, wb.Height);
            wb.DrawToBitmap(bitmap, new Rectangle(0, 0, wb.Width, wb.Height));
            wb.Dispose();

            return bitmap;
        }

        private string CompressToZip(Dictionary<string, byte[]> fileList, string filePath)
        {
            var zip = SetRandomFileName("zip");
            filePath += zip;

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var item in fileList)
                        {
                            var file = archive.CreateEntry(item.Key);
                            using (var entryStream = file.Open())
                            using (var binaryWriter = new BinaryWriter(entryStream))
                            {
                                binaryWriter.Write(item.Value);
                            }
                        }
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                    }
                }
                return zip;
            }
            catch (IOException e)
            {
                Console.WriteLine("An error occurred in CompressToZip()");
                Console.WriteLine($"Error: {e.Message}");
                throw new IOException();
            }
        }

        private string SetRandomFileName(string fileType, string url = "")
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var str = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
            str += $".{fileType}";
            return str;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DetectifyService.Interfaces;
using DetectifyService.Models;

namespace DetectifyService.Repository
{
    public class UrlXmlRepository : IUrlRepository
    {
        private readonly string _urlXml = string.Format(ConfigurationManager.AppSettings["UrlDoc"], Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName);
        private readonly XDocument _doc;

        public UrlXmlRepository()
        {
            _doc = XDocument.Load(_urlXml);
        }

        private void AddUrl(Url url)
        {
            var ids = _doc.Descendants("UrlId");
            var id = 1;
            if (ids.Any())
            {
                id = int.Parse(ids.Max(x => x.Value)) + 1;
            }
            _doc.Element("Urls").Add(
                new XElement("Url",
                    new XElement("UrlId", id),
                    new XElement("Name", url.UrlName),
                    new XElement("FileName", url.ScreenshotFileName),
                    new XElement("Filepath", url.ScreenshotFilePath),
                    new XElement("LastUpdated", url.LastUpdated)));

            _doc.Save(_urlXml);
        }

        public void AddUrls(IEnumerable<Url> urls)
        {
            foreach (var url in urls)
            {
                if (_doc.Descendants("Name").Any(x => x.Value == url.UrlName))
                {
                    UpdateUrl(url);
                }
                else
                {
                    AddUrl(url);
                }
            }
            _doc.Save(_urlXml);
        }

        public Url GetUrl(int id)
        {
            var xmlUrl = _doc.Descendants("Url").FirstOrDefault(x => x.Element("UrlId").Value == id.ToString());
            if (xmlUrl != null)
            {
                return new Url
                {
                    LastUpdated = DateTime.Parse(xmlUrl.Element("LastUpdated").Value),
                    ScreenshotFileName = xmlUrl.Element("FileName").Value,
                    ScreenshotFilePath = xmlUrl.Element("Filepath").Value,
                    UrlId = int.Parse(xmlUrl.Element("UrlId").Value),
                    UrlName = xmlUrl.Element("Name").Value
                };
            }

            return null;
        }

        public Url GetUrlByName(string urlName)
        {
            var xmlUrl = _doc.Descendants("Url").FirstOrDefault(x => x.Element("Name").Value == urlName);
            if (xmlUrl != null)
            {
                return new Url
                {
                    LastUpdated = DateTime.Parse(xmlUrl.Element("LastUpdated").Value),
                    ScreenshotFileName = xmlUrl.Element("FileName").Value,
                    ScreenshotFilePath = xmlUrl.Element("Filepath").Value,
                    UrlId = int.Parse(xmlUrl.Element("UrlId").Value),
                    UrlName = xmlUrl.Element("Name").Value
                };
            }

            return null;
        }

        public IEnumerable<Url> GetAllUrls()
        {
            var urls = _doc.Descendants("Url").Select(x => new Url
            {
                LastUpdated = DateTime.Parse(x.Element("LastUpdated").Value),
                ScreenshotFileName = x.Element("FileName").Value,
                ScreenshotFilePath = x.Element("Filepath").Value,
                UrlId = int.Parse(x.Element("UrlId").Value),
                UrlName = x.Element("Name").Value
            });
            return urls;
        }

        private void UpdateUrl(Url url)
        {
            var item = _doc.Descendants("Url").FirstOrDefault(x => x.Element("Name").Value == url.UrlName);
            item.SetElementValue("FileName", url.ScreenshotFileName);
            item.SetElementValue("Filepath", url.ScreenshotFilePath);
            item.SetElementValue("LastUpdated", url.LastUpdated);
            _doc.Save(_urlXml);
        }
    }
}

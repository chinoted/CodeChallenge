using System;

namespace DetectifyService.Models
{
    public class Url
    {
        public int UrlId { get; set; }
        public string UrlName { get; set; }
        public string ScreenshotFileName { get; set; }
        public string ScreenshotFilePath { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

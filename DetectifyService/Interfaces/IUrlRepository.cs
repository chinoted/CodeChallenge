using System.Collections.Generic;
using DetectifyService.Models;

namespace DetectifyService.Interfaces
{
    interface IUrlRepository
    {
        void AddUrls(IEnumerable<Url> urls);
        Url GetUrl(int id);
        Url GetUrlByName(string url);
        IEnumerable<Url> GetAllUrls();
    }
}

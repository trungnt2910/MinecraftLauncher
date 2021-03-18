using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class MicrosoftStoreApp
    {
        private string response = null;
        private List<AppxLocation> locations = new List<AppxLocation>();

        public string Token { get; private set; }
        public List<AppxLocation> Locations { get => locations; }

        public MicrosoftStoreApp(string token)
        {
            Token = token;
        }

        public async Task<HttpStatusCode> LoadAsync()
        {
            return await Task.Run(() =>
            {
                var url = "https://store.rg-adguard.net/api/GetFiles";

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";

                httpRequest.ContentType = "application/x-www-form-urlencoded";

                var data = $"type=url&url={Uri.EscapeUriString($"https://www.microsoft.com/store/apps/{Token}")}";

                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    response = streamReader.ReadToEnd();
                }

                ParseLocations();

                return httpResponse.StatusCode;
            });
        }

        private void ParseLocations()
        {
            locations.Clear();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(response);

            var table = doc.DocumentNode.SelectSingleNode("//table[@class='tftable']")
                        .Descendants("tr")
                        .Where(tr => tr.Elements("td").Count() >= 1)
                        .ToList();

            foreach (var row in table)
            {
                var data = row.Elements("td").ToList();
                string url = data[0].Descendants("a").FirstOrDefault().GetAttributeValue("href", null);
                string name = data[0].InnerText;
                string expire = data[1].InnerText;
                string sha1 = data[2].InnerText;

                var info = name.Split('_');

                if (new string[] {".appx", ".appxbundle", ".msix", ".msixbundle" }.Contains(Path.GetExtension(name)))
                {
                    locations.Add(new AppxLocation()
                    {
                        URL = url,
                        Name = name,
                        ExpireTime = DateTime.Parse(expire),
                        SHA1 = sha1,
                        PackageName = info[0],
                        Version = Version.Parse(info[1]),
                        Architecture = info[2]
                    }); ;
                }
            }
        }

        public AppxLocation Find(string name, Architecture arch)
        {
            return locations.Find(location => location.PackageName == name && location.Architecture == arch.ToString());
        }
    }
}

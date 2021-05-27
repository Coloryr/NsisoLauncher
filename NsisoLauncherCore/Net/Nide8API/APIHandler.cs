using Heijden.Dns.Portable;
using Heijden.DNS;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Nide8API
{
    public class APIHandler
    {
        public string Nide8ID { get; private set; }
        public APIHandler(string id)
        {
            Nide8ID = id;
        }

        public async Task<APIModules> GetInfoAsync()
        {
            string json = await HttpRequesterAPI.HttpGetStringAsync(string.Format("https://auth2.nide8.com:233/{0}", Nide8ID));
            return await Task.Factory.StartNew(() =>
            {
                return JsonConvert.DeserializeObject<APIModules>(json);
            });
        }

        public async Task<bool> TestIsNide8DnsOK()
        {
            Resolver resolver = new();
            var response = await resolver.Query("auth2.nide8.com", QType.A);
            if (string.IsNullOrWhiteSpace(response.Error) && response.RecordsA.Length != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

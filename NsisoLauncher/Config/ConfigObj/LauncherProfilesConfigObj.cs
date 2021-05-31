using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NsisoLauncher.Config.ConfigObj
{
    public record LauncherProfilesConfigObj
    {
        [JsonProperty("selectedProfile")]
        public string SelectedProfile { get; set; }

        [JsonProperty("profiles")]
        public JObject Profiles { get; set; }

        [JsonProperty("clientToken")]
        public string ClientToken { get; set; }
    }
}

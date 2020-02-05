using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;

namespace NsisoLauncherCore.Net.FunctionAPI
{
    public class FunctionAPIHandler
    {
        public DownloadSource Source { get; private set; }

        const string BMCLBase = "https://bmclapi2.bangbang93.com";

        const string MCBBSUrl = "https://download.mcbbs.net";

        const string ForgeUrl = "https://files.minecraftforge.net";

        public string VersionListURL { get; set; }
        public string ForgeListURL { get; set; }
        public string LiteloaderListURL { get; set; } 

        public FunctionAPIHandler(DownloadSource lib)
        {
            Source = lib;
            switch (Source)
            {
                case DownloadSource.Mojang:
                    VersionListURL = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
                    ForgeListURL = BMCLBase + "/forge/minecraft";
                    LiteloaderListURL = BMCLBase + "/liteloader/list";
                    break;
                case DownloadSource.BMCLAPI:
                    VersionListURL = BMCLBase + "/mc/game/version_manifest.json";
                    ForgeListURL = BMCLBase + "/forge/minecraft";
                    LiteloaderListURL = BMCLBase + "/liteloader/list";
                    break;
                case DownloadSource.MCBBS:
                    VersionListURL = MCBBSUrl + "/mc/game/version_manifest.json";
                    ForgeListURL = MCBBSUrl + "/forge/minecraft";
                    LiteloaderListURL = MCBBSUrl + "/liteloader/list";
                    break;
            }
        }

        public string DoURLReplace(string url)
        {
            return GetDownloadUrl.DoURLReplace(Source, url);
        }

        /// <summary>
        /// 联网获取版本列表
        /// </summary>
        /// <returns>版本列表</returns>
        public async Task<List<JWVersion>> GetVersionList()
        {
            string json = await APIRequester.HttpGetStringAsync(VersionListURL);
            if (json != null)
            {
                var e = JsonConvert.DeserializeObject<JWVersions>(json);
                return e.Versions;
            }
            return null;
        }

        /// <summary>
        /// 联网获取指定版本所有的FORGE
        /// </summary>
        /// <param name="version">要搜索的版本</param>
        /// <returns>Forge列表</returns>
        public async Task<List<JWForge>> GetForgeList(MCVersion version)
        {
            string json = await APIRequester.HttpGetStringAsync(string.Format("{0}/{1}", ForgeListURL, version.ID));
            var e = JsonConvert.DeserializeObject<List<JWForge>>(json);
            return e;
        }

        /// <summary>
        /// 联网获取指定版本所有的Liteloader
        /// </summary>
        /// <param name="version">要搜索的版本</param>
        /// <returns>Liteloader列表</returns>
        public async Task<JWLiteloader> GetLiteloaderList(MCVersion version)
        {
            string json = await APIRequester.HttpGetStringAsync(string.Format("{0}/?mcversion={1}", LiteloaderListURL, version.ID));
            var e = JsonConvert.DeserializeObject<JWLiteloader>(json);
            return e;
        }
    }
}

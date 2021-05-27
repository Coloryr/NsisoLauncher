using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Tools;

namespace NsisoLauncher.ModPack
{
    class GetForge
    {
        public DownloadTask Get(manifestObj obj)
        {
            return GetDownloadUrl.GetForgeDownloadURL(App.Config.MainConfig.Download.DownloadSource,
                obj.minecraft.version, obj.minecraft.modLoaders[0].id.Replace("forge-", ""));
        }
    }
}

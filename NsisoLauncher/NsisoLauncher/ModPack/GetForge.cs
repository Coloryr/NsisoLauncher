using NsisoLauncherCore;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Util.Installer;
using System;

namespace NsisoLauncher.ModPack
{
    class GetForge
    {
        public DownloadTask Get(manifestObj obj)
        {
            string forge = obj.minecraft.modLoaders[0].id.Replace("forge", "");
            string local = PathManager.TempDirectory + "\\forge-" + obj.minecraft.version + forge + ".jar";
            string forgePath = string.Format("maven/net/minecraftforge/forge/{0}{1}/forge-{0}{1}-installer.jar", obj.minecraft.version, forge);
            DownloadTask dt = new DownloadTask(App.GetResourceString("String.NewDownloadTaskWindow.Core.Forge"),
                string.Format("https://bmclapi2.bangbang93.com/{0}", forgePath),
                local);
            dt.Todo = new Func<Exception>(() =>
            {
                try
                {
                    CommonInstaller installer = new CommonInstaller(local, new CommonInstallOptions()
                    { GameRootPath = App.Handler.GameRootPath });
                    installer.BeginInstall();
                    return null;
                }
                catch (Exception ex)
                { return ex; }
            });
            return dt;
        }
    }
}

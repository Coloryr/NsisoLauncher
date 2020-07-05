using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.FunctionAPI;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.Installer.Fabric
{
    class FabricInstaller : IInstaller
    {
        public string InstallerPath { get; set; }
        public CommonInstallOptions Options { get; set; }
        public ProgressCallback Monitor { get; set; }

        const string data = "-jar \"{0}\" client -dir \"{1}\" -mcversion \"{2}\" -loader \"{3}\"";

        public FabricInstaller(string installerPath, CommonInstallOptions options)
        {
            if (string.IsNullOrWhiteSpace(installerPath))
            {
                throw new ArgumentException("Installer path can not be null or whitespace.");
            }
            this.InstallerPath = installerPath;
            this.Options = options ?? throw new ArgumentNullException("Install options is null");
        }

        public async void BeginInstall(ProgressCallback callback, CancellationToken cancellationToken)
        {
            CommonInstaller.IsInstal = true;
            Monitor = callback;
            var obj = (APIModules.TwoObj)Options.obj;
            string ver;
            if (string.IsNullOrWhiteSpace(obj.version.InheritsVersion))
            {
                ver = obj.version.ID;
            }
            else
            {
                ver = obj.version.InheritsVersion;
            }
            string arg = string.Format(data, InstallerPath, Options.GameRootPath, ver, obj.fabric.Version);
            ProcessStartInfo processStartInfo = new ProcessStartInfo(Options.Java.Path, arg)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            await Task.Factory.StartNew(() =>
            {
                var result = Process.Start(processStartInfo);
                result.BeginOutputReadLine();
                result.OutputDataReceived += Result_OutputDataReceived;
                result.WaitForExit();

                File.Delete(InstallerPath);
            });

            CommonInstaller.IsInstal = false;
        }
        private void Result_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}

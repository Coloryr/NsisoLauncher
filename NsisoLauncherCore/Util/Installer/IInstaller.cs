using NsisoLauncherCore.Net;
using System.Threading;

namespace NsisoLauncherCore.Util.Installer
{
    public interface IInstaller
    {
        void BeginInstall(ProgressCallback callback, CancellationToken cancellationToken);
    }
}

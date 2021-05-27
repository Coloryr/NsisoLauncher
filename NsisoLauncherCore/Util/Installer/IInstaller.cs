using NsisoLauncherCore.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.Installer
{
    public interface IInstaller
    {
        Task BeginInstall(ProgressCallback callback, CancellationToken cancellationToken);
    }
}

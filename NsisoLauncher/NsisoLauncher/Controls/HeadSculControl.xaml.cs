using NsisoLauncher.Config;
using NsisoLauncherCore.Net.Head;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NsisoLauncher.Controls
{
    /// <summary>
    /// HeadSculControl.xaml 的交互逻辑
    /// </summary>
    public partial class HeadSculControl : UserControl
    {
        public HeadSculControl()
        {
            InitializeComponent();
        }

        public async Task RefreshIcon_online(string uuid)
        {
            OnlineHead head = new OnlineHead();
            progressRing.IsActive = true;
            iconImage.Source = await head.GetHeadSculSource(uuid);
            progressRing.IsActive = false;
        }
        public async Task RefreshIcon_nide8(string uuid, AuthenticationNode args)
        {
            Nide8Head head = new Nide8Head();
            progressRing.IsActive = true;
            iconImage.Source = await head.GetHeadSculSource(uuid, args);
            progressRing.IsActive = false;
        }
    }
}

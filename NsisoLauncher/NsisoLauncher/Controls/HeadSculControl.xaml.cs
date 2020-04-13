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

        public async Task RefreshIconOnline(string uuid)
        {
            var head = new OnlineHead();
            progressRing.IsActive = true;
            iconImage.Source = await head.GetHeadSculSource(uuid);
            progressRing.IsActive = false;
        }
        public async Task RefreshIconNide8(string uuid, AuthenticationNode args)
        {
            var head = new Nide8Head();
            progressRing.IsActive = true;
            iconImage.Source = await head.GetHeadSculSource(uuid, args);
            progressRing.IsActive = false;
        }
        public async Task RefreshIconInjector(string uuid, AuthenticationNode args)
        {
            var head = new InjectorHead();
            progressRing.IsActive = true;
            iconImage.Source = await head.GetHeadSculSource(uuid, args);
            progressRing.IsActive = false;
        }
    }
}

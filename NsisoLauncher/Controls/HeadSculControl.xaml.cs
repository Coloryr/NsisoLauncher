using NsisoLauncher.Color_yr;
using NsisoLauncher.Config;
using NsisoLauncherCore.Net.CrafatarAPI;
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

        public async Task RefreshIcon(string uuid)
        {
            APIHandler handler = new APIHandler();
            progressRing.IsActive = true;
            iconImage.Source = await handler.GetHeadSculSource(uuid);
            progressRing.IsActive = false;
        }
        //Color_yr Add
        public async Task RefreshIcon_nide8(string uuid, AuthenticationNode args)
        {
            APIHandler_nide8 handler = new APIHandler_nide8();
            progressRing.IsActive = true;
            iconImage.Source = await handler.GetHeadSculSource(uuid, args);
            progressRing.IsActive = false;
        }
    }
}

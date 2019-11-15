using MahApps.Metro.Controls;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// Register.xaml 的交互逻辑
    /// </summary>
    public partial class Register : MetroWindow
    {
        private string uri;
        public Register(string uri)
        {
            this.uri = uri;
            InitializeComponent();
            Bro.Address = uri;
        }
    }
}

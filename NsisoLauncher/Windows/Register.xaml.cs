using MahApps.Metro.Controls;
using System;

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
            Bro.Navigate(new Uri(uri));
        }
    }
}

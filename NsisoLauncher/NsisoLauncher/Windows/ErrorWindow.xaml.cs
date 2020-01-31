using MahApps.Metro.Controls;
using System;
using System.Linq;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// ErrorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorWindow : MetroWindow
    {

        readonly string[] funny = {
            "可能你不知道，这是一个非官方版本",
            "这个启动器只为服主服务（可能）",
            "如果你需要技术支持，可能会瞬间消失"
        };

        public ErrorWindow(Exception ex)
        {
            InitializeComponent();
            Random random = new Random();
            FunnyBlock.Text = funny[random.Next(funny.Count())];
            textBox.Text = ex.ToString();
        }
    }
}

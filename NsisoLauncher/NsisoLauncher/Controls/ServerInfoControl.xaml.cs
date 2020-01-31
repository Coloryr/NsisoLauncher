using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using NsisoLauncherCore.Net.Server;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;

namespace NsisoLauncher.Controls
{
    /// <summary>
    /// ServerInfoControl.xaml 的交互逻辑
    /// </summary>
    public partial class ServerInfoControl : UserControl
    {
        public ServerInfoControl()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
            APP_Color();
        }

        [System.ComponentModel.TypeConverter(typeof(LengthConverter))]
        [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public new double Width { get; set; }
        public void APP_Color()
        {
            BrushColor get = new BrushColor();
            Brush b = get.GetBursh();
            if (b != null)
            {
                serverNameTextBlock.Background = b;
                serverPeopleTextBlock.Background = b;
                serverVersionTextBlock.Background = b;
                serverPingTextBlock.Background = b;
                serverMotdTextBlock.Background = b;
            }
        }

        public async void SetServerInfo(Server server)
        {
            if (server.ShowServerInfo)
            {
                serverNameTextBlock.Text = server.ServerName;
                serverStateIcon.Foreground = System.Windows.Media.Brushes.White;
                serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.SyncAltSolid;
                serverPeopleTextBlock.Text = App.GetResourceString("String.Mainwindow.ServerGettingNum");
                serverVersionTextBlock.Text = App.GetResourceString("String.Mainwindow.ServerGettingVer");
                serverPingTextBlock.Text = App.GetResourceString("String.Mainwindow.ServerGettingPing");
                serverMotdTextBlock.Text = null;
                Visibility = Visibility.Visible;
                serverLoadingBar.Visibility = Visibility.Visible;
                serverLoadingBar.IsIndeterminate = true;


                ServerInfo serverInfo = new ServerInfo(server.Address, server.Port);
                await serverInfo.StartGetServerInfoAsync();

                App.logHandler.AppendDebug(serverInfo.JsonResult);
                serverLoadingBar.IsIndeterminate = false;
                serverLoadingBar.Visibility = Visibility.Hidden;
                serverNameTextBlock.Text = server.ServerName;
                if (string.IsNullOrWhiteSpace(serverNameTextBlock.Text) == true)
                    serverNameTextBlock.Text = App.config.MainConfig.Server.ServerName;

                switch (serverInfo.State)
                {
                    case ServerInfo.StateType.GOOD:
                        serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.CheckCircleSolid;
                        serverStateIcon.Foreground = System.Windows.Media.Brushes.Green;
                        serverPeopleTextBlock.Text = string.Format("人数:[{0}/{1}]", serverInfo.CurrentPlayerCount, serverInfo.MaxPlayerCount);
                        serverVersionTextBlock.Text = serverInfo.GameVersion;
                        serverVersionTextBlock.ToolTip = serverInfo.GameVersion;
                        serverPingTextBlock.Text = string.Format("延迟:{0}ms", serverInfo.Ping);
                        if (serverInfo.MOTD != null)
                        {
                            serverMotdTextBlock.ToolTip = serverInfo.MOTD;
                            string str = serverInfo.MOTD;
                            str.Replace(Convert.ToChar(10).ToString(), "&#x000A");
                            serverMotdTextBlock.Text = str;
                        }
                        if (serverInfo.OnlinePlayersName != null)
                        {
                            serverPeopleTextBlock.ToolTip = string.Join("\n", serverInfo.OnlinePlayersName);
                        }
                        if (serverInfo.IconData != null)
                        {
                            using (MemoryStream ms = new MemoryStream(serverInfo.IconData))
                            {
                                serverIcon.Fill = new ImageBrush(ChangeBitmapToImageSource(new Bitmap(ms)));
                            }

                        }
                        break;
                    default:
                        serverNameTextBlock.Text = "获取失败";
                        serverPeopleTextBlock.Text = "";
                        serverVersionTextBlock.Text = "";
                        serverPingTextBlock.Text = "";
                        serverMotdTextBlock.Text = "";
                        break;
                }
            }
            else
            {
                Visibility = Visibility.Hidden;
            }
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>  
        /// 从bitmap转换成ImageSource  
        /// </summary>  
        /// <param name="icon"></param>  
        /// <returns></returns>  
        private static ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        {
            //Bitmap bitmap = icon.ToBitmap();  
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            if (!DeleteObject(hBitmap))
            {
                throw new System.ComponentModel.Win32Exception();
            }
            return wpfBitmap;

        }
    }
}

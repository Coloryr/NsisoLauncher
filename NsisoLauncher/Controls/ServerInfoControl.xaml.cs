using NsisoLauncher.Config.ConfigObj;
using NsisoLauncher.Utils;
using NsisoLauncherCore.Net.Server;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NsisoLauncher.Controls
{
    /// <summary>
    /// ServerInfoControl.xaml 的交互逻辑
    /// </summary>
    public partial class ServerInfoControl : UserControl
    {
        private List<string> noshow = new List<string>()
        {
            "minecraft", "FML", "forge", "mcp"
        };
        public ServerInfoControl()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
        }
        public void APP_Color()
        {
            System.Windows.Media.Brush b = BrushColor.GetBursh();
            if (b != null)
            {
                serverNameTextBlock.Background = b;
                serverPeopleTextBlock.Background = b;
                serverVersionTextBlock.Background = b;
                serverPingTextBlock.Background = b;
                serverMotdTextBlock.Background = b;
                serverModsTextBlock.Background = b;
            }
        }

        private bool isno(string data)
        {
            return Regex.IsMatch(data, @"[a-zA-Z0-9_]*");
        }
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using MemoryStream outStream = new();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapImage));
            enc.Save(outStream);
            Bitmap bitmap = new(outStream);
            return new Bitmap(bitmap);
        }
        public async void SetServerInfo(ServerObj server)
        {
            APP_Color();
            if (server.ShowServerInfo)
            {
                serverNameTextBlock.Text = string.IsNullOrWhiteSpace(server.ServerName) ?
                    App.GetResourceString("String.ServerInfoControl.McServer") : server.ServerName;
                serverIcon.Fill = new ImageBrush(ChangeBitmapToImageSource(BitmapImage2Bitmap(new BitmapImage(new Uri("pack://application:,,,/Resource/unknown_server.png")))));
                serverStateIcon.Foreground = System.Windows.Media.Brushes.White;
                serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.SyncAltSolid;
                serverPeopleTextBlock.Text = App.GetResourceString("String.Mainwindow.ServerGettingNum");
                serverVersionTextBlock.Text = App.GetResourceString("String.Mainwindow.ServerGettingVer");
                serverPingTextBlock.Text = App.GetResourceString("String.Mainwindow.ServerGettingPing");
                serverMotdTextBlock.Text = null;
                serverModsTextBlock.Text = null;
                Visibility = Visibility.Visible;
                serverLoadingBar.Visibility = Visibility.Visible;
                serverLoadingBar.IsIndeterminate = true;

                ServerInfo serverInfo = new(server.Address, server.Port);
                await serverInfo.StartGetServerInfoAsync();

                App.LogHandler.AppendDebug(serverInfo.JsonResult);
                serverLoadingBar.IsIndeterminate = false;
                serverLoadingBar.Visibility = Visibility.Hidden;

                switch (serverInfo.State)
                {
                    case ServerInfo.StateType.GOOD:
                        bool isserverinfo = false;
                        string players = "";
                        serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.CheckCircleSolid;
                        serverStateIcon.Foreground = System.Windows.Media.Brushes.Green;
                        players = serverPeopleTextBlock.Text = App.GetResourceString("String.ServerInfoControl.Member")
                            + string.Format("[{0}/{1}]", serverInfo.CurrentPlayerCount, serverInfo.MaxPlayerCount);
                        serverVersionTextBlock.Text = serverInfo.GameVersion;
                        if (serverInfo.OnlinePlayersName != null)
                        {
                            foreach (var item in serverInfo.OnlinePlayersName)
                            {
                                if (!isserverinfo && isno(item))
                                    isserverinfo = true;
                                if (isserverinfo)
                                    players += item + "\n";
                                else
                                    players += item + ",";
                            }
                            players = players.Substring(0, players.Length - 1);
                        }
                        serverVersionTextBlock.ToolTip = isserverinfo ? players : serverInfo.GameVersion;
                        serverPeopleTextBlock.ToolTip = isserverinfo ? serverPeopleTextBlock.Text : players;
                        serverPingTextBlock.Text = App.GetResourceString("String.ServerInfoControl.Ping")
                            + string.Format("{0}ms", serverInfo.Ping);
                        if (serverInfo.MOTD != null && !string.IsNullOrWhiteSpace(serverInfo.MOTD))
                        {
                            serverMotdTextBlock.ToolTip = serverInfo.MOTD;
                            serverMotdTextBlock.Text = serverInfo.MOTD.Replace(Convert.ToChar(10).ToString(), "&#x000A"); ;
                            serverMotdTextBlock.Visibility = Visibility.Visible;
                        }
                        else
                            serverMotdTextBlock.Visibility = Visibility.Collapsed;
                        if (serverInfo.ForgeInfo != null)
                        {
                            string mods = "mods:";
                            string modsall = "";
                            int count = 0;
                            int lenth = 0;
                            foreach (var item in serverInfo.ForgeInfo.Mods)
                            {
                                if (!noshow.Contains(item.ModID))
                                {
                                    if (count < 6)
                                        mods += item.ModID + ",";
                                    else
                                    {
                                        mods += item.ModID + ",";
                                        mods = mods.Substring(0, mods.Length - 1);
                                        mods += string.Format(App.GetResourceString("String.ServerInfoControl.Mods"),
                                            (serverInfo.ForgeInfo.Mods.Count - count));
                                    }
                                    modsall += item.ModID + ",";
                                    lenth += item.ModID.Length;
                                    if (lenth > 50)
                                    {
                                        lenth = 0;
                                        modsall += "\n";
                                    }
                                    count++;
                                }
                            }
                            serverModsTextBlock.Text = mods;
                            serverModsTextBlock.ToolTip = modsall;

                        }
                        if (serverInfo.IconData != null)
                        {
                            MemoryStream ms = new(serverInfo.IconData);
                            serverIcon.Fill = new ImageBrush(ChangeBitmapToImageSource(new Bitmap(ms)));
                        }
                        break;
                    default:
                        serverNameTextBlock.Text = App.GetResourceString("String.ServerInfoControl.Error");
                        serverPeopleTextBlock.Text = "";
                        serverVersionTextBlock.Text = "";
                        serverPingTextBlock.Text = "";
                        serverMotdTextBlock.Text = "";
                        serverModsTextBlock.Text = "";
                        break;
                }
            }
            else
                Visibility = Visibility.Hidden;
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

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.ModPack;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.FunctionAPI;
using NsisoLauncherCore.Net.Tools;
using NsisoLauncherCore.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// NewDownloadTaskWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewDownloadTaskWindow : MetroWindow
    {
        ObservableCollection<JWVersion> verList = new ObservableCollection<JWVersion>();
        ObservableCollection<JWForge> forgeList = new ObservableCollection<JWForge>();
        ObservableCollection<JWLiteloader> liteloaderList = new ObservableCollection<JWLiteloader>();

        public string Local { get; set; }

        private FunctionAPIHandler apiHandler;

        public NewDownloadTaskWindow(bool res = false)
        {
            apiHandler = new FunctionAPIHandler(App.Config.MainConfig.Download.DownloadSource);
            InitializeComponent();
            versionListDataGrid.ItemsSource = verList;
            forgeListDataGrid.ItemsSource = forgeList;
            liteloaderListDataGrid.ItemsSource = liteloaderList;
            ICollectionView vwV = CollectionViewSource.GetDefaultView(verList);
            vwV.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
            vwV.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            ICollectionView vwF = CollectionViewSource.GetDefaultView(forgeList);
            vwF.SortDescriptions.Add(new SortDescription("Version", ListSortDirection.Descending));
            if (res == true)
                RefreshVerButton_Click(null, null);
            DataContext = this;
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<MCVersion> vers = await App.Handler.GetVersionsAsync();
            verToInstallForgeComboBox.ItemsSource = vers;
            verToInstallLiteComboBox.ItemsSource = vers;
        }

        private async void RefreshVersion()
        {
            var loading = await this.ShowProgressAsync(App.GetResourceString("String.NewDownloadTaskWindow.Get.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.Get.Text"));
            loading.SetIndeterminate();
            List<JWVersion> result;
            try
            {
                result = await apiHandler.GetVersionList();
            }
            catch (WebException)
            {
                result = null;
            }
            await loading.CloseAsync();
            verList.Clear();
            if (result == null)
            {
                await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.GetList.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.GetList.Text"));
            }
            else
            {
                foreach (var item in result)
                {
                    item.Type = App.GetResourceString("String.NewDownloadTaskWindow.Type." + item.Type);
                    verList.Add(item);
                }
            }
        }

        private async void RefreshForge()
        {
            MCVersion ver;
            if (verToInstallForgeComboBox.SelectedItem != null)
            {
                ver = (MCVersion)verToInstallForgeComboBox.SelectedItem;
            }
            else
            {
                await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.Forge.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.Forge.Text"));
                return;
            }
            var loading = await this.ShowProgressAsync(App.GetResourceString("String.NewDownloadTaskWindow.ForgeList.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.ForgeList.Text"));
            loading.SetIndeterminate();
            List<JWForge> result;
            forgeList.Clear();
            try
            {
                result = await apiHandler.GetForgeList(ver);
            }
            catch (WebException)
            {
                await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.ForgeListFail.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.ForgeListFail.Text"));
                return;
            }
            await loading.CloseAsync();
            if (result == null || result.Count == 0)
            {
                await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.ForgeListNull.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.ForgeListNull.Text"));
            }
            else
            {
                foreach (var item in result)
                {
                    forgeList.Add(item);
                }
            }
        }

        private async void RefreshLiteloader()
        {
            MCVersion ver;
            if (verToInstallLiteComboBox.SelectedItem != null)
            {
                ver = (MCVersion)verToInstallLiteComboBox.SelectedItem;
            }
            else
            {
                await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.Liteloader.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.Liteloader.Text"));
                return;
            }
            var loading = await this.ShowProgressAsync(App.GetResourceString("String.NewDownloadTaskWindow.LiteloaderList.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.LiteloaderList.Text"));
            loading.SetIndeterminate();
            JWLiteloader result;
            liteloaderList.Clear();
            try
            {
                result = await apiHandler.GetLiteloaderList(ver);
            }
            catch (WebException)
            {
                await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.LiteloaderListFail.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.LiteloaderListFail.Text"));
                return;
            }
            await loading.CloseAsync();
            if (result == null)
            {
                await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.LiteloaderListNull.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.LiteloaderListNull.Text"));
            }
            else
            {
                liteloaderList.Add(result);
            }
        }

        private async void DownloadVerButton_Click(object sender, RoutedEventArgs e)
        {
            IList selectItems = versionListDataGrid.SelectedItems;
            if (selectItems.Count == 0)
            {
                await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.Chose.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.Chose.Text"));
            }
            else
            {
                var loading = await this.ShowProgressAsync(App.GetResourceString("String.NewDownloadTaskWindow.DownLoad.Title"),
                    string.Format(App.GetResourceString("String.NewDownloadTaskWindow.DownLoad.Title"), selectItems.Count));
                loading.SetIndeterminate();
                await AppendVersionsDownloadTask(selectItems);
                await loading.CloseAsync();
                Close();
            }
        }

        //TODO:修复FORGE刷新不成功崩溃
        private async void DownloadForgeButton_Click(object sender, RoutedEventArgs e)
        {
            MCVersion ver;
            if (verToInstallForgeComboBox.SelectedItem != null)
            {
                ver = (MCVersion)verToInstallForgeComboBox.SelectedItem;
            }
            else
            {
                await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.ForgeChose.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.ForgeChose.Text"));
                return;
            }

            JWForge forge;
            if (forgeListDataGrid.SelectedItem != null)
            {
                forge = (JWForge)forgeListDataGrid.SelectedItem;
            }
            else
            {
                await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.ChoseForge.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.ChoseForge.Text"));
                return;
            }

            await AppendForgeDownloadTaskAsync(ver, forge);
            Close();
        }

        private async void DownloadLiteloaderButton_Click(object sender, RoutedEventArgs e)
        {
            MCVersion ver;
            if (verToInstallLiteComboBox.SelectedItem != null)
            {
                ver = (MCVersion)verToInstallLiteComboBox.SelectedItem;
            }
            else
            {
                await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.LiteloaderChose.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.LiteloaderChose.Text"));
                return;
            }

            JWLiteloader liteloader = null;
            if (liteloaderListDataGrid.SelectedItem != null)
            {
                liteloader = (JWLiteloader)liteloaderListDataGrid.SelectedItem;
            }
            else
            {
                await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.ChoseLiteloader.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.ChoseLiteloader.Text"));
                return;
            }

            AppendLiteloaderDownloadTask(ver, liteloader);
            Close();
        }

        private async Task AppendVersionsDownloadTask(IList list)
        {
            try
            {
                foreach (JWVersion item in list)
                {
                    var http = new HttpRequesterAPI(TimeSpan.FromSeconds(10));
                    string json = await http.HttpGetStringAsync(apiHandler.DoURLReplace(item.Url));
                    MCVersion ver = App.Handler.JsonToVersion(json);
                    string jsonPath = App.Handler.GetJsonPath(ver.ID);

                    string dir = Path.GetDirectoryName(jsonPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.WriteAllText(jsonPath, json);

                    List<DownloadTask> tasks = new List<DownloadTask>();

                    tasks.Add(new DownloadTask(App.GetResourceString("String.NewDownloadTaskWindow.Source"),
                        apiHandler.DoURLReplace(ver.AssetIndex.URL), App.Handler.GetAssetsIndexPath(ver.Assets)));

                    tasks.AddRange(await FileHelper.GetLostDependDownloadTaskAsync(App.Config.MainConfig.Download.DownloadSource, App.Handler, ver));

                    App.Downloader.SetDownloadTasks(tasks);
                    App.Downloader.StartDownload();
                }
            }
            catch (WebException ex)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.Fail.Title"),
                        App.GetResourceString("String.NewDownloadTaskWindow.Fail.Text") + ex.Message);
                }));
            }
            catch (Exception ex)
            {
                AggregateExceptionArgs args = new AggregateExceptionArgs()
                {
                    AggregateException = new AggregateException(ex)
                };
                App.CatchAggregateException(this, args);
            }

        }

        private async Task AppendForgeDownloadTaskAsync(MCVersion ver, JWForge forge)
        {
            DownloadTask dt = await GetDownloadUrl.GetForgeDownloadURL(App.Config.MainConfig.Download.DownloadSource, forge, ver.ID);
            App.Downloader.SetDownloadTasks(dt);
            App.Downloader.StartDownload();
        }

        private void AppendLiteloaderDownloadTask(MCVersion ver, JWLiteloader liteloader)
        {
            DownloadTask dt = GetDownloadUrl.GetLiteloaderDownloadURL(App.Config.MainConfig.Download.DownloadSource, liteloader);
            App.Downloader.SetDownloadTasks(dt);
            App.Downloader.StartDownload();
        }

        private void RefreshVerButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshVersion();
        }

        private void RefreshForgeButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshForge();
        }

        private void RefresLiteButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshLiteloader();
        }

        private void VerToInstallForgeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshForge();
        }

        private void VerToInstallLiteComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshLiteloader();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = App.GetResourceString("String.NewDownloadTaskWindow.ModPack.Select");
            openFileDialog.DefaultExt = "整合包|*.zip";
            openFileDialog.RestoreDirectory = true;


            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Local_T.Text = openFileDialog.FileName;
                Local = openFileDialog.FileName;
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var res = await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.ModPack.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.ModPack.Text"),
                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                {
                    AffirmativeButtonText = App.GetResourceString("String.Base.Yes"),
                    NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                    DefaultButtonFocus = MessageDialogResult.Affirmative
                });
            if (res == MessageDialogResult.Affirmative)
            {
                var loading = await this.ShowProgressAsync(App.GetResourceString("String.NewDownloadTaskWindow.ModPack.Title2"),
                App.GetResourceString("String.NewDownloadTaskWindow.ModPack.Text2"));
                loading.SetIndeterminate();

                CheckModPack CheckModPack = new CheckModPack(loading);
                var res1 = await CheckModPack.Check(Local);
                if (res1 == null)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.NewDownloadTaskWindow.ModPack.Error.Title"),
                App.GetResourceString("String.NewDownloadTaskWindow.ModPack.Error.Text"));
                }
                else
                {
                    App.Downloader.SetDownloadTasks(res1);
                    App.Downloader.StartDownload();
                }

                await loading.CloseAsync();

                Close();
            }
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            if (App.DownloadWindow_ == null)
            {
                App.DownloadWindow_ = new DownloadWindow();
                App.DownloadWindow_.Show();
            }
            else
            {
                App.DownloadWindow_.Refresh();
            }
            App.NewDownloadTaskWindow_ = null;
        }
    }
}

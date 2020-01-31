﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.FunctionAPI;
using NsisoLauncherCore.Util.Installer;
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
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;
using Version = NsisoLauncherCore.Modules.Version;

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

        private FunctionAPIHandler apiHandler;

        public NewDownloadTaskWindow(bool res = false)
        {
            apiHandler = new FunctionAPIHandler(App.config.MainConfig.Download.DownloadSource);
            InitializeComponent();
            versionListDataGrid.ItemsSource = verList;
            forgeListDataGrid.ItemsSource = forgeList;
            liteloaderListDataGrid.ItemsSource = liteloaderList;
            ICollectionView vwV = CollectionViewSource.GetDefaultView(verList);
            vwV.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
            vwV.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            ICollectionView vwF = CollectionViewSource.GetDefaultView(forgeList);
            vwF.SortDescriptions.Add(new SortDescription("Version", ListSortDirection.Descending));
            if(res == true)
                RefreshVerButton_Click(null, null);
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<Version> vers = await App.handler.GetVersionsAsync();
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
                await this.ShowProgressAsync(App.GetResourceString("String.NewDownloadTaskWindow.GetList.Title"),
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
            Version ver;
            if (verToInstallForgeComboBox.SelectedItem != null)
            {
                ver = (Version)verToInstallForgeComboBox.SelectedItem;
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
            Version ver = null;
            if (verToInstallLiteComboBox.SelectedItem != null)
            {
                ver = (Version)verToInstallLiteComboBox.SelectedItem;
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
                var loading = await this.ShowProgressAsync(App.GetResourceString("String.NewDownloadTaskWindow.DownLoad.Title"), string.Format(App.GetResourceString("String.NewDownloadTaskWindow.DownLoad.Title"), selectItems.Count));
                loading.SetIndeterminate();
                await AppendVersionsDownloadTask(selectItems);
                await loading.CloseAsync();
                this.Close();
            }
        }

        //TODO:修复FORGE刷新不成功崩溃
        private async void DownloadForgeButton_Click(object sender, RoutedEventArgs e)
        {
            Version ver;
            if (verToInstallForgeComboBox.SelectedItem != null)
            {
                ver = (Version)verToInstallForgeComboBox.SelectedItem;
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

            AppendForgeDownloadTask(ver, forge);
            this.Close();
        }

        private async void DownloadLiteloaderButton_Click(object sender, RoutedEventArgs e)
        {
            Version ver;
            if (verToInstallLiteComboBox.SelectedItem != null)
            {
                ver = (Version)verToInstallLiteComboBox.SelectedItem;
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
            this.Close();
        }

        private async Task AppendVersionsDownloadTask(IList list)
        {
            try
            {
                foreach (JWVersion item in list)
                {
                    string json = await APIRequester.HttpGetStringAsync(apiHandler.DoURLReplace(item.Url));
                    NsisoLauncherCore.Modules.Version ver = App.handler.JsonToVersion(json);
                    string jsonPath = App.handler.GetJsonPath(ver.ID);

                    string dir = Path.GetDirectoryName(jsonPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.WriteAllText(jsonPath, json);

                    List<DownloadTask> tasks = new List<DownloadTask>();

                    tasks.Add(new DownloadTask(App.GetResourceString("String.NewDownloadTaskWindow.Source"), apiHandler.DoURLReplace(ver.AssetIndex.URL), App.handler.GetAssetsIndexPath(ver.Assets)));

                    tasks.AddRange(await NsisoLauncherCore.Util.FileHelper.GetLostDependDownloadTaskAsync(App.config.MainConfig.Download.DownloadSource, App.handler, ver));

                    App.downloader.SetDownloadTasks(tasks);
                    App.downloader.StartDownload();
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

        private void AppendForgeDownloadTask(Version ver, JWForge forge)
        {
            string forgePath = NsisoLauncherCore.PathManager.TempDirectory + string.Format(@"\Forge_{0}-Installer.jar", forge.Build);
            DownloadTask dt = new DownloadTask(App.GetResourceString("String.NewDownloadTaskWindow.Core.Forge"),
                string.Format("https://bmclapi2.bangbang93.com/forge/download/{0}", forge.Build),
                forgePath);
            dt.Todo = new Func<Exception>(() =>
            {
                try
                {
                    CommonInstaller installer = new CommonInstaller(forgePath, new CommonInstallOptions() { GameRootPath = App.handler.GameRootPath });
                    installer.BeginInstall();
                    return null;
                }
                catch (Exception ex)
                { return ex; }
            });
            App.downloader.SetDownloadTasks(dt);
            App.downloader.StartDownload();

        }

        private void AppendLiteloaderDownloadTask(Version ver, JWLiteloader liteloader)
        {
            string liteloaderPath = NsisoLauncherCore.PathManager.TempDirectory + string.Format(@"\Liteloader_{0}-Installer.jar", liteloader.Version);
            DownloadTask dt = new DownloadTask(App.GetResourceString("String.NewDownloadTaskWindow.Core.Liteloader"),
                string.Format("https://bmclapi2.bangbang93.com/liteloader/download?version={0}", liteloader.Version),
                liteloaderPath);
            dt.Todo = new Func<Exception>(() =>
            {
                try
                {
                    CommonInstaller installer = new CommonInstaller(liteloaderPath, new CommonInstallOptions() { GameRootPath = App.handler.GameRootPath });
                    installer.BeginInstall();
                    return null;
                }
                catch (Exception ex)
                { return ex; }
            });
            App.downloader.SetDownloadTasks(dt);
            App.downloader.StartDownload();

        }

        private async Task InstallCommonExtend(string path)
        {
            CommonInstaller installer = new CommonInstaller(path, new CommonInstallOptions() { GameRootPath = App.handler.GameRootPath });
            await installer.BeginInstallAsync();
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
    }
}

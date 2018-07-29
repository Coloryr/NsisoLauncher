﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Core.Net;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// DownloadWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadWindow : MetroWindow
    {
        private ObservableCollection<DownloadTask> Tasks;
        public DownloadWindow()
        {
            InitializeComponent();
            App.downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
            App.downloader.DownloadSpeedChanged += Downloader_DownloadSpeedChanged;
            App.downloader.DownloadCompleted += Downloader_DownloadCompleted;
            Refresh();
        }

        private void Refresh()
        {
            if (App.downloader.DownloadTasks != null)
            {
                Tasks = new ObservableCollection<DownloadTask>(App.downloader.DownloadTasks);
            }
            else
            {
                Tasks = new ObservableCollection<DownloadTask>();
            }
            downloadList.ItemsSource = Tasks;
        }

        public async Task ShowWhenDownloading()
        {
            this.Topmost = true;
            this.Show();
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    EventWaitHandle _waitHandle = new AutoResetEvent(false);
                    App.downloader.DownloadCompleted += (a, b) =>
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            try
                            {
                                this.Close();
                            }
                            catch (Exception) { }
                        }));
                        _waitHandle.Set();
                    };
                    _waitHandle.WaitOne();
                }
                catch (Exception ex)
                {
                    AggregateExceptionArgs args = new AggregateExceptionArgs()
                    {
                        AggregateException = new AggregateException(ex)
                    };
                    App.CatchAggregateException(this, args);
                }
            });
        }

        private void Downloader_DownloadCompleted(object sender, Utils.DownloadCompletedArg e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                speedTextBlock.Text = "0Kb/s";
                progressBar.Maximum = 1;
                progressBar.Value = 0;
                progressPerTextBlock.Text = "000%";
                if (e.ErrorList == null || e.ErrorList.Count == 0)
                {
                    this.ShowMessageAsync(App.GetResourceString("String.Downloadwindow.DownloadComplete"),
                        App.GetResourceString("String.Downloadwindow.DownloadComplete2"));
                }
                else
                {
                    this.ShowMessageAsync(App.GetResourceString("String.Downloadwindow.DownloadCompleteWithError"),
                        string.Format(App.GetResourceString("String.Downloadwindow.DownloadCompleteWithError2"), e.ErrorList.Count, e.ErrorList.First().Value.Message));
                }

            }));
        }

        private void Downloader_DownloadSpeedChanged(object sender, Utils.DownloadSpeedChangedArg e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                speedTextBlock.Text = e.SpeedValue.ToString() + e.SpeedUnit;
            }));
        }

        private void Downloader_DownloadProgressChanged(object sender, Utils.DownloadProgressChangedArg e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.progressBar.Maximum = e.TaskCount;
                this.progressBar.Value = e.TaskCount - e.LastTaskCount;
                this.progressPerTextBlock.Text = ((double)(e.TaskCount - e.LastTaskCount) / (double)e.TaskCount).ToString("0%");
                Tasks.Remove(e.DoneTask);
            }));
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync(App.GetResourceString("String.Downloadwindow.MakesureCancel"),
                App.GetResourceString("String.Downloadwindow.MakesureCancel"), 
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings() { AffirmativeButtonText = App.GetResourceString("String.Base.Yes"),
                    NegativeButtonText = App.GetResourceString("String.Base.Cancel") });
            if (result == MessageDialogResult.Affirmative)
            {
                App.downloader.RequestStop();
                this.progressBar.Value = 0;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new NewDownloadTaskWindow().ShowDialog();
            Refresh();
        }
    }
}

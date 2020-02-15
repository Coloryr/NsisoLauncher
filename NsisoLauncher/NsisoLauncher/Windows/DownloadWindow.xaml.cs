using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Taskbar;
using NsisoLauncherCore.Net;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NsisoLauncher.Windows
{

    /// <summary>
    /// DownloadWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadWindow : MetroWindow
    {
        private ObservableCollection<DownloadTask> Tasks;
        private Timer time;

        public DownloadWindow(bool auto = false)
        {
            InitializeComponent();
            App.Downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
            App.Downloader.DownloadSpeedChanged += Downloader_DownloadSpeedChanged;
            App.Downloader.DownloadCompleted += Downloader_DownloadCompleted;
            Refresh();
            if (auto)
                time = new Timer(new TimerCallback(Time), null, 100, -1);
        }

        private void Time(object state)
        {
            Dispatcher.Invoke(() =>
            {
                NewDownload(null, null);
            });
            time.Dispose();
        }

        public void Refresh()
        {
            Tasks = App.Downloader.DownloadTasks != null ? new ObservableCollection<DownloadTask>(App.Downloader.DownloadTasks) : new ObservableCollection<DownloadTask>();
            downloadList.ItemsSource = Tasks;
        }

        public async Task<DownloadCompletedArg> ShowWhenDownloading()
        {
            Show();
            return await Task.Factory.StartNew(() =>
            {
                DownloadCompletedArg completedArg = null;
                try
                {
                    EventWaitHandle _waitHandle = new AutoResetEvent(false);
                    App.Downloader.DownloadCompleted += (a, b) =>
                    {
                        Dispatcher.Invoke(new Action(Close));
                        _waitHandle.Set();
                        completedArg = b;
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
                return completedArg;
            });
        }

        private void Downloader_DownloadCompleted(object sender, DownloadCompletedArg e)
        {
            Dispatcher.Invoke(new Action(async () =>
            {
                speedTextBlock.Text = "0Kb/s";
                progressBar.Maximum = 1;
                progressBar.Value = 0;
                progressPerTextBlock.Text = "000%";
                if (e.ErrorList == null || e.ErrorList.Count == 0)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Downloadwindow.DownloadComplete"),
                        App.GetResourceString("String.Downloadwindow.DownloadComplete2"));
                    Close();
                }
                else
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Downloadwindow.DownloadCompleteWithError"),
                        string.Format(App.GetResourceString("String.Downloadwindow.DownloadCompleteWithError2"), e.ErrorList.Count, e.ErrorList.First().Value.Message));
                    Close();
                }
            }));
        }

        private void Downloader_DownloadSpeedChanged(object sender, DownloadSpeedChangedArg e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                speedTextBlock.Text = e.SpeedValue.ToString() + e.SpeedUnit;
            }));
        }

        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedArg e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                progressBar.Maximum = e.TaskCount;
                progressBar.Value = e.TaskCount - e.LastTaskCount;
                double progress = ((double)(e.TaskCount - e.LastTaskCount) / (double)e.TaskCount);
                if (progress > 0)
                    TaskbarManager.Instance.SetProgressValue((int)(progress * 100), 100);
                progressPerTextBlock.Text = progress.ToString("0%");
                Tasks.Remove(e.DoneTask);
            }));
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (App.Downloader.IsBusy)
            {
                var result = await this.ShowMessageAsync(App.GetResourceString("String.Downloadwindow.MakesureCancel"),
                    App.GetResourceString("String.Downloadwindow.MakesureCancel"),
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings()
                    {
                        AffirmativeButtonText = App.GetResourceString("String.Base.Yes"),
                        NegativeButtonText = App.GetResourceString("String.Base.Cancel")
                    });
                if (result == MessageDialogResult.Affirmative)
                {
                    App.Downloader.RequestStop();
                    progressBar.Value = 0;
                    this.Close();
                }
            }
            else
            {
                await this.ShowMessageAsync(App.GetResourceString("String.Downloadwindow.Cancel.Title"),
                    App.GetResourceString("String.Downloadwindow.Cancel.Text"));
            }
        }

        private void NewDownload(object sender, RoutedEventArgs e)
        {
            if (App.NewDownloadTaskWindow_ == null)
            {
                App.NewDownloadTaskWindow_ = new NewDownloadTaskWindow(true);
                App.NewDownloadTaskWindow_.Show();
            }
            else
            {
                App.NewDownloadTaskWindow_.Activate();
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.Downloader.IsBusy)
            {
                this.ShowModalMessageExternal(App.GetResourceString("String.Downloadwindow.Closing.Title"),
                    App.GetResourceString("String.Downloadwindow.Closing.Text"));
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (HttpRequesterAPI.Proxy != null)
            {
                this.ShowMessageAsync(App.GetResourceString("String.Downloadwindow.Proxy.Title"),
                    App.GetResourceString("String.Downloadwindow.Proxy.Text"));
            }
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            App.DownloadWindow_ = null;
            App.MainWindow_.Refresh();
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
        }
    }
}

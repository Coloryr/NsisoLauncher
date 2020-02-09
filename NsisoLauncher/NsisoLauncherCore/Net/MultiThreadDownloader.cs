using NsisoLauncherCore.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace NsisoLauncherCore.Net
{
    public class DownloadProgressChangedArg : EventArgs
    {
        public int TaskCount { get; set; }
        public int LastTaskCount { get; set; }
        public DownloadTask DoneTask { get; set; }
    }

    public class DownloadSpeedChangedArg : EventArgs
    {
        public decimal SizePerSec { get; set; }
        public decimal SpeedValue { get; set; }
        public string SpeedUnit { get; set; }
    }

    public class DownloadCompletedArg : EventArgs
    {
        public Dictionary<DownloadTask, Exception> ErrorList { get; set; }
    }

    public class MultiThreadDownloader
    {
        /// <summary>
        /// 初始化一个多线程下载器
        /// </summary>
        public MultiThreadDownloader()
        {
            _timer.Elapsed += _timer_Elapsed;
        }

        /// <summary>
        /// 每秒触发事件（下载速度计算）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DownloadSpeedChangedArg arg = new DownloadSpeedChangedArg
            {
                SizePerSec = _downloadSizePerSec
            };
            if (_downloadSizePerSec > 1048576)
            {
                arg.SpeedUnit = "MB/s";
                arg.SpeedValue = Math.Round((decimal)_downloadSizePerSec / (decimal)1048576, 2);
                DownloadSpeedChanged?.Invoke(this, arg);

            }
            else if (_downloadSizePerSec > 1024)
            {
                arg.SpeedUnit = "KB/s";
                arg.SpeedValue = Math.Round((decimal)_downloadSizePerSec / (decimal)1024, 2);
                DownloadSpeedChanged?.Invoke(this, arg);
            }
            else
            {
                arg.SpeedUnit = "B/s";
                arg.SpeedValue = _downloadSizePerSec;
                DownloadSpeedChanged?.Invoke(this, arg);
            }
            _downloadSizePerSec = 0;
        }

        public int ProcessorSize { get; set; } = 5;
        public bool IsBusy { get; private set; } = false;
        public bool CheckFileHash { get; set; }
        /// <summary>
        /// 重新下载尝试次数
        /// </summary>
        public int RetryTimes { get; set; } = 3;

        public event EventHandler<DownloadProgressChangedArg> DownloadProgressChanged;
        public event EventHandler<DownloadSpeedChangedArg> DownloadSpeedChanged;
        public event EventHandler<DownloadCompletedArg> DownloadCompleted;
        public event EventHandler<Log> DownloadLog;

        public IEnumerable<DownloadTask> DownloadTasks { get => _viewDownloadTasks.AsEnumerable(); }

        private System.Timers.Timer _timer = new System.Timers.Timer(1000);
        private ConcurrentQueue<DownloadTask> _downloadTasks = new ConcurrentQueue<DownloadTask>();
        private List<DownloadTask> _viewDownloadTasks;
        private readonly object _viewDownloadLocker = new object();
        private int _taskCount;
        private Task[] _workers;
        private int _downloadSizePerSec;
        private Dictionary<DownloadTask, Exception> _errorList = new Dictionary<DownloadTask, Exception>();
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// 设置一群下载内容
        /// </summary>
        /// <param name="tasks"></param>
        public void SetDownloadTasks(List<DownloadTask> tasks)
        {
            if (!IsBusy)
            {
                tasks.ForEach(x => _downloadTasks.Enqueue(x));
                _viewDownloadTasks = new List<DownloadTask>(tasks);
                _taskCount = tasks.Count;
            }
        }

        /// <summary>
        /// 设置一个下载内容
        /// </summary>
        /// <param name="task"></param>
        public void SetDownloadTasks(DownloadTask task)
        {
            if (!IsBusy)
            {
                _downloadTasks.Enqueue(task);
                _viewDownloadTasks = new List<DownloadTask>();
                _viewDownloadTasks.Add(task);
                _taskCount = 1;
            }

        }

        /// <summary>
        /// 从预览列表中移除一项下载
        /// </summary>
        /// <param name="task"></param>
        private void RemoveItemFromViewTask(DownloadTask task)
        {
            lock (_viewDownloadLocker)
            {
                _viewDownloadTasks.Remove(task);
            }
        }

        public void RequestStop()
        {
            cancellationTokenSource.Cancel();
            CompleteDownload();
            ApendDebugLog("已申请取消下载");
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        public void StartDownload()
        {
            try
            {
                if (!IsBusy)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    IsBusy = true;
                    _errorList.Clear();
                    if (ProcessorSize == 0)
                    {
                        IsBusy = false;
                        throw new ArgumentException("下载器的线程数不能为0");
                    }
                    if (_downloadTasks == null || _downloadTasks.Count == 0)
                    {
                        IsBusy = false;
                        return;
                    }

                    _workers = new Task[ProcessorSize];
                    _timer.Start();

                    for (int i = 0; i < ProcessorSize; i++)
                    {
                        _workers[i] = Task.Run(() => ThreadDownloadWorkAsync(cancellationTokenSource.Token));
                    }

                    Task.Run(() =>
                    {
                        try
                        {
                            Task.WaitAll(_workers);
                            CompleteDownload();
                            DownloadCompleted?.Invoke(this, new DownloadCompletedArg() { ErrorList = _errorList });
                            return;
                        }
                        catch (Exception ex)
                        {
                            SendFatalLog(ex, "下载监视线程发生异常");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                SendFatalLog(ex, "下载线程分配时发生异常");
            }
        }

        private async Task ThreadDownloadWorkAsync(CancellationToken cancelToken)
        {
            var http = new HttpRequesterAPI(TimeSpan.FromSeconds(10));
            try
            {
                while ((!cancelToken.IsCancellationRequested) && (!_downloadTasks.IsEmpty))
                {
                    if (_downloadTasks.TryDequeue(out DownloadTask item))
                    {

                        item.SetState("下载中");

                        await HTTPDownload(item, cancelToken, http);

                        ApendDebugLog("下载完成:" + item.From);

                        #region 校验
                        if (CheckFileHash && item.Checker != null)
                        {
                            item.SetState("校验中");
                            if (!item.Checker.CheckFilePass())
                            {
                                item.SetState("校验失败");
                                ApendDebugLog(string.Format("{0}校验哈希值失败，目标哈希值:{1}", item.TaskName, item.Checker.CheckSum));
                                File.Delete(item.To);
                                if (!_errorList.ContainsKey(item))
                                {
                                    _errorList.Add(item, new Exception("文件校验失败"));
                                }
                            }
                            else
                            {
                                item.SetState("校验成功");
                                ApendDebugLog(string.Format("{0}校验哈希值成功:{1}", item.TaskName, item.Checker.CheckSum));
                            }
                        }
                        #endregion

                        #region 执行下载后函数
                        if (item.Todo != null)
                        {
                            if (!cancelToken.IsCancellationRequested)
                            {
                                ApendDebugLog(string.Format("开始执行{0}下载后的安装过程", item.TaskName));
                                item.SetState("安装中");

                                ProgressCallback callback = new ProgressCallback();
                                callback.ProgressChanged += item.AcceptProgressChangedArg;
                                try
                                {
                                    Exception exc = await Task.Run(() => item.Todo(callback, cancelToken));
                                    if (exc != null)
                                    {
                                        SendDownloadErrLog(item, exc);
                                        if (!_errorList.ContainsKey(item))
                                        {
                                            _errorList.Add(item, exc);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SendDownloadErrLog(item, ex);
                                    if (!_errorList.ContainsKey(item))
                                    {
                                        _errorList.Add(item, ex);
                                    }
                                }
                                finally
                                {
                                    callback.ProgressChanged -= item.AcceptProgressChangedArg;
                                }
                            }
                            else
                            {
                                ApendDebugLog("放弃安装:" + item.TaskName);
                            }
                        }
                        #endregion

                        item.SetDone();
                        RemoveItemFromViewTask(item);
                        DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArg() { TaskCount = _taskCount, LastTaskCount = _viewDownloadTasks.Count, DoneTask = item });
                    }
                }
            }
            catch (Exception ex)
            {
                SendFatalLog(ex, "下载线程工作时发生异常");
            }

        }

        private async Task HTTPDownload(DownloadTask task, CancellationToken cancelToken, HttpRequesterAPI http)
        {

            if (string.IsNullOrWhiteSpace(task.From) || string.IsNullOrWhiteSpace(task.To))
            {
                return;
            }

            string realFilename = task.To;
            string buffFilename = realFilename + ".downloadtask";
            Exception exception = null;

            for (int i = 1; i <= RetryTimes; i++)
            {
                try
                {
                    //下载前文件准备
                    if (Path.IsPathRooted(realFilename))
                    {
                        string dirName = Path.GetDirectoryName(realFilename);
                        if (!Directory.Exists(dirName))
                        {
                            Directory.CreateDirectory(dirName);
                        }
                    }
                    if (File.Exists(realFilename))
                    {
                        return;
                    }
                    if (File.Exists(buffFilename))
                    {
                        File.Delete(buffFilename);
                    }

                    //下载流程
                    using (var getResult = await http.HttpGetAsync(task.From, cancelToken))
                    {
                        getResult.EnsureSuccessStatusCode();
                        task.SetTotalSize(getResult.Content.Headers.ContentLength.GetValueOrDefault());
                        using (Stream responseStream = await getResult.Content.ReadAsStreamAsync())
                        {
                            using (FileStream fs = new FileStream(buffFilename, FileMode.Create))
                            {
                                byte[] bArr = new byte[1024];
                                int size = await responseStream.ReadAsync(bArr, 0, (int)bArr.Length);

                                while (size > 0)
                                {
                                    //_pauseResetEvent.Wait(cancelToken);
                                    await fs.WriteAsync(bArr, 0, size);
                                    size = await responseStream.ReadAsync(bArr, 0, (int)bArr.Length);
                                    _downloadSizePerSec += size;
                                    task.IncreaseDownloadSize(size);
                                }
                            }
                        }
                    }

                    //下载完成后转正
                    File.Move(buffFilename, realFilename);

                    //无错误跳出重试循环
                    exception = null;
                    break;
                }
                catch (Exception e)
                {
                    exception = e;
                    task.SetState(string.Format("重试第{0}次", i));
                    SendDownloadErrLog(task, e);

                    //继续重试
                    continue;
                }
            }
            //处理异常
            if (exception != null)
            {
                SendDownloadErrLog(task, exception);
                if (!_errorList.ContainsKey(task))
                {
                    _errorList.Add(task, exception);
                }
            }
        }

        private void CompleteDownload()
        {
            _timer.Stop();
            _taskCount = 0;
            _downloadSizePerSec = 0;
            IsBusy = false;
            ApendDebugLog("全部下载任务已完成");
        }

        private void ApendDebugLog(string msg)
        {
            this.DownloadLog?.Invoke(this, new Log() { LogLevel = LogLevel.DEBUG, Message = msg });
        }

        private void SendLog(Log e)
        {
            DownloadLog?.Invoke(this, e);
        }

        private void SendFatalLog(Exception ex, string msg)
        {
            SendLog(new Log() { Exception = ex, LogLevel = LogLevel.FATAL, Message = msg });
        }

        private void SendDownloadErrLog(DownloadTask task, Exception ex)
        {
            SendLog(new Log() { Exception = ex, LogLevel = LogLevel.ERROR, Message = string.Format("任务{0}下载失败,源地址:{1}原因:{2}", task.TaskName, task.From, ex.Message) });
        }

        private Dictionary<string, string> Maplist = new Dictionary<string, string>()
        {
            {@"http://files.minecraftforge.net/maven/net/minecraftforge/forge/1.7.10-10.13.4.1614-1.7.10/forge-1.7.10-10.13.4.1614-1.7.10.jar"
            ,@"https://files.minecraftforge.net/maven/net/minecraftforge/forge/1.7.10-10.13.4.1614-1.7.10/forge-1.7.10-10.13.4.1614-1.7.10-universal.jar"}
        };
    }
}

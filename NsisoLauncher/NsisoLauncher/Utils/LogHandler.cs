using NsisoLauncherCore.Modules;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncher.Core.Util
{
    public class LogHandler : IDisposable
    {
        public bool WriteToFile { get; set; }
        public event EventHandler<Log> OnLog;
        private readonly ReaderWriterLockSlim LogLock = new ReaderWriterLockSlim();

        public LogHandler(bool writetofile = false)
        {
            WriteToFile = writetofile;
        }

        public void AppendLog(object sender, Log log)
        {
            OnLog?.Invoke(sender, log);
            if (WriteToFile)
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        LogLock.EnterWriteLock();
                        try
                        {
                            if (log.LogLevel == LogLevel.GAME)
                            {
                                File.AppendAllText("log.txt", string.Format("[GAME]{0}\r\n", log.Message));
                            }
                            else
                            {
                                File.AppendAllText("log.txt", string.Format("[{0}][{1}]{2}\r\n", DateTime.Now.ToString(), log.LogLevel, log.Message));
                            }
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            LogLock.ExitWriteLock();
                        }
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
        }

        public void AppendDebug(string msg)
        {
            AppendLog(this, new Log() { LogLevel = LogLevel.DEBUG, Message = msg });
        }

        public void AppendInfo(string msg)
        {
            AppendLog(this, new Log() { LogLevel = LogLevel.INFO, Message = msg });
        }

        public void AppendWarn(string msg)
        {
            AppendLog(this, new Log() { LogLevel = LogLevel.WARN, Message = msg });
        }

        public void AppendError(Exception e)
        {
            AppendLog(this, new Log() { LogLevel = LogLevel.ERROR, Message = e.ToString() });
        }

        public void AppendFatal(Exception e)
        {
            AppendLog(this, new Log() { LogLevel = LogLevel.FATAL, Message = e.ToString() });
            new Windows.ErrorWindow(e).Show();
        }

        public void Dispose()
        {
            LogLock.Dispose();
        }
    }
}

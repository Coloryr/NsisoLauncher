﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Core.Util;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// ErrorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorWindow : MetroWindow
    {
        private const string errorApiAdress = "http://www.nsiso.com/api/Public/nsisoapi/";

        BackgroundWorker updateThread = new BackgroundWorker();

        string[] funny = {
            "你所不知道的事实：参与这个启动器的开发者只有一个人，而且整个开发工作室也只有这一个人",
            "崩溃了？不要急，看见你的键盘了吗？看见了吗？现在把它拿起来。拿起来的对吧？，现在去砸显示器",
            "还记得世界末日这个脑洞大开的电影吗？这个崩溃不会造成那样的后果的，放心",
            "其实这个启动器里面最大的彩蛋就是这个错误窗口的槽点栏。每次都会随机生成。就比如说这条",
            "这个启动器发生意外的原因是：开发者正在陪他的老婆，而且对这个启动器的修复不闻不问（伪",
            "如果你打算拿这个窗口左下角的联系方式来骚扰我的话，那么再见。当然除非你是妹子，那我不会介意",
            "其实开发者在开发新版本的时候还是比较懒的，有些内容还是旧版本里的，比如说这个错误窗口！",
            "新版本的异常处理貌似不错！崩溃是不会直接崩溃了，但问题是这个异常什么时候搞定？",
            "好心人，拜托上传个错误报告吧，会有美女找你的"
        };

        public ErrorWindow(Exception ex)
        {
            InitializeComponent();
            updateThread.DoWork += UpdateThread_DoWork;
            updateThread.RunWorkerCompleted += UpdateThread_RunWorkerCompleted;
            Random random = new Random();
            FunnyBlock.Text = funny[random.Next(funny.Count())];
            this.textBox.Text = ex.ToString();
            AppendInfo();
        }

        private void UpdateThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var args = Environment.GetCommandLineArgs();
            foreach (var item in args)
            {
                info.Arguments += (item + ' ');
            }
            info.Arguments += "-rebootByError";
            System.Diagnostics.Process.Start(info);
            App.Current.Shutdown();
        }

        private void UpdateThread_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string err = Uri.EscapeDataString((string)e.Argument);
                string pa = "service=Default.Report&text=" + err;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(errorApiAdress);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                byte[] data = Encoding.UTF8.GetBytes(pa);
                req.ContentLength = data.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        //作者邮箱点击后
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:siso@nsiso.com");
        }

        //作者qq点击后
        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://wpa.qq.com/msgrd?v=3&uin=2081964100&site=qq&menu=yes");
        }

        //作者github点击后
        private void Hyperlink_Click_2(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Nsiso");
        }

        private void RebootButton_Click(object sender, RoutedEventArgs e)
        {
            loadingGrid.Visibility = Visibility.Visible;
            updateThread.RunWorkerAsync(this.textBox.Text);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(textBox.Text);
            this.ShowMessageAsync("复制成功", "你现在可以点击窗口左下角作者联系方式，并把这该死的错误抛给他");
        }

        private void AppendInfo()
        {
            textBox.AppendText("\r\n==========环境信息==========");
            textBox.AppendText("\r\nCPU信息:" + SystemTools.GetProcessorInfo());
            textBox.AppendText("\r\n内存信息: 总大小:" + SystemTools.GetTotalMemory().ToString() + "MB/可用大小:" + SystemTools.GetRunmemory().ToString() + "MB");
            textBox.AppendText("\r\n显卡信息:" + SystemTools.GetVideoCardInfo());
            textBox.AppendText("\r\n操作系统:" + Environment.OSVersion.Platform);
            textBox.AppendText("\r\n版本号:" + Environment.OSVersion.VersionString);
            textBox.AppendText("\r\n系统位数:" + SystemTools.GetSystemArch());
            textBox.AppendText("\r\n程序运行命令行:" + Environment.CommandLine);
            textBox.AppendText("\r\n程序工作目录:" + Environment.CurrentDirectory);
        }
    }
}

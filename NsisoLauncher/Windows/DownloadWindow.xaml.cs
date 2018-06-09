﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using NsisoLauncher.Core.Net;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// DownloadWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadWindow : MetroWindow
    {
        public DownloadWindow()
        {
            InitializeComponent();
            downloadList.ItemsSource = App.downloader.TasksObservableCollection;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Core.Util;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : MetroWindow
    {
        private Config.MainConfig config;

        public SettingWindow()
        {
            InitializeComponent();

            Refresh();
        }

        private async void Refresh()
        {
            config = DeepCloneObject(App.config.MainConfig);
            debugCheckBox.DataContext = config.Launcher;
            javaPathComboBox.ItemsSource = App.javaList;
            environmentGrid.DataContext = config.Environment;
            downloadGrid.DataContext = config.Download;
            memorySlider.Maximum = SystemTools.GetTotalMemory();
            VersionsComboBox.ItemsSource = await App.handler.GetVersionsAsync();
            customGrid.DataContext = config.Customize;
            AccentColorComboBox.ItemsSource = ThemeManager.Accents;
            appThmeComboBox.ItemsSource = ThemeManager.AppThemes;
            serverGroupBox.DataContext = config.Server;
            userGrid.DataContext = config.User;
        }

        private async void chooseJavaButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "选择Java",
                Filter = "Java应用程序(无窗口)|javaw.exe|Java应用程序(含窗口)|java.exe",
            };
            if (dialog.ShowDialog() == true)
            {
                Java java = await Java.GetJavaInfoAsync(dialog.FileName);
                if (java!=null)
                {
                    this.javaPathComboBox.Text = java.Path;
                    this.javaInfoLabel.Content = string.Format("Java版本：{0}，位数：{1}", java.Version, java.Arch);
                }
                else
                {
                    this.javaPathComboBox.Text = dialog.FileName;
                    await this.ShowMessageAsync("选择的Java无法正确获取信息", "请确认您选择的是正确的Java应用");
                }
            }
        }

        private void gamedirChooseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog()
            {
                Description = "选择游戏运行根目录",
                ShowNewFolderButton = true
            };
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            else
            {
                gamedirPathTextBox.Text = dialog.SelectedPath.Trim();
                config.Environment.GamePath = dialog.SelectedPath.Trim();
            }
        }
        #region 全局设置部分
        private void memorySlider_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            config.Environment.MaxMemory = Convert.ToInt32(((RangeSlider)sender).UpperValue);
        }

        private void memorySlider_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            config.Environment.MinMemory = Convert.ToInt32(((RangeSlider)sender).LowerValue);
        }
        #endregion

        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        //保存按钮点击后
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            App.config.MainConfig = config;
            GameHelper.SaveOptions(
                (List<VersionOption>)versionOptionsGrid.ItemsSource,
                App.handler,
                (Core.Modules.Version)VersionsComboBox.SelectedItem);
            App.config.Save();
            this.ShowMessageAsync("保存成功", "所有设置已成功保存在本地");
        }

        //取消按钮点击后
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void VersionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox comboBox = (System.Windows.Controls.ComboBox)sender;

            if (comboBox.SelectedItem != null)
            {
                versionOptionsGrid.ItemsSource = GameHelper.GetOptions(App.handler, (Core.Modules.Version)comboBox.SelectedItem);
            }
            else
            {
                versionOptionsGrid.ItemsSource = null;
            }
        }

        private async void refreshVersionsButton_Click(object sender, RoutedEventArgs e)
        {
            VersionsComboBox.ItemsSource = await App.handler.GetVersionsAsync();
        }

        private void AccentColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Accent item = (Accent)((System.Windows.Controls.ComboBox)sender).SelectedItem;
            if (item != null)
            {
                var AppStyle = ThemeManager.DetectAppStyle(System.Windows.Application.Current);
                var theme = AppStyle.Item1;
                ThemeManager.ChangeAppStyle(System.Windows.Application.Current, ThemeManager.GetAccent(item.Name), theme);
            }
        }

        private void appThmeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppTheme item = (AppTheme)((System.Windows.Controls.ComboBox)sender).SelectedItem;
            if (item != null)
            {
                var AppStyle = ThemeManager.DetectAppStyle(System.Windows.Application.Current);
                var accent = AppStyle.Item2;
                ThemeManager.ChangeAppStyle(System.Windows.Application.Current, accent, item);
            }
        }

        private static T DeepCloneObject<T>(T t) where T : class
        {
            T model = Activator.CreateInstance<T>();                     //实例化一个T类型对象
            PropertyInfo[] propertyInfos = model.GetType().GetProperties();     //获取T对象的所有公共属性
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                //判断值是否为空，如果空赋值为null见else
                if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    //如果convertsionType为nullable类，声明一个NullableConverter类，该类提供从Nullable类到基础基元类型的转换
                    NullableConverter nullableConverter = new NullableConverter(propertyInfo.PropertyType);
                    //将convertsionType转换为nullable对的基础基元类型
                    propertyInfo.SetValue(model, Convert.ChangeType(propertyInfo.GetValue(t,null), nullableConverter.UnderlyingType), null);
                }
                else
                {
                    propertyInfo.SetValue(model, Convert.ChangeType(propertyInfo.GetValue(t,null), propertyInfo.PropertyType), null);
                }
            }
            return model;
        }

        private void javaPathComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Java java = (Java)(((System.Windows.Controls.ComboBox)sender).SelectedItem);
            if (java != null)
            {
                this.javaInfoLabel.Content = string.Format("Java版本：{0}，位数：{1}", java.Version, java.Arch);
            }
            else
            {
                this.javaInfoLabel.Content = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            config.User.AccessToken = null;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            config.User = new Config.User()
            {
                AuthenticationType = Config.AuthenticationType.OFFLINE,
                ClientToken = Guid.NewGuid().ToString("N"),
            };
        }
    }
}

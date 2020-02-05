using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : MetroWindow
    {
        private MainConfig config;

        private bool _isGameSettingChanged = false;

        public SettingWindow()
        {
            InitializeComponent();
            FirstBinding();
            Refresh();
        }

        private void FirstBinding()
        {
            AccentColorComboBox.ItemsSource = ThemeManager.Accents;
            appThmeComboBox.ItemsSource = ThemeManager.AppThemes;
            authModuleCombobox.ItemsSource = authModules;
            versionTextBlock.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private async void Refresh()
        {
            //深度克隆设置
            config = DeepCloneObject(App.Config.MainConfig);

            //绑定content设置
            this.DataContext = config;

            javaPathComboBox.ItemsSource = App.JavaList;
            memorySlider.Maximum = SystemTools.GetTotalMemory();

            authModules.Clear();
            foreach (var item in config.User.AuthenticationDic)
            {
                if (item.Key == "offline" || item.Key == "online")
                    continue;
                authModules.Add(new KeyValuePair<string, AuthenticationNode>(item.Key, item.Value));
            }

            VersionsComboBox.ItemsSource = await App.Handler.GetVersionsAsync();

            Lauguage.ItemsSource = new List<string>() { "中文", "日本語", "English" };
            Lauguage.SelectedItem = config.Lauguage;

            if (App.Config.MainConfig.Environment.VersionIsolation)
            {
                VersionChose.Visibility = Visibility.Visible;
            }
            else
            {
                VersionChose.Visibility = Visibility.Collapsed;
                versionOptionsGrid.ItemsSource = await GameHelper.GetOptionsAsync(VersionOption.Type.options, App.Handler, new MCVersion() { ID = "null" });
                versionOptionsofGrid.ItemsSource = await GameHelper.GetOptionsAsync(VersionOption.Type.optionsof, App.Handler, new MCVersion() { ID = "null" });
            }
            CheckBox_Checked(null, null);
        }

        public void ShowAddAuthModule()
        {
            tabControl.SelectedIndex = 3;
            addAuthModuleExpander.IsExpanded = true;
            addAuthModuleExpander.Focus();
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
                if (java != null)
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
        private void memorySlider_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            config.Environment.MaxMemory = Convert.ToInt32(((RangeSlider)sender).UpperValue);
        }

        private void memorySlider_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            config.Environment.MinMemory = Convert.ToInt32(((RangeSlider)sender).LowerValue);
        }

        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        //保存按钮点击后
        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            switch (config.Environment.GamePathType)
            {
                case GameDirEnum.ROOT:
                    App.Handler.GameRootPath = Path.GetFullPath(".minecraft");
                    break;
                case GameDirEnum.APPDATA:
                    App.Handler.GameRootPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                    break;
                case GameDirEnum.PROGRAMFILES:
                    App.Handler.GameRootPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles) + "\\.minecraft";
                    break;
                case GameDirEnum.CUSTOM:
                    App.Handler.GameRootPath = config.Environment.GamePath + "\\.minecraft";
                    break;
                default:
                    throw new ArgumentException("判断游戏目录类型时出现异常，请检查配置文件中GamePathType节点");
            }
            App.Handler.VersionIsolation = config.Environment.VersionIsolation;
            App.Downloader.CheckFileHash = config.Download.CheckDownloadFileHash;

            App.Config.MainConfig = config;

            if (_isGameSettingChanged)
            {
                if (App.Config.MainConfig.Environment.VersionIsolation)
                {
                    await GameHelper.SaveOptionsAsync(VersionOption.Type.options,
                    (List<VersionOption>)versionOptionsGrid.ItemsSource,
                    App.Handler,
                    (NsisoLauncherCore.Modules.MCVersion)VersionsComboBox.SelectedItem);

                    await GameHelper.SaveOptionsAsync(VersionOption.Type.optionsof,
                   (List<VersionOption>)versionOptionsofGrid.ItemsSource,
                   App.Handler,
                   (NsisoLauncherCore.Modules.MCVersion)VersionsComboBox.SelectedItem);
                }
                else
                {
                    await GameHelper.SaveOptionsAsync(VersionOption.Type.options,
                    (List<VersionOption>)versionOptionsGrid.ItemsSource,
                    App.Handler,
                    new NsisoLauncherCore.Modules.MCVersion() { ID = "null" });
                    await GameHelper.SaveOptionsAsync(VersionOption.Type.optionsof,
                    (List<VersionOption>)versionOptionsofGrid.ItemsSource,
                    App.Handler,
                    new NsisoLauncherCore.Modules.MCVersion() { ID = "null" });
                }
            }

            App.Config.Save();
            saveButton.Content = App.GetResourceString("String.Settingwindow.Saving");
            Config.Environment env = App.Config.MainConfig.Environment;
            Java java = null;
            if (env.AutoJava)
            {
                java = Java.GetSuitableJava(App.JavaList);
            }
            else
            {
                java = App.JavaList.Find(x => x.Path == env.JavaPath);
                if (java == null)
                {
                    java = Java.GetJavaInfo(env.JavaPath);
                }
            }
            if (java != null)
            {
                App.Handler.Java = java;
                Close();
            }
            else
            {
                await this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.SaveError"), App.GetResourceString("String.Settingwindow.JavaError"));
            }
        }

        //取消按钮点击后
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void VersionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox comboBox = (System.Windows.Controls.ComboBox)sender;

            if (comboBox.SelectedItem != null)
            {
                versionOptionsGrid.ItemsSource = await GameHelper.GetOptionsAsync(VersionOption.Type.options, App.Handler, (MCVersion)comboBox.SelectedItem);
                versionOptionsofGrid.ItemsSource = await GameHelper.GetOptionsAsync(VersionOption.Type.optionsof, App.Handler, (MCVersion)comboBox.SelectedItem);
            }
            else
            {
                versionOptionsGrid.ItemsSource = null;
            }
        }

        private async void refreshVersionsButton_Click(object sender, RoutedEventArgs e)
        {
            VersionsComboBox.ItemsSource = await App.Handler.GetVersionsAsync();
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
                    propertyInfo.SetValue(model, Convert.ChangeType(propertyInfo.GetValue(t, null), nullableConverter.UnderlyingType), null);
                }
                else
                {
                    propertyInfo.SetValue(model, Convert.ChangeType(propertyInfo.GetValue(t, null), propertyInfo.PropertyType), null);
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

        private void forgetUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userComboBox.SelectedItem == null)
            {
                this.ShowMessageAsync("您未选择要进行操作的用户", "请先选择您要进行操作的用户");
                return;
            }
            KeyValuePair<string, UserNode> selectedItem = (KeyValuePair<string, UserNode>)userComboBox.SelectedItem;
            UserNode node = selectedItem.Value;
            //todo （后）恢复注销用户功能
            node.AccessToken = null;
            this.ShowMessageAsync("注销成功", "请保存以生效");
        }

        private void clearUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userComboBox.SelectedItem == null)
            {
                this.ShowMessageAsync("您未选择要进行操作的用户", "请先选择您要进行操作的用户");
                return;
            }

            KeyValuePair<string, UserNode> selectedItem = (KeyValuePair<string, UserNode>)userComboBox.SelectedItem;
            UserNode node = selectedItem.Value;
            node.AccessToken = null;
            node.Profiles = null;
            node.UserData = null;
            node.SelectProfileUUID = null;
            this.ShowMessageAsync("重置用户成功", "请保存以生效");
        }

        private void delUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userComboBox.SelectedItem == null)
            {
                this.ShowMessageAsync("您未选择要进行操作的用户", "请先选择您要进行操作的用户");
                return;
            }

            string key = (string)userComboBox.SelectedValue;
            config.User.UserDatabase.Remove(key);
            this.ShowMessageAsync("删除用户成功", "请保存以生效");
        }

        private void delAllAuthnodeButton_Click(object sender, RoutedEventArgs e)
        {
            config.User.AuthenticationDic.Clear();
            config.User.AuthenticationDic.Add("mojang", new AuthenticationNode()
            {
                AuthType = AuthenticationType.MOJANG,
                Name = App.GetResourceString("String.Mainwindow.Auth.Mojang")
            });
            this.ShowMessageAsync("清除成功", "请保存以生效");
        }

        private void delAllUserButton_Click(object sender, RoutedEventArgs e)
        {
            config.User.UserDatabase.Clear();
            this.ShowMessageAsync("清除成功", "请保存以生效");
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            // 激活的是当前默认的浏览器
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }

        ObservableCollection<KeyValuePair<string, AuthenticationNode>> authModules = new ObservableCollection<KeyValuePair<string, AuthenticationNode>>();

        private void AuthModuleCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object selectedItem = authModuleCombobox.SelectedItem;
            if (selectedItem == null)
            {
                authmoduleControl.ClearAll();
            }
            else
            {
                authmoduleControl.SelectionChangedAccept((KeyValuePair<string, AuthenticationNode>)selectedItem);
            }
        }

        public async void AddAuthModule(string name, AuthenticationNode authmodule)
        {
            if (authModules.Any(x => x.Key == name))
            {
                await this.ShowMessageAsync("添加的验证模型名称已存在", "您可以尝试更换可用的验证模型名称");
                return;
            }
            var item = new KeyValuePair<string, AuthenticationNode>(name, authmodule);
            authModules.Add(item);
            config.User.AuthenticationDic.Add(name, authmodule);
            authModuleCombobox.SelectedItem = item;
        }

        public async void SaveAuthModule(KeyValuePair<string, AuthenticationNode> node)
        {
            await this.ShowMessageAsync("保存更改", "已修改你的设置，点击应用保存");
        }

        public async void DeleteAuthModule(KeyValuePair<string, AuthenticationNode> node)
        {
            authModules.Remove(node);
            config.User.AuthenticationDic.Remove(node.Key);
            await this.ShowMessageAsync("删除成功", "记得点击应用按钮保存噢");
        }

        private void ClearAuthselectButton_Click(object sender, RoutedEventArgs e)
        {
            authModuleCombobox.SelectedItem = null;
        }

        private void clearAllauthButton_Click(object sender, RoutedEventArgs e)
        {
            NIDE8_C.Visibility = Visibility.Collapsed;
            lockauthCombobox.SelectedItem = null;
        }

        private void VersionOptionsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _isGameSettingChanged = true;
        }

        private void VersionOptionsofGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _isGameSettingChanged = true;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox1.IsChecked == true)
            {
                checkBox2.IsEnabled = false;
                checkBox3.IsEnabled = false;
                checkBox1_s.IsEnabled = true;
                set1.IsEnabled = true;
                Text1.IsEnabled = true;
            }
            else if (checkBox1.IsChecked == false)
            {
                checkBox1_s.IsEnabled = false;
                if (checkBox2.IsChecked == true)
                {
                    checkBox2_s.IsEnabled = true;
                    set1.IsEnabled = true;
                    Text1.IsEnabled = true;
                }
                if (checkBox2.IsChecked == false)
                {
                    checkBox2_s.IsEnabled = false;
                    set1.IsEnabled = false;
                    Text1.IsEnabled = false;
                }
                if (checkBox3.IsChecked == true)
                {
                    checkBox3_s.IsEnabled = true;
                    if (checkBox3_s.IsChecked == true)
                    {
                        set.IsEnabled = true;
                        Text.IsEnabled = true;
                    }
                    else if (checkBox3_s.IsChecked == false)
                    {
                        set.IsEnabled = false;
                        Text.IsEnabled = false;
                    }
                }
                if (checkBox3.IsChecked == false)
                {
                    checkBox3_s.IsEnabled = true;
                    set.IsEnabled = false;
                    Text.IsEnabled = false;
                }
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(Text.Text, out int number))
            {
                e.Handled = true;
            }
        }

        private void lockauthCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic Node = lockauthCombobox.SelectedItem;
            NIDE8_C.Visibility = Node.Value.AuthType == AuthenticationType.NIDE8 ? Visibility.Visible : Visibility.Collapsed;

        }
    }
}

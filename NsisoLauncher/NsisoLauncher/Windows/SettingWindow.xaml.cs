using ControlzEx.Theming;
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

        private bool _isGameSettingChanged = false;
        bool isload = true;

        public SettingWindow()
        {
            InitializeComponent();
            FirstBinding();
            Refresh();
        }

        private void FirstBinding()
        {
            appThmeComboBox.ItemsSource = ThemeManager.Current.Themes;
            authModuleCombobox.ItemsSource = authModules;
            versionTextBlock.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private async void Refresh()
        {

            //绑定content设置
            this.DataContext = App.Config.MainConfig;

            javaPathComboBox.ItemsSource = App.JavaList;
            memorySlider.Maximum = SystemTools.GetTotalMemory();

            authModules.Clear();
            foreach (var item in App.Config.MainConfig.User.AuthenticationDic)
            {
                if (item.Key == "offline" || item.Key == "online")
                    continue;
                authModules.Add(new KeyValuePair<string, AuthenticationNode>(item.Key, item.Value));
            }

            VersionsComboBox.ItemsSource = await App.Handler.GetVersionsAsync();

            Lauguage.ItemsSource = new List<string>() { "中文", "日本語", "English" };
            Lauguage.SelectedItem = App.Config.MainConfig.Lauguage;

            if (App.Config.MainConfig.Environment.VersionIsolation)
                VersionChose.Visibility = Visibility.Visible;
            else
            {
                VersionChose.Visibility = Visibility.Collapsed;
                versionOptionsGrid.ItemsSource = await GameHelper.GetOptionsAsync(VersionOption.Type.options, App.Handler, new MCVersion() { ID = "null" });
                versionOptionsofGrid.ItemsSource = await GameHelper.GetOptionsAsync(VersionOption.Type.optionsof, App.Handler, new MCVersion() { ID = "null" });
            }
            CheckBox_Checked(null, null);
            checkBox2_s_Check(null, null);
            isload = false;
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
                Title = App.GetResourceString("String.Settingwindow.Message.Java.Title"),
                Filter = App.GetResourceString("String.Settingwindow.Message.Java.Filter"),
            };
            if (dialog.ShowDialog() == true)
            {
                Java java = await Java.GetJavaInfoAsync(dialog.FileName);
                if (java != null)
                {
                    this.javaPathComboBox.Text = java.Path;
                    this.javaInfoLabel.Content = string.Format(
                        App.GetResourceString("String.Settingwindow.Message.Java.Content"),
                        java.Version, java.Arch);
                }
                else
                {
                    this.javaPathComboBox.Text = dialog.FileName;
                    await this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.Message.NoJava.Title"),
                        App.GetResourceString("String.Settingwindow.Message.NoJava.Text"));
                }
            }
        }

        private void gamedirChooseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog()
            {
                Description = App.GetResourceString("String.Settingwindow.Message.Game.Description"),
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
                App.Config.MainConfig.Environment.GamePath = dialog.SelectedPath.Trim();
            }
        }
        private void memorySlider_UpperValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            App.Config.MainConfig.Environment.MaxMemory = Convert.ToInt32(((RangeSlider)sender).UpperValue);
        }

        private void memorySlider_LowerValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            App.Config.MainConfig.Environment.MinMemory = Convert.ToInt32(((RangeSlider)sender).LowerValue);
        }

        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        //保存按钮点击后
        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            switch (App.Config.MainConfig.Environment.GamePathType)
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
                    App.Handler.GameRootPath = App.Config.MainConfig.Environment.GamePath + "\\.minecraft";
                    break;
                default:
                    throw new ArgumentException(App.GetResourceString("String.Settingwindow.Message.GamePathType.Error"));
            }
            App.Handler.VersionIsolation = App.Config.MainConfig.Environment.VersionIsolation;
            App.Downloader.CheckFileHash = App.Config.MainConfig.Download.CheckDownloadFileHash;

            if (_isGameSettingChanged)
            {
                if (App.Config.MainConfig.Environment.VersionIsolation)
                {
                    await GameHelper.SaveOptionsAsync(VersionOption.Type.options,
                    (List<VersionOption>)versionOptionsGrid.ItemsSource,
                    App.Handler,
                    (MCVersion)VersionsComboBox.SelectedItem);

                    await GameHelper.SaveOptionsAsync(VersionOption.Type.optionsof,
                   (List<VersionOption>)versionOptionsofGrid.ItemsSource,
                   App.Handler,
                   (MCVersion)VersionsComboBox.SelectedItem);
                }
                else
                {
                    await GameHelper.SaveOptionsAsync(VersionOption.Type.options,
                    (List<VersionOption>)versionOptionsGrid.ItemsSource,
                    App.Handler,
                    new MCVersion() { ID = "null" });
                    await GameHelper.SaveOptionsAsync(VersionOption.Type.optionsof,
                    (List<VersionOption>)versionOptionsofGrid.ItemsSource,
                    App.Handler,
                    new MCVersion() { ID = "null" });
                }
            }

            App.Config.Save();
            App.ProxyRe();

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
                await this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.SaveError"),
                    App.GetResourceString("String.Settingwindow.JavaError"));
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

        private void appThmeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Theme item = (Theme)((System.Windows.Controls.ComboBox)sender).SelectedItem;
            if (item != null)
            {
                ThemeManager.Current.ChangeTheme(System.Windows.Application.Current, item);
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
                this.javaInfoLabel.Content = string.Format(
                    App.GetResourceString("String.Settingwindow.Message.Java.Content"),
                    java.Version, java.Arch);
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
                this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.Message.User.Title"),
                        App.GetResourceString("String.Settingwindow.Message.User.Text"));
                return;
            }
            KeyValuePair<string, UserNode> selectedItem = (KeyValuePair<string, UserNode>)userComboBox.SelectedItem;
            UserNode node = selectedItem.Value;
            node.AccessToken = null;
            this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.Message.User1.Title"),
                App.GetResourceString("String.Settingwindow.Message.User1.Text"));
        }

        private void clearUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userComboBox.SelectedItem == null)
            {
                this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.Message.User2.Title"),
                        App.GetResourceString("String.Settingwindow.Message.User2.Text"));
                return;
            }

            KeyValuePair<string, UserNode> selectedItem = (KeyValuePair<string, UserNode>)userComboBox.SelectedItem;
            UserNode node = selectedItem.Value;
            node.AccessToken = null;
            node.Profiles = null;
            node.UserData = null;
            node.SelectProfileUUID = null;
            this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.Message.User3.Title"),
                 App.GetResourceString("String.Settingwindow.Message.User3.Text"));
        }

        private void delUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userComboBox.SelectedItem == null)
            {
                this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.Message.User4.Title"),
                 App.GetResourceString("String.Settingwindow.Message.User4.Text"));
                return;
            }

            string key = (string)userComboBox.SelectedValue;
            App.Config.MainConfig.User.UserDatabase.Remove(key);
            this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.Message.User5.Title"),
                 App.GetResourceString("String.Settingwindow.Message.User5.Text"));
        }

        private void delAllAuthnodeButton_Click(object sender, RoutedEventArgs e)
        {
            App.Config.MainConfig.User.AuthenticationDic.Clear();
            App.Config.MainConfig.User.AuthenticationDic.Add("mojang", new AuthenticationNode()
            {
                AuthType = AuthenticationType.MOJANG,
                Name = App.GetResourceString("String.Mainwindow.Auth.Mojang")
            });
            this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.Message.User6.Title"),
                 App.GetResourceString("String.Settingwindow.Message.User6.Text"));
        }

        private void delAllUserButton_Click(object sender, RoutedEventArgs e)
        {
            App.Config.MainConfig.User.UserDatabase.Clear();
            this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.Message.User6.Title"),
                 App.GetResourceString("String.Settingwindow.Message.User6.Text"));
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
                await this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.Message.User7.Title"),
                 App.GetResourceString("String.Settingwindow.Message.User7.Text"));
                return;
            }
            var item = new KeyValuePair<string, AuthenticationNode>(name, authmodule);
            authModules.Add(item);
            App.Config.MainConfig.User.AuthenticationDic.Add(name, authmodule);
            authModuleCombobox.SelectedItem = item;
        }

        public async void SaveAuthModule(KeyValuePair<string, AuthenticationNode> node)
        {
            await this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.Message.User8.Title"),
                 App.GetResourceString("String.Settingwindow.Message.User8.Text"));
        }

        public async void DeleteAuthModule(KeyValuePair<string, AuthenticationNode> node)
        {
            authModules.Remove(node);
            App.Config.MainConfig.User.AuthenticationDic.Remove(node.Key);
            await this.ShowMessageAsync(App.GetResourceString("String.Settingwindow.Message.User9.Title"),
                App.GetResourceString("String.Settingwindow.Message.User9.Text"));
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
                checkBox4.IsEnabled = false;
                checkBox1_s.IsEnabled = true;
                checkBox1_r.IsEnabled = true;
                set1.IsEnabled = true;
                Text1.IsEnabled = true;
            }
            else if (checkBox1.IsChecked == false)
            {
                checkBox1_s.IsEnabled = false;
                checkBox1_r.IsEnabled = false;
                checkBox2.IsEnabled = true;
                checkBox3.IsEnabled = true;
                checkBox4.IsEnabled = true;
                if (checkBox2.IsChecked == true || checkBox3.IsChecked == true)
                {
                    checkBox1.IsEnabled = false;
                }
                else
                {
                    checkBox1.IsEnabled = true;
                }
                if (checkBox2.IsChecked == true)
                {
                    checkBox2_s.IsEnabled = true;
                    checkBox2_r.IsEnabled = true;
                    set1.IsEnabled = true;
                    Text1.IsEnabled = true;
                }
                if (checkBox2.IsChecked == false)
                {
                    checkBox2_s.IsEnabled = false;
                    checkBox2_r.IsEnabled = false;
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
                Text2.IsEnabled = checkBox4.IsChecked == true ? true : false;
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
            NIDE8_C.Visibility = Node?.Value.AuthType == AuthenticationType.NIDE8 ? Visibility.Visible : Visibility.Collapsed;
            NIDE8_C.IsChecked = Node?.Value.AuthType == AuthenticationType.NIDE8 ? NIDE8_C.IsChecked : false;
        }

        private string Get_string(string a, string b, string c = null)
        {
            int x = a.IndexOf(b) + b.Length;
            int y;
            if (c != null)
            {
                y = a.IndexOf(c, x);
                if (y - x <= 0)
                    return a;
                else
                    return a.Substring(x, y - x);
            }
            else
                return a.Substring(x);
        }
        bool ischange = false;
        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (ischange || isload)
                return;
            ischange = true;
            if (ServerAddres.Text.Contains(":"))
            {
                var temp = ServerAddres.Text.Split(':');
                App.Config.MainConfig.Server.Address = ServerAddres.Text = temp[0];
                ServerPort.Text = temp[1];
                ushort.TryParse(temp[1], out ushort port);
                App.Config.MainConfig.Server.Port = port;
            }
            else
            {
                ServerPort.Text = "0";
                App.Config.MainConfig.Server.Port = 0;
            }
            ischange = false;
        }

        private void checkBox2_s_Check(object sender, RoutedEventArgs e)
        {
            if (checkBox2_s.IsChecked == true)
            {
                checkBox2_r.IsEnabled = true;
            }
            else if (checkBox2_s.IsChecked == false)
            {
                checkBox2_r.IsEnabled = false;
            }
        }
    }
}

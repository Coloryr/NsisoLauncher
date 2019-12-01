﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Windows;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NsisoLauncher.Controls
{
    /// <summary>
    /// AuthModuleControl.xaml 的交互逻辑
    /// </summary>
    public partial class AuthModuleControl : UserControl
    {
        private KeyValuePair<string, AuthenticationNode> authModule;

        private AuthenticationType authenticationType = AuthenticationType.OFFLINE;

        public AuthModuleControl()
        {
            InitializeComponent();
            saveButton.IsEnabled = false;
            delButton.IsEnabled = false;
        }

        public void SelectionChangedAccept(KeyValuePair<string, AuthenticationNode> node)
        {
            authModule = node;
            if (authModule.Value != null)
            {
                authmoduleNameTextbox.Text = authModule.Value.Name;
                REGISTER_URI.Text = authModule.Value.REG;
                Bro_IN.IsChecked = authModule.Value.use_in;
                switch (authModule.Value.AuthType)
                {
                    case AuthenticationType.NIDE8:
                        nide8Radio.IsChecked = true;
                        authDataTextbox.Text = authModule.Value.Property["nide8ID"];
                        Bro_IN.Visibility = REGISTER.Visibility = Visibility.Hidden;
                        break;
                    case AuthenticationType.AUTHLIB_INJECTOR:
                        aiRadio.IsChecked = true;
                        authDataTextbox.Text = authModule.Value.Property["authserver"];
                        Bro_IN.Visibility = REGISTER.Visibility = Visibility.Visible;
                        break;
                    case AuthenticationType.CUSTOM_SERVER:
                        customRadio.IsChecked = true;
                        authDataTextbox.Text = authModule.Value.Property["authserver"];
                        Bro_IN.Visibility = REGISTER.Visibility = Visibility.Visible;
                        break;
                    default:
                        return;
                }
                addButton.IsEnabled = false;
                saveButton.IsEnabled = true;
                delButton.IsEnabled = true;
                authmoduleNameTextbox.IsEnabled = false;
            }
        }

        private void Nide8_Checked(object sender, RoutedEventArgs e)
        {
            authmoduleLable.Content = "统一通行证ID：";
            authenticationType = AuthenticationType.NIDE8;
            Bro_IN.Visibility = REGISTER.Visibility = Visibility.Hidden;
        }

        private void AI_Checked(object sender, RoutedEventArgs e)
        {
            authmoduleLable.Content = "验证地址：";
            authenticationType = AuthenticationType.AUTHLIB_INJECTOR;
            Bro_IN.Visibility = REGISTER.Visibility = Visibility.Visible;
        }

        private void Custom_Checked(object sender, RoutedEventArgs e)
        {
            authmoduleLable.Content = "代理服务器地址：";
            authenticationType = AuthenticationType.CUSTOM_SERVER;
            Bro_IN.Visibility = REGISTER.Visibility = Visibility.Visible;
        }

        public void ClearAll()
        {
            authmoduleNameTextbox.IsEnabled = true;
            nide8Radio.IsChecked = false;
            aiRadio.IsChecked = false;
            customRadio.IsChecked = false;
            authmoduleNameTextbox.Text = string.Empty;
            authDataTextbox.Text = string.Empty;

            addButton.IsEnabled = true;
            saveButton.IsEnabled = false;
            delButton.IsEnabled = false;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIsEmpty())
            {
                string authName = authmoduleNameTextbox.Text;
                string authData = authDataTextbox.Text;
                AuthenticationNode node = new AuthenticationNode()
                {
                    AuthType = authenticationType,
                    Name = authName,
                    REG = REGISTER_URI.Text,
                    use_in = Bro_IN.IsChecked == true ? true : false
                };
                switch (authenticationType)
                {
                    case AuthenticationType.NIDE8:
                        node.Property.Add("nide8ID", authData);
                        break;
                    case AuthenticationType.AUTHLIB_INJECTOR:
                        node.Property.Add("authserver", authData);
                        break;
                    case AuthenticationType.CUSTOM_SERVER:
                        node.Property.Add("authserver", authData);
                        break;
                    default:
                        node.Property.Add("authserver", authData);
                        break;
                }
                ((SettingWindow)Window.GetWindow(this)).AddAuthModule(authName, node);
                SaveButton_Click(null, null);
                ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync("已添加", "已保存你添加的登录模型");
            }
        }

        private bool CheckIsEmpty()
        {
            if (authenticationType == AuthenticationType.OFFLINE)
            {
                ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync("未选择验证模型", "请选择您要使用的验证模型");
                return true;
            }
            if (string.IsNullOrWhiteSpace(authmoduleNameTextbox.Text))
            {
                ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync("未填写验证模型名称", "请填写您验证模型的名称");
                return true;
            }
            if (string.IsNullOrWhiteSpace(authDataTextbox.Text))
            {
                ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync("未填写模型数据", "请填写您验证模型的数据");
                return true;
            }
            return false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIsEmpty())
            {
                string authData = authDataTextbox.Text;
                authModule.Value.Property.Clear();
                authModule.Value.AuthType = authenticationType;
                authModule.Value.REG = REGISTER_URI.Text;
                authModule.Value.use_in = Bro_IN.IsChecked == true ? true : false;
                switch (authenticationType)
                {
                    case AuthenticationType.NIDE8:
                        authModule.Value.Property.Add("nide8ID", authData);
                        break;
                    case AuthenticationType.AUTHLIB_INJECTOR:
                        authModule.Value.Property.Add("authserver", authData);
                        break;
                    case AuthenticationType.CUSTOM_SERVER:
                        authModule.Value.Property.Add("authserver", authData);
                        break;
                    default:
                        authModule.Value.Property.Add("authserver", authData);
                        break;
                }
                ((SettingWindow)Window.GetWindow(this)).SaveAuthModule(authModule);
            }
        }

        private void DelButton_Click(object sender, RoutedEventArgs e)
        {
            if (authModule.Value.Name == "mojang")
            {
                ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync("删除失败", "无法删除正版登录");
                return;
            }
            ((SettingWindow)Window.GetWindow(this)).DeleteAuthModule(authModule);

        }
    }
}

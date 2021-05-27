using MahApps.Metro.Controls;
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
                addButton.Visibility = Visibility.Collapsed;
                saveButton.Visibility = delButton.Visibility = Visibility.Visible;
                authmoduleNameTextbox.Text = authModule.Value.Name;
                REGISTER_URI.Text = authModule.Value.RegisteAddress;
                SKIN_URI.Text = authModule.Value.SkinUrl;
                Bro_IN.IsChecked = authModule.Value.UseSelfBrowser;
                if (authModule.Value.HeadType == HeadType.URL)
                {
                    SKIN_T_U.IsChecked = true;
                }
                else
                {
                    SKIN_T_J.IsChecked = true;
                }
                switch (authModule.Value.AuthType)
                {
                    case AuthenticationType.NIDE8:
                        nide8Radio.IsChecked = true;
                        authDataTextbox.Text = authModule.Value.Property["nide8ID"];
                        SKIN.Visibility = Bro_IN.Visibility = REGISTER.Visibility =
                            SKIN_TYPE.Visibility = Visibility.Hidden;
                        break;
                    case AuthenticationType.AUTHLIB_INJECTOR:
                        aiRadio.IsChecked = true;
                        authDataTextbox.Text = authModule.Value.Property["authserver"];
                        SKIN.Visibility = Bro_IN.Visibility = REGISTER.Visibility =
                            SKIN_TYPE.Visibility = Visibility.Visible;
                        break;
                    case AuthenticationType.CUSTOM_SERVER:
                        customRadio.IsChecked = true;
                        authDataTextbox.Text = authModule.Value.Property["authserver"];
                        SKIN.Visibility = Bro_IN.Visibility = REGISTER.Visibility =
                            SKIN_TYPE.Visibility = Visibility.Visible;
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
            authmoduleLable.Content = App.GetResourceString("String.AuthModuleControl.Nide8");
            authenticationType = AuthenticationType.NIDE8;
            Bro_IN.Visibility = REGISTER.Visibility =
            SKIN.Visibility = SKIN_TYPE.Visibility = Visibility.Hidden;
        }

        private void AI_Checked(object sender, RoutedEventArgs e)
        {
            authmoduleLable.Content = App.GetResourceString("String.AuthModuleControl.Adress");
            authenticationType = AuthenticationType.AUTHLIB_INJECTOR;
            Bro_IN.Visibility = REGISTER.Visibility =
            SKIN.Visibility = SKIN_TYPE.Visibility = Visibility.Visible;
        }

        private void Custom_Checked(object sender, RoutedEventArgs e)
        {
            authmoduleLable.Content = App.GetResourceString("String.AuthModuleControl.Local");
            authenticationType = AuthenticationType.CUSTOM_SERVER;
            Bro_IN.Visibility = REGISTER.Visibility =
            SKIN.Visibility = SKIN_TYPE.Visibility = Visibility.Visible;
        }

        public void ClearAll()
        {
            addButton.Visibility = Visibility.Visible;
            saveButton.Visibility = delButton.Visibility = Visibility.Collapsed;

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
                    RegisteAddress = REGISTER_URI.Text,
                    UseSelfBrowser = Bro_IN.IsChecked == true ? true : false
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
            }
        }

        private bool CheckIsEmpty()
        {
            if (authenticationType == AuthenticationType.OFFLINE)
            {
                ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync(App.GetResourceString("String.AuthModuleControl.NoChose.Title"),
                    App.GetResourceString("String.AuthModuleControl.NoChose.Text"));
                return true;
            }
            if (string.IsNullOrWhiteSpace(authmoduleNameTextbox.Text))
            {
                ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync(App.GetResourceString("String.AuthModuleControl.NoName.Title"),
                    App.GetResourceString("String.AuthModuleControl.NoName.Text"));
                return true;
            }
            if (string.IsNullOrWhiteSpace(authDataTextbox.Text))
            {
                ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync(App.GetResourceString("String.AuthModuleControl.NoData.Title"),
                    App.GetResourceString("String.AuthModuleControl.NoData.Text"));
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
                authModule.Value.RegisteAddress = REGISTER_URI.Text;
                authModule.Value.SkinUrl = SKIN_URI.Text;
                authModule.Value.UseSelfBrowser = Bro_IN.IsChecked == true ? true : false;
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
            if (authModule.Value.Name == "mojang" || authModule.Value.Name == "offline")
            {
                ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync(App.GetResourceString("String.AuthModuleControl.Error.Title"),
                    App.GetResourceString("String.AuthModuleControl.Error.Text"));
                return;
            }
            ((SettingWindow)Window.GetWindow(this)).DeleteAuthModule(authModule);

        }

        private void SKIN_T_U_Checked(object sender, RoutedEventArgs e)
        {
            authModule.Value.HeadType = HeadType.URL;
            SKIN_T_J.IsChecked = false;
        }

        private void SKIN_T_J_Checked(object sender, RoutedEventArgs e)
        {
            authModule.Value.HeadType = HeadType.JSON;
            SKIN_T_J.IsChecked = true;
        }
    }
}

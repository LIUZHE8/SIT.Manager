using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SIT.Manager.Classes;
using SIT.Manager.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayPage : Page
    {
        public PlayPage()
        {
            this.InitializeComponent();

            DataContext = App.ManagerConfig;

            ConnectionInfo_TextChanged(null, null);
        }
        string AddressBoxData;
        string AddressBoxDefault = "[ censored, click to reveal ]";

        private void ConnectionInfo_TextChanged(object sender, object args)
        {
            bool missingInfo = AddressBox.Text.Length == 0 || UsernameBox.Text.Length == 0 || PasswordBox.Password.Length == 0 || string.IsNullOrEmpty(App.ManagerConfig.InstallPath);
            ToolTipService.SetToolTip(ConnectButton, new ToolTip()
            {
                Content = missingInfo ? "Fill in all the fields first." : $"Attempt to connect to {AddressBox.Text} and launch the game."
            });
            if (AddressBox.Text.Length == 0 || UsernameBox.Text.Length == 0 || PasswordBox.Password.Length == 0 || string.IsNullOrEmpty(App.ManagerConfig.InstallPath))
            {
                ToolTipService.SetToolTip(ConnectButton, new ToolTip()
                {
                    Content = $"���Ƚ���������д������"
                });
                ConnectButton.IsEnabled = false;
            }
            else if (AddressBox.Text.Length > 0)
            {
                ToolTipService.SetToolTip(ConnectButton, new ToolTip()
                {
                    Content = $"�������ӵ� {AddressBox.Text} ��������Ϸ��"
                });
                ConnectButton.IsEnabled = true;
            }
        }

            if (AddressBox.Text.Length > 9 && AddressBox.Text != AddressBoxDefault)
            {
                AddressBoxData = AddressBox.Text;
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (AddressBox.Text.Length == 0 || UsernameBox.Text.Length == 0 || PasswordBox.Password.Length == 0 || string.IsNullOrEmpty(App.ManagerConfig.InstallPath))
            {
                ToolTipService.SetToolTip(ConnectButton, new ToolTip()
                {
                    Content = $"���Ƚ���������д������"
                });
                ConnectButton.IsEnabled = false;
            }
        }
            else if (AddressBox.Text.Length > 0)
            {
                ToolTipService.SetToolTip(ConnectButton, new ToolTip()
                {
                    Content = $"�������ӵ� {AddressBox.Text} ��������Ϸ��"
                });
                ConnectButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Connect to a server
        /// </summary>
        private async Task<string> Connect()
        {
            ManagerConfig.Save((bool)RememberMeCheck.IsChecked);

            if (App.ManagerConfig.InstallPath == null)
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "���ô���",
                    Content = "\"��װ·��\" δ���á�ת�� ���� ���ÿͻ��˰�װ·����",
                    CloseButtonText = "��"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return "error";
            }

            if (!File.Exists(App.ManagerConfig.InstallPath + @"\BepInEx\plugins\StayInTarkov.dll"))
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "��װ����",
                    Content = "�޷��ҵ� \"StayInTarkov.dll\"�����ӷ�����ǰ���Ȱ�װ SIT��",
                    CloseButtonText = "��"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return "error";
            }

            if (!File.Exists(App.ManagerConfig.InstallPath + @"\EscapeFromTarkov.exe"))
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "��װ����",
                    Content = "�޷�����Ϸ��װĿ¼���ҵ� \"EscapeFromTarkov.exe\"��",
                    CloseButtonText = "��"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return "error";
            }

            if (AddressBox.Text.Length == 0 || UsernameBox.Text.Length == 0 || PasswordBox.Password.Length == 0)
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "ƾ�ݴ���",
                    Content = "��������ַ���û���������δ��д��",
                    CloseButtonText = "��"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return "error";
            }

            if (!AddressBox.Text.Contains(@"http://"))
            {
                UriBuilder builder = new(AddressBoxData);
                builder.Port = builder.Port == 80 ? 6969 : builder.Port;
                AddressBoxData = builder.Uri.ToString().TrimEnd(new[] { '/', '\\' });
            }
            catch(UriFormatException)
            {
                await new ContentDialog()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "Input Error",
                    Content = "Invalid address.",
                    CloseButtonText = "Ok"
                }.ShowAsync(ContentDialogPlacement.InPlace);

            if (!AddressBox.Text.Match(@":\d{2,5}$"))
            {
                AddressBox.Text = AddressBox.Text + @":6969";
            }

            string returnData = await LoginToServer();
            return returnData;
        }

        /// <summary>
        /// Login to a server
        /// </summary>
        /// <returns>string</returns>
        private async Task<string> LoginToServer()
        {
            TarkovRequesting requesting = new TarkovRequesting(null, AddressBoxData, false);

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "username", UsernameBox.Text },
                { "email", UsernameBox.Text },
                { "edition", "Edge Of Darkness" },
                { "password", PasswordBox.Password },
                { "backendUrl", AddressBoxData }
            };

            try
            {
                var returnData = requesting.PostJson("/launcher/profile/login", JsonSerializer.Serialize(data));

                // If failed, attempt to register
                if (returnData == "FAILED")
                {
                    string jsonData = requesting.PostJson("/launcher/server/connect", JsonSerializer.Serialize(new object())).ToString();
                    Newtonsoft.Json.Linq.JObject connectData = Newtonsoft.Json.Linq.JObject.Parse(jsonData);
                    AkiServer.Editions = connectData["editions"].Values<string>().ToArray();
                    ContentDialog createAccountDialog = new()
                    {
                        XamlRoot = Content.XamlRoot,
                        Title = "�˻�������",
                        Content = "���������Ҳ���ָ���˻����Ƿ�ʹ�õ�ǰƾ��ע���˻���",
                        IsPrimaryButtonEnabled = true,
                        PrimaryButtonText = "��",
                        CloseButtonText = "��"
                    };

                    ContentDialogResult msgBoxResult = await createAccountDialog.ShowAsync(ContentDialogPlacement.InPlace);
                    if (msgBoxResult == ContentDialogResult.Primary)
                    {
                        SelectEditionDialog selectWindow = new()
                        {
                            XamlRoot = Content.XamlRoot
                        };
                        await selectWindow.ShowAsync();
                        string edition = selectWindow.edition;

                        if (edition != null)
                            data["edition"] = edition;
                        // Register
                        returnData = requesting.PostJson("/launcher/profile/register", JsonSerializer.Serialize(data));
                        // Login attempt after register
                        returnData = requesting.PostJson("/launcher/profile/login", JsonSerializer.Serialize(data));

                    }
                    else
                    {
                        return null;
                    }
                }
                else if(returnData == "INVALID_PASSWORD")
                {
                    Utils.ShowInfoBar("����", $"�������!", InfoBarSeverity.Error);
                    return "error";
                }

                return returnData;
            }
            catch (System.Net.WebException webEx)
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "��¼����",
                    Content = $"�޷������������ͨ��\n{webEx.Message}",
                    CloseButtonText = "��"
                };

                Loggy.LogToFile("Login Error: " + webEx);

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return "error";
            }
            catch (Exception ex)
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "��¼����",
                    Content = $"�޷������������ͨ��\n{ex.Message}",
                    CloseButtonText = "��"
                };

                Loggy.LogToFile("Login Error: " + ex);

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return "error";
            }
        }


        /// <summary>
        /// Handling Connect button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string returnData = await Connect();

            if (returnData == "error")
            {
                return;
            }
            if (string.IsNullOrEmpty(returnData))
            {
                return;
            }

            Utils.ShowInfoBar("Connect", $"Successfully connected to {AddressBox.Text}", InfoBarSeverity.Success);

            string arguments = $"-token={returnData} -config={{\"BackendUrl\":\"{AddressBoxData}\",\"Version\":\"live\"}}";
            Process.Start(App.ManagerConfig.InstallPath + @"\EscapeFromTarkov.exe", arguments);

            if (App.ManagerConfig.CloseAfterLaunch)
            {
                Application.Current.Exit();
            }
        }

        private void AddressBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox t = sender as TextBox;
            t.Text = AddressBoxDefault;
        }

        private void AddressBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t.Text.Equals(AddressBoxDefault))
            {
                t.Text = AddressBoxData;
            }
        }
    }
}

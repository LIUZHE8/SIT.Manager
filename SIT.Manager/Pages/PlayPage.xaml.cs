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

            if (AddressBox.Text.Length == 0 || UsernameBox.Text.Length == 0 || PasswordBox.Password.Length == 0)
            {
                ConnectButton.IsEnabled = false;
            }
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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

            if (!(AddressBox.Text.Contains(@"http://") || AddressBox.Text.Contains(@"https://")))
            {
                AddressBox.Text = @"http://" + AddressBox.Text;
            }

            if (AddressBox.Text.EndsWith(@"/") || AddressBox.Text.EndsWith(@"\"))
            {
                AddressBox.Text = AddressBox.Text.Remove(AddressBox.Text.Length - 1, 1);
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
            TarkovRequesting requesting = new TarkovRequesting(null, AddressBox.Text, false);

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "username", UsernameBox.Text },
                { "email", UsernameBox.Text },
                { "edition", "Edge Of Darkness" },
                { "password", PasswordBox.Password },
                { "backendUrl", AddressBox.Text }
            };

            try
            {
                var returnData = requesting.PostJson("/launcher/profile/login", JsonSerializer.Serialize(data));

                // If failed, attempt to register
                if (returnData == "FAILED")
                {
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

            Utils.ShowInfoBar("����", $"�ѳɹ������� {AddressBox.Text}", InfoBarSeverity.Success);

            string arguments = $"-token={returnData} -config={{\"BackendUrl\":\"{AddressBox.Text}\",\"Version\":\"live\"}}";
            Process.Start(App.ManagerConfig.InstallPath + @"\EscapeFromTarkov.exe", arguments);

            if (App.ManagerConfig.CloseAfterLaunch)
            {
                Application.Current.Exit();
            }
        }
    }
}

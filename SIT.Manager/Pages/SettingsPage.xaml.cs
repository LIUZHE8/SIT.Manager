using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SIT.Manager.Classes;
using SIT.Manager.Controls;
using System;
using System.IO;
using System.Reflection;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            DataContext = App.ManagerConfig;

            VersionHyperlinkButton.Content = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        }

        private async void ChangeInstallButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new()
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            folderPicker.FileTypeFilter.Add("*");

            MainWindow window = App.m_window as MainWindow;
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder eftFolder = await folderPicker.PickSingleFolderAsync();
            if (eftFolder != null && File.Exists(eftFolder.Path + @"\EscapeFromTarkov.exe"))
            {
                App.ManagerConfig.InstallPath = eftFolder.Path;

                Utils.CheckEFTVersion(eftFolder.Path);
                Utils.CheckSITVersion(eftFolder.Path);

                ManagerConfig.Save();
                Utils.ShowInfoBar("配置", $"逃离塔科夫游戏路径已设定为 \"{eftFolder.Path}\"");
            }
            else
            {
                Utils.ShowInfoBar("错误", "选定的路径无效。请确保选定路径为正确的 逃离塔科夫 主游戏安装目录。", InfoBarSeverity.Error);
                return;
            }
        }

        private async void ChangeAkiServerPath_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new()
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            folderPicker.FileTypeFilter.Add("*");

            MainWindow window = App.m_window as MainWindow;
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder akiServerPath = await folderPicker.PickSingleFolderAsync();
            if (akiServerPath != null && File.Exists(akiServerPath.Path + @"\Aki.Server.exe"))
            {
                App.ManagerConfig.AkiServerPath = akiServerPath.Path;

                ManagerConfig.Save();
                Utils.ShowInfoBar("配置", $"SPT-AKI 服务端路径已设定为 \"{akiServerPath.Path}\"");
            }
            else
            {
                Utils.ShowInfoBar("错误", "选定的路径无效。请确保选定路径为正确的 SPT-AKI 服务端安装目录。");
            }
        }

        private async void ColorChangeButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerDialog colorPickerWindow = new()
            {
                XamlRoot = Content.XamlRoot
            };

            await colorPickerWindow.ShowAsync();

            string pickedColor = colorPickerWindow.SelectedColor;

            if (pickedColor != null)
            {
                App.ManagerConfig.ConsoleFontColor = pickedColor;
                ManagerConfig.Save();
            }
        }

        private async void ConsoleFamilyFontChange_Click(object sender, RoutedEventArgs e)
        {
            FontFamilyPickerDialog fontFamilyPickerDialog = new()
            {
                XamlRoot = Content.XamlRoot
            };

            await fontFamilyPickerDialog.ShowAsync();

            string pickedFontFamily = fontFamilyPickerDialog.selectedFontFamily;

            if (pickedFontFamily != null)
            {
                App.ManagerConfig.ConsoleFontFamily = pickedFontFamily;
                ManagerConfig.Save();
            }
        }

        private void VersionHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new()
            {
                RequestedOperation = DataPackageOperation.Copy,
            };

            dataPackage.SetText(VersionHyperlinkButton.Content.ToString());
            Clipboard.SetContent(dataPackage);
        }
    }
}

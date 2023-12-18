using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SIT.Manager.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModsPage : Page
    {
        public ModsPage()
        {
            this.InitializeComponent();

            if (App.ManagerConfig.AcceptedModsDisclaimer == true)
            {
                DisclaimerGrid.Visibility = Visibility.Collapsed;
                ModGrid.Visibility = Visibility.Visible;
            }

            LoadMasterList();
        }

        private void LoadMasterList()
        {
            if (string.IsNullOrEmpty(App.ManagerConfig.InstallPath))
            {
                Utils.ShowInfoBar("����", "\"��װ·��\" δ���á�ת�� ���� �����á�", InfoBarSeverity.Error);
                return;
            }

            string dir = App.ManagerConfig.InstallPath + @"\SITLauncher\Mods\Extracted\";
            List<ModInfo> outdatedMods = new();

            if (!File.Exists(dir + @"MasterList.json"))
            {
                ModsList.Items.Add(new ModInfo()
                {
                    Name = "δ�ҵ�ģ��"
                });
                ModsList.IsHitTestVisible = false;
                return;
            }

            if (ModsList.IsHitTestVisible == false)
                ModsList.IsHitTestVisible = true;

            List<ModInfo> masterList = JsonSerializer.Deserialize<List<ModInfo>>(File.ReadAllText(dir + @"MasterList.json"));
            masterList = masterList.OrderBy(x => x.Name).ToList();

            ModsList.ItemsSource = masterList;
            if (ModsList.Items.Count > 0)
            {
                ModsList.SelectedIndex = 0;

                if (InfoGrid.Visibility == Visibility.Collapsed)
                    InfoGrid.Visibility = Visibility.Visible;
            }

            foreach (ModInfo mod in masterList)
            {
                var keyValuePair = App.ManagerConfig.InstalledMods.Where(x => x.Key == mod.Name).FirstOrDefault();
                if (!keyValuePair.Equals(default(KeyValuePair<string, string>)))
                {
                    Version installedVersion = new(keyValuePair.Value);
                    Version currentVersion = new(mod.PortVersion);

                    int result = installedVersion.CompareTo(currentVersion);
                    if (result < 0)
                    {
                        outdatedMods.Add(mod);
                    }
                }

            }

            if (outdatedMods.Count > 0)
            {
                AutoUpdate(outdatedMods);
            }
        }

        /// <summary>
        /// Automatically updates installed mods that are outdated.
        /// </summary>
        /// <param name="outdatedMods"><see cref="List{T}"/> of <see cref="ModInfo"/> that are outdated.</param>
        private async void AutoUpdate(List<ModInfo> outdatedMods)
        {
            // As this is being run on another thread than the UI we need to fetch the MainWindow
            MainWindow window = App.m_window as MainWindow;

            List<string> outdatedNames = new();
            outdatedNames.AddRange(from ModInfo mod in outdatedMods
                                   select mod.Name);

            string outdatedString = string.Join("\n", outdatedNames);

            ScrollView scrollView = new()
            {
                Content = new TextBlock()
                {
                    Text = $"���� {outdatedMods.Count} ���ɰ汾ģ�顣��Ҫ����������\n�ɰ汾ģ��:\n\n{outdatedString}"
                }
            };

            ContentDialog contentDialog = new()
            {
                XamlRoot = window.Content.XamlRoot,
                Title = "�ҵ�һЩģ�����",
                Content = scrollView,
                CloseButtonText = "��",
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "��"
            };

            ContentDialogResult result = await contentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                foreach (ModInfo mod in outdatedMods)
                {
                    App.ManagerConfig.InstalledMods.Remove(mod.Name);
                    InstallMod(mod, true);
                }
            }
            else
            {
                return;
            }

            Utils.ShowInfoBar("ģ�����", $"�Ѹ��� {outdatedMods.Count} ��ģ�顣", InfoBarSeverity.Success);
        }

        private async void DownloadModPack()
        {
            if (string.IsNullOrEmpty(App.ManagerConfig.InstallPath))
            {
                Utils.ShowInfoBar("����", "\"��װ·��\" δ���á�ת�� ���� ���ÿͻ��˰�װ·����", InfoBarSeverity.Error);
                return;
            }

            try
            {
                DownloadModPackageButton.IsEnabled = false;
                Loggy.LogToFile("DownloadModPack: Starting download of mod package.");

                string dir = App.ManagerConfig.InstallPath + @"\SITLauncher\Mods";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string[] subDirs = Directory.GetDirectories(dir);
                foreach (string subDir in subDirs)
                {
                    Directory.Delete(subDir, true);
                }

                Directory.CreateDirectory(dir + @"\Extracted");

                await Utils.DownloadFile("SIT.Mod.Ports.Collection.zip", dir, "https://github.tarkov.free.hr/stayintarkov/SIT-Mod-Port-Mirror/releases/latest/download/SIT.Mod.Ports.Collection.zip", true);
                Utils.ExtractArchive(dir + @"\SIT.Mod.Ports.Collection.zip", dir + @"\Extracted");
                DownloadModPackageButton.IsEnabled = true;

                LoadMasterList();
            }
            catch (Exception ex)
            {
                Loggy.LogToFile("DownloadModPack:\n" + ex.Message);
                DownloadModPackageButton.IsEnabled = true;
                return;
            }
        }

        private async void InstallMod(ModInfo mod, bool suppressNotification = false)
        {
            if (string.IsNullOrEmpty(App.ManagerConfig.InstallPath))
            {
                Utils.ShowInfoBar("ģ�鰲װ", "\"��װ·��\" δ���á�ת�� ���� ���ÿͻ��˰�װ·����", InfoBarSeverity.Error);
                return;
            }

            try
            {
                if (mod.SupportedVersion != App.ManagerConfig.SitVersion)
                {
                    ContentDialog contentDialog = new ContentDialog()
                    {
                        XamlRoot = XamlRoot,
                        Title = "����",
                        Content = $"�㳢�԰�װ��ģ���뵱ǰ�Ѱ�װ SIT �汾�����ݡ�\n\n���ݵ� SIT �汾: {mod.SupportedVersion}\n��ǰ�Ѱ�װ SIT �汾: {(string.IsNullOrEmpty(App.ManagerConfig.SitVersion) ? "δ֪" : App.ManagerConfig.SitVersion)}\n\n��Ҫ������",
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        IsPrimaryButtonEnabled = true,
                        PrimaryButtonText = "��",
                        CloseButtonText = "��"
                    };

                    ContentDialogResult response = await contentDialog.ShowAsync();

                    if (response != ContentDialogResult.Primary)
                        return;
                }

                InstallButton.IsEnabled = false;
                if (mod == null || string.IsNullOrEmpty(App.ManagerConfig.InstallPath))
                    return;

                string installPath = App.ManagerConfig.InstallPath;
                string gamePluginsPath = installPath + @"\BepInEx\plugins\";
                string gameConfigPath = installPath + @"\BepInEx\config\";

                foreach (string pluginFile in mod.PluginFiles)
                {
                    File.Copy(installPath + @"\SITLauncher\Mods\Extracted\plugins\" + pluginFile, gamePluginsPath + pluginFile, true);
                }

                foreach (var configFile in mod.ConfigFiles)
                {
                    File.Copy(installPath + @"\SITLauncher\Mods\Extracted\config\" + configFile, gameConfigPath + configFile, true);
                }

                App.ManagerConfig.InstalledMods.Add(mod.Name, mod.PortVersion);
                ManagerConfig.Save();

                if (suppressNotification == false)
                    Utils.ShowInfoBar("ģ�鰲װ", $"{mod.Name} �Ѱ�װ�ɹ���", InfoBarSeverity.Success);
                UninstallButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Loggy.LogToFile("InstallMod: " + ex.Message);
                InstallButton.IsEnabled = true;
                Utils.ShowInfoBar("��װģ��", $"{mod.Name} ��װʧ�ܡ���������������־��(Launcher.log)", InfoBarSeverity.Error);
                return;
            }
        }

        private async void UninstallMod(ModInfo mod)
        {
            try
            {
                UninstallButton.IsEnabled = false;

                if (mod == null || string.IsNullOrEmpty(App.ManagerConfig.InstallPath))
                    return;

                string installPath = App.ManagerConfig.InstallPath;
                string gamePluginsPath = installPath + @"\BepInEx\plugins\";
                string gameConfigPath = installPath + @"\BepInEx\config\";

                foreach (string pluginFile in mod.PluginFiles)
                {
                    if (File.Exists(gamePluginsPath + pluginFile))
                    {
                        File.Delete(gamePluginsPath + pluginFile);
                    }
                    else
                    {
                        ContentDialog dialog = new()
                        {
                            XamlRoot = XamlRoot,
                            Title = "ж��ģ��ʱ����",
                            Content = $"�㱨�� {mod.Name} ��һ���ļ�ȱʧ: \"{pluginFile}\"\n\n�Ƿ�ǿ�ƽ���ģ���Ƴ��Ѱ�װģ���б�",
                            CloseButtonText = "��",
                            IsPrimaryButtonEnabled = true,
                            PrimaryButtonText = "��"
                        };

                        ContentDialogResult result = await dialog.ShowAsync();

                        if (result != ContentDialogResult.Primary)
                        {
                            throw new FileNotFoundException($"A file was missing from the mod {mod.Name}: '{pluginFile}'");
                        }
                    }
                }

                foreach (var configFile in mod.ConfigFiles)
                {
                    if (File.Exists(gameConfigPath + configFile))
                    {
                        File.Delete(gameConfigPath + configFile);
                    }
                    else
                    {
                        ContentDialog dialog = new()
                        {
                            XamlRoot = XamlRoot,
                            Title = "ж��ģ��ʱ����",
                            Content = $"�㱨�� {mod.Name} ��һ���ļ�ȱʧ: \"{configFile}\"\n\n�Ƿ�ǿ�ƽ���ģ���Ƴ��Ѱ�װģ���б�",
                            CloseButtonText = "No",
                            IsPrimaryButtonEnabled = true,
                            PrimaryButtonText = "Yes"
                        };

                        ContentDialogResult result = await dialog.ShowAsync();

                        if (result != ContentDialogResult.Primary)
                        {
                            throw new FileNotFoundException($"A file was missing from the mod {mod.Name}: '{configFile}'");
                        }
                    }
                }

                App.ManagerConfig.InstalledMods.Remove(mod.Name);
                ManagerConfig.Save();

                Utils.ShowInfoBar("ģ��ж��", $"{mod.Name} �ѳɹ�ж�ء�", InfoBarSeverity.Success);
                InstallButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Loggy.LogToFile("UninstallMod: " + ex.Message);
                UninstallButton.IsEnabled = true;
                Utils.ShowInfoBar("ģ��ж��", $"{mod.Name} ж��ʧ�ܡ���������������־��(Launcher.log)", InfoBarSeverity.Error);
                return;
            }
        }

        #region Buttons & Events
        private void DownloadModPackageButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadModPack();
        }
        private void ModsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModInfo selectedMod = (ModInfo)ModsList.SelectedItem;

            if (selectedMod == null)
                return;

            bool isInstalled = App.ManagerConfig.InstalledMods.Keys.Contains(selectedMod.Name);

            InstallButton.IsEnabled = !isInstalled;
            UninstallButton.IsEnabled = isInstalled;
        }
        private void IUnderstandButton_Click(object sender, RoutedEventArgs e)
        {
            DisclaimerGrid.Visibility = Visibility.Collapsed;
            ModGrid.Visibility = Visibility.Visible;

            App.ManagerConfig.AcceptedModsDisclaimer = true;
            ManagerConfig.Save();
        }
        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModsList.SelectedIndex == -1 || ModsList.SelectedItem == null)
                return;

            InstallMod((ModInfo)ModsList.SelectedItem);
        }
        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModsList.SelectedIndex == -1 || ModsList.SelectedItem == null)
                return;

            UninstallMod((ModInfo)ModsList.SelectedItem);
        }
        #endregion
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using SIT.Manager.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Pages
{
    /// <summary>
    /// ServerPage for handling SPT-AKI Server execution and console output.
    /// </summary>
    public sealed partial class ServerPage : Page
    {
        MainWindow? window = App.m_window as MainWindow;

        public ServerPage()
        {
            this.InitializeComponent();

            DataContext = App.ManagerConfig;

            AkiServer.OutputDataReceived += AkiServer_OutputDataReceived;
            AkiServer.RunningStateChanged += AkiServer_RunningStateChanged;
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (AkiServer.State == AkiServer.RunningState.NOT_RUNNING)
            {
                if (AkiServer.IsUnhandledInstanceRunning())
                {
                    AddConsole("���� SPT-AKI ������������С���ر��κ��������е� SPT-AKI �����ʵ����");

                    return;
                }

                if (!File.Exists(AkiServer.FilePath))
                {
                    AddConsole("δ�ҵ� SPT-AKI ����ˡ�����������ǰ��ת�� ���� ���� SPT-AKI �����·����");
                    return;
                }

                AddConsole("������������...");

                try
                {
                    AkiServer.Start();
                }
                catch (Exception ex)
                {
                    AddConsole(ex.Message);
                }
            }
            else
            {
                AddConsole("���ڹرշ�����...");

                try
                {
                    AkiServer.Stop();
                }
                catch (Exception ex)
                {
                    AddConsole(ex.Message);
                }
            }
        }

        private void AddConsole(string text)
        {
            if (text == null)
                return;

            Paragraph paragraph = new();
            Run run = new();

            //[32m, [2J, [0;0f,
            run.Text = Regex.Replace(text, @"(\[\d{1,2}m)|(\[\d{1}[a-zA-Z])|(\[\d{1};\d{1}[a-zA-Z])", "");

            paragraph.Inlines.Add(run);
            ConsoleLog.Blocks.Add(paragraph);
        }

        private void AkiServer_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            window.DispatcherQueue.TryEnqueue(() => AddConsole(e.Data));
        }

        private void AkiServer_RunningStateChanged(AkiServer.RunningState runningState)
        {
            window.DispatcherQueue.TryEnqueue(() =>
            {
                switch (runningState)
                {
                    case AkiServer.RunningState.RUNNING:
                        {
                            AddConsole("������������!");
                            StartServerButtonSymbolIcon.Symbol = Symbol.Stop;
                            StartServerButtonTextBlock.Text = "�رշ�����";
                        }
                        break;
                    case AkiServer.RunningState.NOT_RUNNING:
                        {
                            AddConsole("�������ѹر�!");
                            StartServerButtonSymbolIcon.Symbol = Symbol.Play;
                            StartServerButtonTextBlock.Text = "����������";
                        }
                        break;
                    case AkiServer.RunningState.STOPPED_UNEXPECTEDLY:
                        {
                            AddConsole("����������ر�! �������̨�еĴ��󱨸档");
                            StartServerButtonSymbolIcon.Symbol = Symbol.Play;
                            StartServerButtonTextBlock.Text = "����������";
                        }
                        break;
                }
            });
        }

        private void ConsoleLog_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConsoleLogScroller.ScrollToVerticalOffset(ConsoleLogScroller.ScrollableHeight);
        }
    }
}

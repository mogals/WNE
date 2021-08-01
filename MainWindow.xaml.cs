using ImapIdle;
using MailKit.Security;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WNE.Parsing;

namespace WNE
{
    public partial class MainWindow : Window
    {
        public string saveFolder { get; set; }

        public Setting setting { get; set; }

        public IdleClient idleClient;
        public MainWindow()
        {
            InitializeComponent();
            setting = YamlFileController.Instance.DeSerialize<Setting>("설정.yml");
            richTextBox.AppendText($"\n테스트 메일용 약속 문자 : {setting.테스트예약표기문자}");
            if (setting.거래내역서기능사용여부)
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    richTextBox.AppendText($"\n[{dialog.FileName}]에 엑셀파일을 저장합니다.");
                    saveFolder = dialog.FileName;
                }
            }

        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            leftPanel.Visibility = Visibility.Collapsed;
            setting.엑셀저장폴더 = saveFolder;
            var items = setting.GetType().GetProperties();
            idleClient = new IdleClient(setting, richTextBox);
            Task task = idleClient.RunAsync();
        }

        private void richTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            richTextBox.ScrollToEnd();
        }
    }
}
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

namespace WNE
{
    public partial class MainWindow : Window
    {
        public string saveFolder { get; set; }
        public Info info { get; set; }

        public IdleClient idleClient;
        public MainWindow()
        {
            InitializeComponent();
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                richTextBox.AppendText($"\n[{dialog.FileName}] 에 엑셀파일을 저장합니다.");
                saveFolder = dialog.FileName;
            }
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            //leftPanel.Visibility = Visibility.Collapsed;
            int result;
            int.TryParse(beforeTime.Text, out result);
            Info info = new Info()
            {
                beforeTime = result,
                saveFolder = saveFolder,
                SslOptions = SecureSocketOptions.Auto,
                imapMailPort = 993,
                mailHost = "imap.gmail.com",
                mailUsername = gmail.Text,
                mailPassword = password.Password,
                naverUsername = "moire478_bot",
                naverPassword = "moire1779!",
                ddnayoUsername = "hs빌",
                ddnayoPassword = "moire8824!",
                ddnayoAuthLogin = "false",
                ddnayoAccomodationId = 6850,
                ddnayoMaxStayDays = 15,
                loginRequestUrl = "https://partner.ddnayo.com/security/login",
                registerRequestUrl = "https://partner.ddnayo.com/pms-api/reservation/ready",
                cancelRequestUrl = string.Empty,
                deleteRequestUrl = string.Empty,
                retrieveRequestUrl = $"https://partner.ddnayo.com/pms-api/accommodation/{6850}/reservation/management-list",
            };
            idleClient = new IdleClient(info, richTextBox);
            idleClient.RunAsync();

        }

        //bool leftPanelVisible = true;

        private void richTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            richTextBox.ScrollToEnd();
        }
    }
}
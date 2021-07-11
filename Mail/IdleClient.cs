using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using OpenQA.Selenium.Chrome;
using System.Net;
using System.Linq;
using Report;
using WNE.Parsing;
using WNE;
using System.Windows.Controls;
using System.Windows.Media;
using WNE.Ddnayo;
using OpenQA.Selenium;
using MailKit.Search;

namespace ImapIdle
{
    public class IdleClient : IDisposable
    {
        public DdnayoClient ddnayoClient;

        public Info _info = new Info();
        List<IMessageSummary> messages;
        CancellationTokenSource cancel;
        CancellationTokenSource done;
        bool messagesArrived;
        ImapClient client;
        Excel excel = new Excel();

        public ChromeDriverService chromeDriverService;
        public ChromeOptions chromeOptions;
        public ChromeDriver chromeDriver;
        public NetworkCredential credentials;
        public ProtocolLogger logger;
        public RichTextBox _richTextBox;
        public MediaPlayer mediaPlayer;

        public IdleClient(Info info, RichTextBox richTextBox)
        {
            _info = info;
            _richTextBox = richTextBox;
            client = new ImapClient();
            messages = new List<IMessageSummary>();
            cancel = new CancellationTokenSource();
            mediaPlayer = new MediaPlayer();

            ddnayoClient = new DdnayoClient();
        }

        public async Task RunAsync()
        {
            ConnectPartnerCenter();
            try
            {
                ReconnectAsync();
                await ProcessMessagesAsync();
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e);
                await client.DisconnectAsync(true);
                return;
            }
            var inbox = client.Inbox;
            inbox.CountChanged += OnCountChanged;
            inbox.MessageExpunged += OnMessageExpunged;
            inbox.MessageFlagsChanged += OnMessageFlagsChanged;
            await IdleAsync();
            inbox.MessageFlagsChanged -= OnMessageFlagsChanged;
            inbox.MessageExpunged -= OnMessageExpunged;
            inbox.CountChanged -= OnCountChanged;
            await client.DisconnectAsync(true);
        }
        public void ReconnectAsync()
        {
            if (!client.IsConnected)
            {
                client.Connect(_info.mailHost, _info.imapMailPort, _info.SslOptions, cancel.Token);
            }


            if (!client.IsAuthenticated)
            {
                credentials = new NetworkCredential(_info.mailUsername, _info.mailPassword);
                client.Authenticate(credentials, cancel.Token);
                client.Inbox.Open(FolderAccess.ReadWrite, cancel.Token);
                _richTextBox.AppendText("\n");
                var fetchedMessages = client.Inbox.Search(SearchQuery.Not(SearchQuery.HasGMailLabel(@"\Starred")));
                _richTextBox.AppendText(fetchedMessages.Count.ToString());
                client.Inbox.SetLabels(new UniqueId(fetchedMessages.Last().Id), new String[] { @"\Starred" }, true);

                _richTextBox.AppendText("메일 접속함");
            }
        }

        async Task ProcessMessagesAsync()
        {
            IList<IMessageSummary> fetched;
            do
            {
                try
                {
                    int startIndex = messages.Count;
                    fetched = client.Inbox.Fetch(startIndex, -1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId, cancel.Token);
                    
                    break;
                }
                catch (ImapProtocolException e)
                {
                    Console.WriteLine(e);
                    ReconnectAsync();
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                    ReconnectAsync();
                }
            } while (true);

            foreach (var message in fetched)
            {
                try
                {
                    if (true)
                    {
                        _richTextBox.AppendText("\n");
                        _richTextBox.AppendText($"{message.Date} : 메일 도착");

                        // 메일 분석
                        MimeMessage mimeMessage = client.Inbox.GetMessage(message.Index);
                        var reservationFromMail = new Reservation(mimeMessage);

                        // 예약파트너센터 분석
                        string reservationInfoStringFromPartnerCenter;
                        Reservation reservationFromPartnerCenter = new Reservation();
                        if (reservationFromMail.예약상태 is not ReservationState.예약메일아님)
                        {
                            reservationInfoStringFromPartnerCenter = GetReservationInfoStringFromPartnerCenter(reservationFromMail.예약번호네이버);
                            reservationFromPartnerCenter = new Reservation(reservationInfoStringFromPartnerCenter);
                            GetPastReservation(reservationFromPartnerCenter);
                        }

                        switch (reservationFromMail.예약상태)
                        {
                            case ReservationState.확정:
                                _richTextBox.AppendText("\n");
                                _richTextBox.AppendText($"{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 예약 메일 도착");

                                await ddnayoClient.Ready(reservationFromMail, reservationFromPartnerCenter);

                                excel.Save(reservationFromMail, reservationFromPartnerCenter, _info);
                                
                                Console.WriteLine();
                                Console.WriteLine("아래 예약 메일 => 처리 완료(실제 등록됐는지는 위의 로그 참고)");
                                Console.WriteLine($"{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명}");

                                _richTextBox.AppendText("\n");
                                _richTextBox.AppendText($"{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 예약 메일 처리 완료");

                                mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "예약이등록됐습니다.mp3")));
                                mediaPlayer.Play();
                                break;

                            case ReservationState.소액테스트:
                                Console.WriteLine();
                                Console.WriteLine("소액 테스트 메일 => 무시");
                                Console.WriteLine($"{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명}");

                                _richTextBox.AppendText("\n");
                                _richTextBox.AppendText($"{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 소액 테스트 메일 => 무시");

                                mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "예약을무시합니다.mp3")));
                                mediaPlayer.Play();
                                break;

                            case ReservationState.입금대기:
                                Console.WriteLine();
                                Console.WriteLine("아래 입금대기 메일 => 무시");
                                Console.WriteLine($"{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명}");

                                var path = new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "예약을무시합니다.mp3"));
                                _richTextBox.AppendText(path.AbsolutePath);

                                _richTextBox.AppendText("\n");
                                _richTextBox.AppendText($"{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 입금대기 메일 무시");

                                mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "예약을무시합니다.mp3")));
                                mediaPlayer.Play();
                                break;

                            case ReservationState.취소:
                                _richTextBox.AppendText("\n");
                                _richTextBox.AppendText($"{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 취소 메일 도착");
                                
                                await ddnayoClient.Cancel(reservationFromMail, reservationFromPartnerCenter);

                                excel.RemoveExcel(reservationFromMail, reservationFromPartnerCenter, _info);
                                
                                Console.WriteLine();
                                Console.WriteLine("아래 취소 메일 => 처리 완료(실제 취소됐는지는 위의 로그 참고)");
                                Console.WriteLine($"{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명}");

                                _richTextBox.AppendText("\n");
                                _richTextBox.AppendText($"{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 취소 메일 처리 완료");

                                mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "예약이취소됐습니다.mp3")));
                                mediaPlayer.Play();
                                break;

                            case ReservationState.예약메일아님:
                                mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "예약메일이아닙니다.mp3")));
                                mediaPlayer.Play();
                                break;

                            case ReservationState.완료:
                                // 이런 케이스는 존재하지 않음
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{message.Date} : {message.Envelope.Subject}");
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _richTextBox.AppendText("\n" + e.ToString());
                    Console.WriteLine("페치한 메일 및 파트너센터 분석 중 에러 발생");

                    _richTextBox.AppendText("\n");
                    _richTextBox.AppendText($"페치한 메일 또는 파트너센터 분석 중 에러 발생");

                    mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "오류가발생했습니다.mp3")));
                    mediaPlayer.Play();
                }
                finally
                {
                    messages.Add(message);
                }
            }
        }

        private void GetPastReservation(Reservation reservationFromPartnerCenter)
        {
            chromeDriver.Navigate().GoToUrl($"https://partner.booking.naver.com/bizes/192655/booking-list-view/users/{reservationFromPartnerCenter.예약자번호}/bookings");
            Thread.Sleep(3000);
            var 지난예약들 = chromeDriver.FindElementByXPath(@"/html/body/div/div/div[2]/div[2]/div/div[2]/div[2]/div/div[2]/div/div/div/div/div").FindElements(By.TagName("a"));
            foreach (var 지난예약 in 지난예약들)
            {
                var 지난예약Parsed = new PastReservation(지난예약.Text.Trim());
                reservationFromPartnerCenter.지난예약들.Add(지난예약Parsed);
            }
        }

        public void ConnectPartnerCenter()
        {
            chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("disable-gpu");
            if (!Directory.Exists("C:\\boooking\\Default"))
            {
                Directory.CreateDirectory("C:\\booking\\Default");
            }
            chromeOptions.AddArgument("user-data-dir=c:\\booking");

            chromeDriver = new ChromeDriver(chromeDriverService, chromeOptions);
            chromeDriver.Navigate().GoToUrl("https://partner.booking.naver.com/bizes/192655/booking-list-view");
            Console.WriteLine();
            Console.WriteLine("예약 파트너 센터 로그인 대기 중");
            _richTextBox.AppendText("\n예약 파트너 센터 로그인 대기 중");
            do
            {
                if (!chromeDriver.Url.Contains("login"))
                {
                    break;
                }
                Thread.Sleep(1000);
            } while (true);
            Console.WriteLine("예약 파트너 센터 로그인 완료!");
            _richTextBox.AppendText("\n예약 파트너 센터 로그인 완료!");
            chromeDriver.Navigate().GoToUrl("https://partner.booking.naver.com/bizes/192655/booking-list-view");
        }

        public string GetReservationInfoStringFromPartnerCenter(string naverReservationNumber)
        {
            chromeDriver.Navigate().GoToUrl($"https://partner.booking.naver.com/bizes/192655/booking-list-view/bookings/{naverReservationNumber}");
            Thread.Sleep(3000);

            var 예약자번호 = chromeDriver.FindElementByXPath(@"/html/body/div/div/div[2]/div[2]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div[1]/div/div[1]/div/span[2]/div/a").GetAttribute("data-tst_click_link").Trim();
            var 개인정보 = chromeDriver.FindElementByXPath(@"/html/body/div[1]/div/div[2]/div[2]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div[1]").Text.Trim();
            var 예약내역 = chromeDriver.FindElementByXPath(@"/html/body/div[1]/div/div[2]/div[2]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div[2]").Text.Trim();
            var 결제정보 = chromeDriver.FindElementByXPath(@"/html/body/div[1]/div/div[2]/div[2]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div[3]").Text.Trim();
            var 툴팁 = chromeDriver.FindElementByXPath(@"/html/body/div[1]/div/div[2]/div[2]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div[3]/div[2]/div/div/div[2]").Text.Trim();
            var 옵션리스트 = 툴팁.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList().Select(item => "옵션 : " + item.Trim());
            var 옵션 = string.Join(Environment.NewLine, 옵션리스트);
            var message = nameof(예약자번호) + " " + 예약자번호 + Environment.NewLine 
                        + 개인정보 + Environment.NewLine
                        + 예약내역 + Environment.NewLine
                        + 결제정보 + Environment.NewLine
                        + 옵션;
            Console.WriteLine("++++++++++++++++ 파트너센터 추출 +++++++++++++++++++");
            Console.WriteLine(message);
            return message;
        }

        async Task WaitForNewMessagesAsync()
        {
            do
            {
                try
                {
                    if (client.Capabilities.HasFlag(ImapCapabilities.Idle))
                    {
                        done = new CancellationTokenSource(new TimeSpan(0, 9, 0));
                        try
                        {
                            await client.IdleAsync(done.Token, cancel.Token);
                        }
                        finally
                        {
                            done.Dispose();
                            done = null;
                        }
                    }
                    else
                    {
                        // Note: we don't want to spam the IMAP server with NOOP commands, so lets wait a minute
                        // between each NOOP command.
                        await Task.Delay(new TimeSpan(0, 1, 0), cancel.Token);
                        await client.NoOpAsync(cancel.Token);
                    }
                    break;
                }
                catch (ImapProtocolException e)
                {
                    Console.WriteLine(e);
                    ReconnectAsync();
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                    ReconnectAsync();
                }
            } while (true);
        }

        async Task IdleAsync()
        {
            do
            {
                try
                {
                    await WaitForNewMessagesAsync();

                    if (messagesArrived)
                    {
                        _richTextBox.AppendText("\n");
                        _richTextBox.AppendText("기존 메일 처리 중(받은편지함에서 별표 없는 메일만 찾아서 처리합니다.");
                        await ProcessMessagesAsync();
                        messagesArrived = false;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            } while (!cancel.IsCancellationRequested);
        }

        void OnCountChanged(object sender, EventArgs e)
        {
            mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "메일도착알림.mp3")));
            mediaPlayer.Play();
            var folder = (ImapFolder)sender;

            if (folder.Count > messages.Count)
            {
                int arrived = folder.Count - messages.Count;

                if (arrived > 1)
                {
                    Console.WriteLine("\t{0}개의 새 메시지 도착", arrived);
                }
                else
                {
                    Console.WriteLine("\t1 개의 새 메시지 도착.");

                }
                messagesArrived = true;
                done?.Cancel();
            }
        }

        void OnMessageExpunged(object sender, MessageEventArgs e)
        {
            var folder = (ImapFolder)sender;

            if (e.Index < messages.Count)
            {
                var message = messages[e.Index];

                Console.WriteLine("{0}: #{1}번 메시지 삭제됨 : {2}", folder, e.Index, message.Envelope.Subject);
                messages.RemoveAt(e.Index);
            }
            else
            {
                Console.WriteLine("{0}: #{1}번 메시지 삭제됨", folder, e.Index);
            }
        }

        void OnMessageFlagsChanged(object sender, MessageFlagsChangedEventArgs e)
        {
            var folder = (ImapFolder)sender;

            Console.WriteLine("{0}: #{1}번 메시지의 플래그가 바뀜 ({2}).", folder, e.Index, e.Flags);
        }
        public void Exit()
        {
            cancel.Cancel();
        }

        public void Dispose()
        {
            client.Dispose();
            cancel.Dispose();
        }
    }
}
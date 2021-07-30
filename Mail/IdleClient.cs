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

        public Setting setting = new Setting();
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
        public RichTextBox richTextBox;
        public MediaPlayer mediaPlayer;

        // 통계 상태 관리
        public bool first;
        public List<Reservation> parsed { get; set; } = new List<Reservation>();

        public IdleClient(Setting _setting, RichTextBox _richTextBox)
        {
            setting = _setting;
            richTextBox = _richTextBox;
            client = new ImapClient();
            cancel = new CancellationTokenSource();
            mediaPlayer = new MediaPlayer();

            ddnayoClient = new DdnayoClient();
        }

        public async Task RunAsync()
        {
            try
            {
                ReconnectAsync();
                richTextBox.AppendText("\n통계 처리 중");
                await GetStatistics("20:00");
                ConnectPartnerCenter();
                richTextBox.AppendText("\n받은편지함에서 별표 없는 메일 처리 시작");
                await ProcessMessagesAsync();
            }
            catch (OperationCanceledException e)
            {
                richTextBox.AppendText($"{e}");
                await client.DisconnectAsync(true);
                return;
            }
            var inbox = client.Inbox;
            inbox.CountChanged += OnCountChanged;
            inbox.MessageExpunged += OnMessageExpunged;
            inbox.MessageFlagsChanged += OnMessageFlagsChanged;
            richTextBox.AppendText($"\n/*({DateTime.Now.ToString("MM-dd hh:mm:ss")})");
            await IdleAsync();
            richTextBox.AppendText($"\n/****({DateTime.Now.ToString("MM-dd hh:mm:ss")})");
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
            }
            richTextBox.AppendText("\n메일 접속 및 로그인함");
        }

        public async Task GetStatistics(string baseTime, Reservation newReservation = null)  // baseTime 예시 => 17:05
        {
            await Task.Delay(100);
            List<UniqueId> delivered;
            int retry = 0;
            string[] 시분분리 = baseTime.Split(":");
            DateTime 지금시각 = DateTime.Now;
            DateTime 기준시각 = new DateTime(지금시각.Year, 지금시각.Month, 지금시각.Day, int.Parse(시분분리[0].Trim()), int.Parse(시분분리[1].Trim()), 0);

            if ((지금시각 - 기준시각).TotalSeconds < 0)
            {
                기준시각 = 기준시각.AddDays(-1);
            }
            if (newReservation is null)
            {
                do
                {
                    try
                    {
                        //richTextBox.AppendText($"{지금시각.ToString()} {지금시각.Kind.ToString()}");
                        delivered = client.Inbox.Search(SearchQuery.DeliveredAfter(기준시각).And(SearchQuery.HasGMailLabel(@"\Starred"))).ToList();  // The resolution of this search query does not include the time.
                        foreach (var message in delivered)
                        {
                            // 메일 분석
                            MimeMessage mimeMessage = client.Inbox.GetMessage(new UniqueId(message.Id));

                            if ((mimeMessage.Date.DateTime - 기준시각).TotalSeconds > 0)
                            {
                                var reservationFromMail = new Reservation(mimeMessage, richTextBox);
                                //richTextBox.AppendText($"\n{mimeMessage.Date.DateTime.ToString("yy-MM-dd HH:mm:ss")} => {reservationFromMail.예약상태.ToString()}");

                                if (parsed.Where(item => item.예약번호네이버.Equals(reservationFromMail.예약번호네이버) && item.예약상태.Equals(reservationFromMail.예약상태)).Count() == 0)
                                {
                                    richTextBox.AppendText("\n PPPPPPP");
                                    parsed.Add(reservationFromMail);
                                }
                            }
                        }
                        break;
                    }
                    catch (Exception e)
                    {
                        retry++;
                        if (retry >= 3)
                        {
                            richTextBox.AppendText($"\n{e.ToString()}");
                            richTextBox.AppendText($"\n통계 처리 중 오류 발생. 오류 메시지를 개발자에게 보내주세요.");
                            break;
                        }
                    }
                    finally
                    {
                        ReconnectAsync();
                    }
                } while (true);

            }
            else // 새 메일 통계 추가
            {
                if (parsed.Where(item => item.예약번호네이버.Equals(newReservation.예약번호네이버) && item.예약상태.Equals(newReservation.예약상태)).Count() == 0)
                {
                    parsed.Add(newReservation);
                    richTextBox.AppendText($"\n{newReservation.메일수신일시.ToString()}");
                }
            }

            parsed.RemoveAll(item => (item.메일수신일시 - 기준시각).TotalSeconds < 0);

            var 전체메일수 = parsed.Count;
            var 예약메일수 = parsed.Where(item => item.예약상태 is ReservationState.확정).Count();
            var 취소메일수 = parsed.Where(item => item.예약상태 is ReservationState.취소).Count();
            var 무시메일수 = parsed.Where(item => item.예약상태 is ReservationState.예약메일아님 or ReservationState.소액테스트입금대기 or ReservationState.소액테스트확정 or ReservationState.입금대기).Count();
            var 오류메일수 = parsed.Where(item => item.예약상태 is ReservationState.분석중오류발생).Count();
            richTextBox.AppendText($"\nㅡㅡㅡㅡㅡ  {기준시각.ToString("MM-dd HH:mm:ss")} 이후 도착한 메일에 대한 통계  ㅡㅡㅡㅡㅡ");
            richTextBox.AppendText($"\n          전체 : {전체메일수}, 예약 : {예약메일수}, 취소 : {취소메일수}, 무시 : {무시메일수}, 오류 : {오류메일수}");
            richTextBox.AppendText($"\nㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ");
        }

        async Task ProcessMessagesAsync()
        {
            IList<UniqueId> starred;
            int retry = 0;
            do
            {
                try
                {
                    starred = client.Inbox.Search(SearchQuery.Not(SearchQuery.HasGMailLabel(@"\Starred"))).OrderBy(item => item.Id).ToList();
                    foreach (UniqueId message in starred)
                    {
                        mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "메일도착알림.mp3")));
                        mediaPlayer.Play();
                        await Task.Delay(1000);
                        // 메일 분석
                        MimeMessage mimeMessage = client.Inbox.GetMessage(new UniqueId(message.Id));
                        Reservation reservationFromMail = new Reservation(mimeMessage, richTextBox);

                        // 예약파트너센터 분석
                        string reservationInfoStringFromPartnerCenter;
                        Reservation reservationFromPartnerCenter = new Reservation();
                        if (reservationFromMail.예약상태 is not ReservationState.예약메일아님)
                        {
                            reservationInfoStringFromPartnerCenter = GetReservationInfoStringFromPartnerCenter(reservationFromMail.예약번호네이버);
                            reservationFromPartnerCenter = new Reservation(reservationInfoStringFromPartnerCenter, richTextBox);
                            GetPastReservation(reservationFromPartnerCenter);
                            GetManagerMemo(reservationFromMail, reservationFromPartnerCenter);
                        }

                        // 떠나요 및 엑셀 처리
                        switch (reservationFromMail.예약상태)
                        {
                            case ReservationState.확정:
                                richTextBox.AppendText($"\n\n@ 떠나요 등록 중 @");
                                richTextBox.AppendText($"\n{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 예약 메일");

                                //await ddnayoClient.Ready(reservationFromMail, reservationFromPartnerCenter);

                                richTextBox.AppendText($"\n\n@ 엑셀 파일 작성 중 @");

                                //excel.Save(reservationFromMail, reservationFromPartnerCenter, _info);

                                richTextBox.AppendText($"\n\n{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 예약 메일 처리 완료 => 제대로 처리되었는지 주기적으로 확인해 주세요.");
                                mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "예약이등록됐습니다.mp3")));
                                mediaPlayer.Play();
                                await Task.Delay(2500);
                                break;

                            case ReservationState.취소:
                                richTextBox.AppendText($"\n\n@ 떠나요 취소 중 @");
                                richTextBox.AppendText($"\n{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 취소 메일");

                                //await ddnayoClient.Cancel(reservationFromMail, reservationFromPartnerCenter);

                                richTextBox.AppendText($"\n\n@ 엑셀 파일 삭제 중 @");
                                //excel.RemoveExcel(reservationFromMail, reservationFromPartnerCenter, _info);

                                richTextBox.AppendText($"\n{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 취소 메일 처리 완료 => 제대로 처리되었는지 주기적으로 확인해 주세요.");
                                mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "예약이취소됐습니다.mp3")));
                                mediaPlayer.Play();
                                await Task.Delay(2500);
                                break;

                            case ReservationState.소액테스트입금대기:
                                richTextBox.AppendText($"\n\n{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 소액 테스트 메일 => 무시");
                                mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "예약을무시합니다.mp3")));
                                mediaPlayer.Play();
                                await Task.Delay(2500);
                                break;

                            case ReservationState.소액테스트확정:
                                richTextBox.AppendText($"\n\n{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 소액 테스트 메일 => 무시");
                                mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "예약을무시합니다.mp3")));
                                mediaPlayer.Play();
                                await Task.Delay(2500);
                                break;

                            case ReservationState.입금대기:
                                richTextBox.AppendText($"\n\n{mimeMessage.Date} : {reservationFromPartnerCenter.예약자명} : 입금대기 메일 무시");
                                mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "예약을무시합니다.mp3")));
                                mediaPlayer.Play();
                                await Task.Delay(2500);
                                break;

                            case ReservationState.예약메일아님:
                                richTextBox.AppendText("\n\n");
                                richTextBox.AppendText($"{mimeMessage.Date} : 예약형식의 메일 아님");
                                mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "예약메일이아닙니다.mp3")));
                                mediaPlayer.Play();
                                await Task.Delay(2500);
                                break;

                            case ReservationState.완료:
                                // Reservation에는 이런 케이스가 존재하지 않음
                                // PastReservation에는 완료 존재함
                                await Task.Delay(25000);
                                break;
                            default:
                                break;
                        }
                        client.Inbox.AddLabels(new UniqueId(message.Id), new String[] { @"\Starred" }, true);
                        await GetStatistics("20:00", reservationFromMail);
                    }
                    starred = client.Inbox.Search(SearchQuery.Not(SearchQuery.HasGMailLabel(@"\Starred"))).OrderBy(item => item.Id).ToList();
                    if (starred.Count == 0)
                    {
                        break;
                    }
                }
                catch (ImapProtocolException e)
                {
                    richTextBox.AppendText($"\n{e.ToString()}");
                    richTextBox.AppendText($"\n메일 또는 파트너센터 분석 중 오류 발생");
                    mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "오류가발생했습니다.mp3")));
                    mediaPlayer.Play();
                }
                catch (IOException e)
                {
                    richTextBox.AppendText($"\n{e.ToString()}");
                    richTextBox.AppendText($"\n메일 또는 파트너센터 분석 중 오류 발생");
                    mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "오류가발생했습니다.mp3")));
                    mediaPlayer.Play();
                }
                catch (Exception e)
                {
                    richTextBox.AppendText($"\n{e.ToString()}");
                    richTextBox.AppendText($"\n알 수 없는 오류 발생");
                    mediaPlayer.Open(new Uri(Path.Combine(Environment.CurrentDirectory, "Media", "오류가발생했습니다.mp3")));
                    mediaPlayer.Play();
                    retry++;
                    if (retry >= 3)
                    {
                        richTextBox.AppendText($"\n");
                        richTextBox.AppendText($"\n예약을 처리하지 못하였습니다. 오류 메시지를 개발자에게 보내주세요.");
                        break;
                    }
                }
                finally
                {
                    ReconnectAsync();
                }
            } while (true);
        }

        private void GetPastReservation(Reservation reservationFromPartnerCenter)
        {
            chromeDriver.Navigate().GoToUrl($"https://partner.booking.naver.com/bizes/192655/booking-list-view/users/{reservationFromPartnerCenter.예약자번호}/bookings");
            Thread.Sleep(3000);
            var 지난예약들 = chromeDriver.FindElementByXPath(@"/html/body/div/div/div[2]/div[2]/div/div[2]/div[2]/div/div[2]/div/div/div/div/div").FindElements(By.TagName("a"));
            foreach (var 지난예약 in 지난예약들)
            {
                int index = 지난예약들.IndexOf(지난예약);
                var 지난예약Parsed = new PastReservation(지난예약.Text.Trim(), richTextBox, index);
                reservationFromPartnerCenter.지난예약들.Add(지난예약Parsed);
            }
        }

        private void GetManagerMemo(Reservation reservationFromMail, Reservation reservationFromPartnerCenter)
        {
            string managerMemo = $"{reservationFromMail.예약신청일시.Value.ToString("M월 d일")} {reservationFromMail.유입경로떠나요기록인데실제내용은결제수단임}{Environment.NewLine}" +
                              $"{reservationFromPartnerCenter.예약자명}{Environment.NewLine}" +
                              $"{reservationFromMail.이용시작일시.Value.ToString("M월 d일 dddd")} {reservationFromMail.객실} {reservationFromMail.이용일수}박{reservationFromMail.이용일수 + 1}일 {reservationFromMail.메모용객실이용금액}{Environment.NewLine}" +
                              $"{string.Join(Environment.NewLine, reservationFromMail.메모용옵션들)}{(reservationFromMail.메모용옵션들.Count != 0 ? Environment.NewLine : string.Empty)}";
            int 완료예약수 = reservationFromPartnerCenter.지난예약들.Where(item => item.예약상태 == ReservationState.완료).Count();
            if (완료예약수 == 0)
            {
                reservationFromPartnerCenter.떠나요메모사항 = managerMemo;
                reservationFromPartnerCenter.엑셀방문횟수 = "신규예약";
            }
            else
            {
                // 한 줄이 20글자 이상일 때 처리
                //var pastReservationsMemo = string.Empty;
                //var newMemoLineToAdd = $"{reservationFromPartnerCenter.지난예약들.Count}회 방문 : ";
                //foreach (var 지난예약 in reservationFromPartnerCenter.지난예약들)
                //{
                //    var 객실명 = 지난예약.객실;
                //    var 이용시작날짜 = 지난예약.이용시작일시.Value.ToString("yy/M/d");
                //    var newMemoToAdd = $"{객실명}({이용시작날짜})";
                //    if ((newMemoLineToAdd + newMemoToAdd).Length < 20)
                //    {
                //        newMemoLineToAdd += newMemoToAdd;
                //    }
                //    else
                //    {
                //        pastReservationsMemo += newMemoLineToAdd;
                //        newMemoLineToAdd = Environment.NewLine + newMemoToAdd;
                //    }
                //}
                //pastReservationsMemo += newMemoLineToAdd;


                // 한 줄이 20글자 이상이더라도 그냥 이어 붙여도 상관없음. 단, 엑셀 파일에서 자동줄바꿈 설정을 해야 함.

                var pastReservationsMemo = string.Empty;
                foreach (var 지난예약 in reservationFromPartnerCenter.지난예약들.OrderBy(item => item.이용시작일시))
                {
                    if (지난예약.예약상태 == ReservationState.완료)
                    {
                        var 객실명 = 지난예약.객실;
                        var 이용시작날짜 = 지난예약.이용시작일시.Value.ToString("yy/M/d").Replace("-", "/");
                        var newMemoToAdd = $"{객실명}({이용시작날짜}), ";
                        pastReservationsMemo += newMemoToAdd;
                    }
                }
                if (pastReservationsMemo.Length >= 2)
                {
                    pastReservationsMemo = pastReservationsMemo.Substring(0, pastReservationsMemo.Length - 2); // 마지막 콤마 제거
                }
                reservationFromPartnerCenter.떠나요메모사항 = managerMemo + pastReservationsMemo;
            }
            richTextBox.AppendText($"\n\n@ 떠나요 메모사항 @\n{reservationFromPartnerCenter.떠나요메모사항}");
            richTextBox.AppendText($"\n\n@ 거래내역서 방문횟수 @\n{reservationFromPartnerCenter.엑셀방문횟수}");
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
            richTextBox.AppendText("\n예약 파트너 센터 로그인 대기 중");
            do
            {
                if (!chromeDriver.Url.Contains("login"))
                {
                    break;
                }
                Thread.Sleep(1000);
            } while (true);
            richTextBox.AppendText("\n예약 파트너 센터 로그인 완료!");
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
            richTextBox.AppendText("\n\n@ 예약파트너센터에서 텍스트 추출 @");
            richTextBox.AppendText($"\n{message}");
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
                            richTextBox.AppendText($"\n/***({DateTime.Now.ToString("MM-dd hh:mm:ss")})");
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
                    richTextBox.AppendText($"\n{e.ToString()}");
                    ReconnectAsync();
                }
                catch (IOException e)
                {
                    richTextBox.AppendText($"\n{e.ToString()}");
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
                    richTextBox.AppendText($"\n/**({DateTime.Now.ToString("MM-dd hh:mm:ss")})");
                    await WaitForNewMessagesAsync();

                    if (messagesArrived)
                    {
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
            done?.Cancel();
            messagesArrived = true;
        }

        void OnMessageExpunged(object sender, MessageEventArgs e)
        {

        }

        void OnMessageFlagsChanged(object sender, MessageFlagsChangedEventArgs e)
        {
            done?.Cancel();
            messagesArrived = true;
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
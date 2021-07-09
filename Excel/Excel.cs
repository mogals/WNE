using System;
using System.IO;
using NPOI.XSSF;
using NPOI.XSSF.UserModel;
using WNE.Ddnayo;
using NPOI.SS.UserModel;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using NE.Naver;
using System.Linq;
using System.Collections.Generic;
using Ddnayo.Ddnayo;
using NPOI.SS.Util;
using WNE.Parsing;
using WNE;
using System.Windows.Media;

namespace Report
{
    public class Excel
    {
        public string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "거래내역서");
        PartnerCenterInfo partenerCenterInfo = new PartnerCenterInfo();
        MediaPlayer mediaPlayer;
        public Excel()
        {
            mediaPlayer = new MediaPlayer();
        }
        List<옵션가격Info> 옵션가격표 = new List<옵션가격Info>()
        {
            new 옵션가격Info()
            {
                옵션명 = "바베큐A",
                가격 = 40000,
            },
            new 옵션가격Info()
            {
                옵션명 = "바베큐B",
                가격 = 40000,
            },
            new 옵션가격Info()
            {
                옵션명 = "바베큐C",
                가격 = 40000,
            },
            new 옵션가격Info()
            {
                옵션명 = "고기추가A",
                가격 = 40000,
            },
            new 옵션가격Info()
            {
                옵션명 = "고기추가B",
                가격 = 40000,
            },
            new 옵션가격Info()
            {
                옵션명 = "쉐프조리",
                가격 = 40000,
            },
            new 옵션가격Info()
            {
                옵션명 = "조식방문",
                가격 = 40000,
            },
            new 옵션가격Info()
            {
                옵션명 = "조식딜리버리",
                가격 = 40000,
            },

        };

        public void Save(Reservation reservationFromMail, Reservation reservationFromPartnerCeter, Info info)
        {
            var 거래내역서파일 = Path.Combine(Environment.CurrentDirectory, "거래내역서.xlsx");
            var workbook = GetWorkbook(거래내역서파일);
            var sheet = workbook.GetSheetAt(0);

            // 거래일자, 공급받는자
            sheet.GetCell(3, 3).SetCellValue(reservationFromPartnerCeter.이용시작일시.Value.ToString("yyyy년 M월 d일 dddd"));
            sheet.GetCell(4, 3).SetCellValue(reservationFromPartnerCeter.예약자명);
            sheet.GetCell(5, 3).SetCellValue(reservationFromPartnerCeter.전화번호);
            sheet.GetCell(6, 3).SetCellValue(reservationFromPartnerCeter.예약번호네이버);
            sheet.GetCell(7, 3).SetCellValue(reservationFromPartnerCeter.이메일);

            // 총 이용 금액
            sheet.GetCell(10, 4).SetCellFormula("K44");

            // 객실요금을 주중금액과 주말금액으로 분리
            // 일월화수목         => 주중
            // 금토               => 주말
            // 일월화수목 & 공휴일 => 주말
            var prices = GetNaverPrice(reservationFromMail, reservationFromPartnerCeter); // 7일간 가격 추출
            var days = reservationFromMail.이용일수.Value;
            int x;                    // 첫번째숙박일수
            int pricex = prices[0];   // 첫번째숙박금액
            int y = 0;                // 두번째숙박일수
            int pricey = prices[1];   // 두번째숙박금액
            for (x = 0; x <= days; x++)
            {
                y = days - x;
                int 추정숙박금액 = pricex * x + pricey * y;
                if (추정숙박금액.Equals(reservationFromMail.객실금액))
                {
                    break;
                }
            }

            // 일차 연립방정식의 해를 찾지 못하고 for문을 탈출한 경우?
            if (x == days +1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("테스트 메일? 방정식을 풀지 못 함");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // 객실 입력 준비
            bool 첫번째숙박이주중인가 = false;
            if (pricex < pricey)
            {
                첫번째숙박이주중인가 = true;
            }
            var adultOccupancy = GetNaverRoomInfo(reservationFromMail).adultOccupancy;
            
            // 객실 첫 번째
            sheet.GetCell(14, 2).SetCellValue($"{reservationFromMail.객실} ({adultOccupancy}인)");
            if (첫번째숙박이주중인가)
            {
                sheet.GetCell(14, 9).SetCellValue($"주중");
            }
            else
            {
                sheet.GetCell(14, 9).SetCellValue($"주말");
            }
            sheet.GetCell(14, 10).SetCellValue($"{x} 박");
            sheet.GetCell(14, 11).SetCellValue(pricex);
            sheet.GetCell(14, 12).SetCellValue(pricex * x);

            // 객실 두 번째
            if (y != 0)
            {
                sheet.GetCell(15, 2).SetCellValue($"{reservationFromMail.객실} ({adultOccupancy}인)");
                if (첫번째숙박이주중인가)
                {
                    sheet.GetCell(15, 9).SetCellValue($"주말");
                }
                else
                {
                    sheet.GetCell(15, 9).SetCellValue($"주중");
                }
                sheet.GetCell(15, 10).SetCellValue($"{y} 박");
                sheet.GetCell(15, 11).SetCellValue(pricey);
                sheet.GetCell(15, 12).SetCellValue(pricey * y);
            }

            // 식사 옵션
            foreach (var item in reservationFromMail.옵션들)
            {
                var 옵션명 = item.옵션명;
                if (옵션명.Contains("바베큐") && 옵션명.Contains("A"))
                {
                    sheet.GetCell(18, 10).SetCellValue(item.옵션사람수또는개수);
                    sheet.GetCell(18, 11).SetCellValue(item.옵션단품금액);
                    sheet.GetCell(18, 12).SetCellValue(item.옵션총금액);
                }

                if (옵션명.Contains("바베큐") && 옵션명.Contains("B"))
                {
                    sheet.GetCell(19, 10).SetCellValue(item.옵션사람수또는개수);
                    sheet.GetCell(19, 11).SetCellValue(item.옵션단품금액);
                    sheet.GetCell(19, 12).SetCellValue(item.옵션총금액);
                }

                if (옵션명.Contains("바베큐") && 옵션명.Contains("C"))
                {
                    sheet.GetCell(20, 10).SetCellValue(item.옵션사람수또는개수);
                    sheet.GetCell(20, 11).SetCellValue(item.옵션단품금액);
                    sheet.GetCell(20, 12).SetCellValue(item.옵션총금액);
                }

                if (옵션명.Contains("고기") && 옵션명.Contains("A"))
                {
                    sheet.GetCell(21, 10).SetCellValue(item.옵션사람수또는개수);
                    sheet.GetCell(21, 11).SetCellValue(item.옵션단품금액);
                    sheet.GetCell(21, 12).SetCellValue(item.옵션총금액);
                }

                if (옵션명.Contains("고기") && 옵션명.Contains("B"))
                {
                    sheet.GetCell(22, 10).SetCellValue(item.옵션사람수또는개수);
                    sheet.GetCell(22, 11).SetCellValue(item.옵션단품금액);
                    sheet.GetCell(22, 12).SetCellValue(item.옵션총금액);
                }

                if (옵션명.Contains("쉐프"))
                {
                    sheet.GetCell(23, 10).SetCellValue(item.옵션사람수또는개수);
                    sheet.GetCell(23, 11).SetCellValue(item.옵션단품금액);
                    sheet.GetCell(23, 12).SetCellValue(0);
                }

                if (옵션명.Contains("조식") && 옵션명.Contains("방문"))
                {
                    sheet.GetCell(24, 10).SetCellValue(item.옵션사람수또는개수);
                    sheet.GetCell(24, 11).SetCellValue(item.옵션단품금액);
                    sheet.GetCell(24, 12).SetCellValue(item.옵션총금액);
                }

                if (옵션명.Contains("조식") && 옵션명.Contains("딜리버리"))
                {
                    sheet.GetCell(25, 10).SetCellValue(item.옵션사람수또는개수);
                    sheet.GetCell(25, 11).SetCellValue(item.옵션단품금액);
                    sheet.GetCell(25, 12).SetCellValue(item.옵션총금액);
                }
            }

            // 네이버예약 합계
            sheet.GetCell(26, 8).SetCellFormula("SUM(M15:M16,M19:M26)");

            // 조건부 서식 설정(네이버 예약)
            var conditionFormatting = sheet.SheetConditionalFormatting;
            var firstRule = conditionFormatting.CreateConditionalFormattingRule("$M15<>\"\"");
            var patternFormat = firstRule.CreatePatternFormatting();
            patternFormat.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
            patternFormat.FillPattern = FillPattern.SolidForeground;
            var regions = new CellRangeAddress[] {
                new CellRangeAddress(14, 15, 1, 12),
                new CellRangeAddress(18, 25, 1, 12),
            };
            conditionFormatting.AddConditionalFormatting(regions, firstRule);

            // 현장결제 및 당일 결제
            // 식사
            sheet.GetCell(30, 12).SetCellFormula("L31*K31");
            sheet.GetCell(31, 12).SetCellFormula("L32*K32");
            sheet.GetCell(32, 12).SetCellFormula("L33*K33");
            sheet.GetCell(33, 12).SetCellFormula("L34*K34");
            sheet.GetCell(34, 12).SetCellFormula("L35*K35");
            // 기타옵션
            sheet.GetCell(36, 12).SetCellFormula("L37*K37");
            sheet.GetCell(37, 12).SetCellFormula("L38*K38");
            sheet.GetCell(38, 12).SetCellFormula("L39*K39");
            sheet.GetCell(39, 12).SetCellFormula("L40*K40");
            sheet.GetCell(40, 12).SetCellFormula("L41*K41");

            // 현장결제 합계
            sheet.GetCell(41, 8).SetCellFormula("SUM(M31:M35,M37:M41)");

            // 총 이용금액
            sheet.GetCell(43, 10).SetCellFormula("SUM(M15:M16,M19:M26,M31:M35,M37:M41)");
            // 껼제완료 (네이버결제)
            sheet.GetCell(46, 10).SetCellFormula("I27");
            // 현장결제
            sheet.GetCell(48, 10).SetCellFormula("I42");
            // 잔금결제?? 항상 0?? 무슨 뜻인지 알아야 함
            sheet.GetCell(50, 10).SetCellFormula("K44-K47-K49");

            // 조건부 서식 설정(현장결제 및 당일결제)
            var secondRule = conditionFormatting.CreateConditionalFormattingRule("$K31<>\"\"");
            var secondPatternFormat = secondRule.CreatePatternFormatting();
            secondPatternFormat.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
            secondPatternFormat.FillPattern = FillPattern.SolidForeground;
            var secondRegions = new CellRangeAddress[] {
                new CellRangeAddress(30, 34, 1, 12),
                new CellRangeAddress(36, 40, 1, 12),
            };
            conditionFormatting.AddConditionalFormatting(secondRegions, secondRule);

            // 결제완료
            sheet.GetCell(46, 7).SetCellValue(reservationFromMail.예약신청일시.Value.ToString("MM월 dd일"));
            sheet.GetCell(47, 7).SetCellValue(GetFlowInSaleDomainHomepage(reservationFromMail.결제수단));

            // 요청사항
            sheet.GetCell(50, 2).SetCellValue(reservationFromMail.요청사항);

            // 수식계산
            XSSFFormulaEvaluator.EvaluateAllFormulaCells(workbook);

            // 저장
            WriteExcel(workbook, reservationFromMail, reservationFromPartnerCeter, info);
        }

        public string GetFlowInSaleDomainHomepage(string 결제수단)
        {
            switch (결제수단)
            {
                case "신용카드":
                    return "네이버 카드결제";
                    break;
                case "무통장입금":
                    return $"네이버 {결제수단}";
                    break;
                case "포인트결제":
                    return $"네이버 {결제수단}";
                    break;
                case "계좌이체":
                    return $"네이버 {결제수단}";
                    break;
                default:
                    return $"{결제수단} : 예상하지 못한 결제수단 발생";
                    break;
            }
        }
        public IWorkbook GetWorkbook(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return new XSSFWorkbook(stream);


            }
        }

        public void WriteExcel(IWorkbook workbook, Reservation reservationFromMail, Reservation reservationFromPartnerCenter, Info info)
        {
            defaultPath = info.saveFolder;
            string year = reservationFromMail.이용시작일시.Value.ToString("yyyy년");
            string month = reservationFromMail.이용시작일시.Value.ToString("M월");
            string date = reservationFromMail.이용시작일시.Value.ToString("MM.dd");
            string name = reservationFromPartnerCenter.예약자명.Replace(" ", string.Empty); ;
            string roomName = reservationFromMail.객실;
            string folderPath = Path.Combine(defaultPath, year, month);
            string fileName = $"거래내역서 {date} {name} ({roomName}).xlsx";
            string filePath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(file);
            }
        }

        public void RemoveExcel(Reservation reservationFromMail, Reservation reservationFromPartnerCenter, Info info)
        {
            defaultPath = info.saveFolder;
            string year = reservationFromMail.이용시작일시.Value.ToString("yyyy년");
            string month = reservationFromMail.이용시작일시.Value.ToString("M월");
            string date = reservationFromMail.이용시작일시.Value.ToString("MM.dd");
            string name = reservationFromPartnerCenter.예약자명.Replace(" ", string.Empty);
            string roomName = reservationFromMail.객실;
            string folderPath = Path.Combine(defaultPath, year, month);
            string fileName = $"거래내역서 {date} {name} ({roomName}).xlsx";
            string filePath = Path.Combine(folderPath, fileName);
            try
            {
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("파일 삭제 실패");
                Console.WriteLine("오류 로그 확인 후 코드 수정");
                throw;
            }
        }
        public List<int> GetNaverPrice(Reservation reservationFromMail, Reservation reservationFromPartnerCenter)
        {
            var cookies = new CookieContainer();
            var handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            var httpClient = new HttpClient(handler);
            var roomId = GetNaverRoomInfo(reservationFromMail).roomId;
            string startDateTime = reservationFromMail.이용시작일시.Value.ToString("yyyy-MM-ddT00:00:00");
            string endDateTime = reservationFromMail.이용시작일시.Value.AddDays(7).ToString("yyyy-MM-ddT00:00:00");
            Console.WriteLine($"startDateTime : {startDateTime}");
            Console.WriteLine($"endDateTime : {endDateTime}");
            var url = $"https://api.booking.naver.com/v3.0/businesses/192655/biz-items/{roomId}/daily-schedules?lang=ko&endDateTime={endDateTime}&startDateTime={startDateTime}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.UserAgent.ParseAdd(" Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36");

            HttpResponseMessage response = httpClient.Send(requestMessage);

            var result = response.Content.ReadAsStringAsync().Result;
            dynamic json = JsonConvert.DeserializeObject<dynamic>(result);
            List<int> prices = new List<int>();
            Console.WriteLine("일주일간 가격 출력");
            for (int i = 0; i < 7; i++)
            {
                string date = reservationFromMail.이용시작일시.Value.AddDays(i).ToString("yyyy-MM-dd");
                var price = (int)json[date]["prices"][0]["price"];
                Console.WriteLine(price);
                prices.Add(price);
            }
            Console.WriteLine("일주일간 가격 출력");
            return prices.Distinct().ToList();
        }

        private RoomInfo GetNaverRoomInfo(Reservation reservationFromMail)
        {
            var naverRoomInfo = partenerCenterInfo.rooms.Where(item => item.객실.Equals(reservationFromMail.객실))
                                                        .FirstOrDefault();
            return naverRoomInfo;
        }
    }

    public static class Extension
    {
        public static ICell GetCell(this ISheet sheet, int rownum, int cellnum)
        {
            var row = sheet.GetRow(rownum);
            if (row == null)
            {
                row = sheet.CreateRow(rownum);
            }
            var cell = row.GetCell(cellnum);
            if (cell == null)
            {
                cell = row.CreateCell(cellnum);
            }
            return cell;
        }

    }



}

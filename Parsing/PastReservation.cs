using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WNE.Parsing
{
    public class PastReservation
    {
        public ReservationState 예약상태;
        public string 예약번호네이버;
        public string 예약번호떠나요;
        public string 예약상품;
        private string 객실raw;
        public string 객실;  // 예약상품에서 추출
        public int 인원수;  // 예약상품에서 추출
        public int 수량;
        public string 이용기간; // 메일에서는 "이용일시"
        public DateTime? 이용시작일시;
        public DateTime? 이용종료일시;
        public int? 이용일수;
        public string 요청사항;

        public PastReservation() { }
        public PastReservation(string partnerCenterMessage)
        {
            Console.WriteLine();
            Console.WriteLine("======= 지난예약 분석 중 =========");
            try
            {
                var text = partnerCenterMessage;

                if (text.Contains("확정"))
                {
                    예약상태 = ReservationState.확정;
                }
                else if (text.Contains("취소"))
                {
                    예약상태 = ReservationState.취소;
                }
                else if (text.Contains("완료"))
                {
                    예약상태 = ReservationState.완료;
                }
                else
                {
                    throw new NotReservationMailException();
                }

                var textBodyToArray = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (var item in textBodyToArray)
                {
                    string 제목 = string.Empty;
                    string 내용 = string.Empty;
                    foreach (var jtem in item)
                    {
                        if (char.IsWhiteSpace(jtem))
                        {
                            var index = item.IndexOf(jtem);
                            제목 = item.Substring(0, index).Trim();
                            내용 = item.Substring(index + 1, item.Length - index - 1).Trim();
                            break;
                        }
                    }

                    // 추출을 하는 과정은 프로퍼티에 집어 넣는 것이 보기 좋을 듯
                    switch (제목)
                    {
                        case "예약번호":
                            Console.WriteLine($"{제목} : {내용}");
                            예약번호네이버 = 내용;
                            break;
                        case "확정":
                        case "취소":
                        case "완료":
                            Console.WriteLine($"{제목} : {내용}");
                            객실raw = 내용;
                            var 객실정리 = 객실raw.Replace(" ", string.Empty)
                                               .Replace("(스위트룸)", string.Empty)
                                              // 몇 인실인지 안 나와 있는 경우도 있음!!
                                              // 결제금액에서 읽어와야?
                                              .Replace("인실", string.Empty)
                                              .Replace("인", string.Empty);
                            var 예약상품분리 = 객실정리.Split("(");
                            객실 = 예약상품분리[0].Replace(" ", string.Empty).Trim();
                            try
                            {
                                인원수 = int.Parse(예약상품분리[1].Replace(")", string.Empty).Trim());
                            }
                            catch (IndexOutOfRangeException)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("객실정보에서 객실 인원수 추출 불가 => 0으로 처리");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"객실 추출 : {객실}");
                            Console.WriteLine($"객실 인원수 추출 : {인원수}");
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case "이용기간":
                            Console.WriteLine($"{제목} : {내용}");
                            이용기간 = 내용;
                            var 이용기간분리 = 이용기간.Replace(" ", string.Empty)
                                                     .Replace(".(", "~")
                                                     .Replace("(", "~")
                                                     .Replace(")", "~")
                                                     .Split('~', StringSplitOptions.RemoveEmptyEntries);
                            이용시작일시 = DateTime.ParseExact(이용기간분리[0], "yyyy.M.d", null);
                            이용종료일시 = DateTime.ParseExact(이용기간분리[2], "yyyy.M.d", null);
                            이용일수 = (이용종료일시.Value - 이용시작일시.Value).Days;
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"이용시작일시 추출 : {이용시작일시}");
                            Console.WriteLine($"이용종료일시 추출 : {이용종료일시}");
                            Console.WriteLine($"이용일수 추출 : {이용일수}박 {이용일수 + 1}일");
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case "수량":
                            Console.WriteLine($"{제목} : {내용}");
                            수량 = int.Parse(내용);
                            break;
                        case "요청사항":
                            Console.WriteLine($"{제목} : {내용}");
                            요청사항 = 내용.Trim();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (NotReservationMailException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("파트너센터에서 자료 조회 중 예약 형식에 맞지 않는 자료 발견");
                Console.WriteLine($"{partnerCenterMessage}");
                예약상태 = ReservationState.예약메일아님;
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (FormatException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.WriteLine("파트너센터 지난내역 자료 조회 중 예상치 못 한 예외 발생 => 개발자에게 문의해 주세요.");
                Console.WriteLine($"예외발생 메일 : {partnerCenterMessage}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}

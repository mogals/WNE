using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Xml;

namespace WNE.Parsing
{
    public class ReservationStatistics
    {
        public ReservationState 예약상태;
        public string 결제금액raw; // 예약상품도 문자열에 같이 들어가 있음
        public int 결제금액;

        public ReservationStatistics(MimeMessage mimeMessage)
        {
            string textBody = string.Empty;
            try
            {
                textBody = mimeMessage.TextBody?.Replace("예약신청 일시", "예약신청일시")
                                                .Replace("예약취소 일시", "예약취소일시")
                                                .Replace("입금 대기", "입금대기");
                if (textBody.Contains("@@@"))
                {
                    예약상태 = ReservationState.테스트메일;
                }
                if (textBody.Contains("확정"))
                {
                    예약상태 = ReservationState.확정;
                }
                else if (textBody.Contains("입금대기"))
                {
                    예약상태 = ReservationState.입금대기;
                }
                else if (textBody.Contains("취소"))
                {
                    예약상태 = ReservationState.취소;
                }
                else
                {
                    throw new NotReservationMailException();
                }

                var textBodyToArray = textBody.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();
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
                    switch (제목)
                    {
                        case "결제금액":
                            결제금액raw = 내용;
                            var 결제금액분리 = 결제금액raw.Split('=');
                            var 결제금액string = 결제금액분리[1].Replace(",", string.Empty)
                                .Replace("원", string.Empty)
                                .Trim();
                            결제금액 = int.Parse(결제금액string);
                            break;
                        default:
                            break;
                    }
                }
                if (IsInRange(결제금액, 0, 5000))
                {
                    if (예약상태 is ReservationState.입금대기)
                    {
                        예약상태 = ReservationState.소액테스트입금대기;
                    }
                    else if (예약상태 is ReservationState.확정)
                    {
                        예약상태 = ReservationState.소액테스트확정;
                    }
                }

            }
            catch (NotReservationMailException)
            {
                예약상태 = ReservationState.예약메일아님;
            }
            catch (Exception e)
            {
                예약상태 = ReservationState.분석중오류발생;
            }
        }
        public bool IsInRange(int price, int min, int max)
        {
            if (min <= price && price <= max)
            {
                return true;
            }
            return false;
        }
    }
}

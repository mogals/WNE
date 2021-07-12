using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

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
        public PastReservation(string partnerCenterMessage, RichTextBox richTextBox, int index)
        {
            richTextBox.AppendText($"\n\n@ 지난 예약 텍스트 추출 ({index + 1}) @");
            richTextBox.AppendText($"\n{partnerCenterMessage}");
            richTextBox.AppendText($"\n\n@ 지난 예약 분석 중 ({index+1}) @");
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

                var textBodyToArray = text.Replace("예약번호", "예약번호 ")
                                          .Replace($"확정{Environment.NewLine}", "확정 ")
                                          .Replace($"취소{Environment.NewLine}", "취소 ")
                                          .Replace($"완료{Environment.NewLine}", "완료 ")
                                          .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();
                richTextBox.AppendText($"\n예약상태 : {예약상태.ToString()}");
                foreach (var item in textBodyToArray)
                {
                    string 제목 = string.Empty;
                    string 내용 = string.Empty;
                    foreach (var jtem in item)
                    {
                        if (char.IsWhiteSpace(jtem))
                        {
                            var jndex = item.IndexOf(jtem);
                            제목 = item.Substring(0, jndex).Trim();
                            내용 = item.Substring(jndex + 1, item.Length - jndex - 1).Trim();
                            break;
                        }
                    }

                    // 추출을 하는 과정은 프로퍼티에 집어 넣는 것이 보기 좋을 듯
                    switch (제목)
                    {
                        case "예약번호":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            예약번호네이버 = 내용;
                            break;
                        case "확정":
                        case "취소":
                        case "완료":
                            richTextBox.AppendText($"\n{제목} : {내용}");
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
                                richTextBox.AppendText($"\n객실정보에서 객실 인원수 추출 불가 => 0으로 처리");
                            }
                            richTextBox.AppendText($"\n객실 추출 : {객실}");
                            richTextBox.AppendText($"\n객실 인원수 추출 : {인원수}");
                            break;
                        case "이용기간":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            이용기간 = 내용;
                            var 이용기간분리 = 이용기간.Replace(" ", string.Empty)
                                                     .Replace(".(", "~")
                                                     .Replace("(", "~")
                                                     .Replace(")", "~")
                                                     .Split('~', StringSplitOptions.RemoveEmptyEntries);
                            이용시작일시 = DateTime.ParseExact(이용기간분리[0], "yyyy.M.d", null);
                            이용종료일시 = DateTime.ParseExact(이용기간분리[2], "yyyy.M.d", null);
                            이용일수 = (이용종료일시.Value - 이용시작일시.Value).Days;
                            richTextBox.AppendText($"\n이용시작일시 추출 : {이용시작일시}");
                            richTextBox.AppendText($"\n이용종료일시 추출 : {이용종료일시}");
                            richTextBox.AppendText($"\n이용일수 추출 : {이용일수}박 {이용일수+1}일");
                            break;
                        case "수량":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            수량 = int.Parse(내용);
                            break;
                        case "요청사항":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            요청사항 = 내용.Trim();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (NotReservationMailException)
            {
                richTextBox.AppendText($"\n{partnerCenterMessage}");
                richTextBox.AppendText($"\n파트너센터 지난 예약 조회 중 형식에 맞지 않는 자료 발견");
                예약상태 = ReservationState.예약메일아님;
            }
            catch (FormatException e)
            {
                richTextBox.AppendText($"\n{e}");
            }
            catch (Exception e)
            {
                richTextBox.AppendText($"\n{e}");
                richTextBox.AppendText($"\n파트너센터 지난 예약 자료 조회 중 예상치 못 한 예외 발생 => 개발자에게 문의해 주세요.");
                richTextBox.AppendText($"예외발생 예약 : {partnerCenterMessage}");
            }
        }
    }
}

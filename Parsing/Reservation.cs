using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Xml;

namespace WNE.Parsing
{
    public class Reservation
    {
        public bool 떠나요처리여부;
        public DateTime 메일수신일시;
        public bool 테스트메일인가;
        public ReservationState 예약상태;
        public string 예약자명;  // 필수
        public DateTime? 예약신청일시;
        public DateTime? 예약취소일시;
        public string 예약번호네이버;
        public string 예약번호떠나요;
        public string 예약상품;
        private string 객실raw;
        public string 객실;  // 예약상품에서 추출
        public int 객실금액;
        public int 인원수;  // 예약상품에서 추출
        public int 수량;
        public string 옵션;
        public string 이용기간; // 메일에서는 "이용일시"
        public DateTime? 이용시작일시;
        public DateTime? 이용종료일시;
        public int? 이용일수;
        public string 결제수단;  // 포인트결제, 신용카드, 무통장입금
        public string 결제상태;  // 결제완료, 입금대기취소, 환불완료,     입금대기?
        public string 결제금액raw; // 예약상품도 문자열에 같이 들어가 있음
        public int 결제금액;
        public string 메모용결제금액;
        public string 메모용객실이용금액;
        public string 환불금액;
        public string 환불수수료raw;
        public string 환불수수료;
        public string 환불수수료율;
        public string 요청사항;
        public string 취소사유;  // 위아래로 구분됨
        public string 전화번호 = "010-0000-0000";  // 필수
        public string 전화번호식별번호;
        public string 전화번호앞자리;
        public string 전화번호뒷자리;
        public string 이메일;
        public string 예약유형;
        public string 유입경로 = string.Empty; // 유입경로가 공백인 상태로 보내지면 떠나요에서 admin으로 자동 설정됨.
        public string 유입경로떠나요기록인데실제내용은결제수단임;
        public string 떠나요메모사항;
        public string 엑셀방문횟수;
        public List<string> 메모용옵션들;
        public List<옵션> 옵션들 = new List<옵션>();
        public List<PastReservation> 지난예약들 = new List<PastReservation>();
        public string NPay주문;
        public string 예약자번호;

        public Reservation() { }
        public Reservation(string partnerCenterMessage, RichTextBox richTextBox)
        {
            richTextBox.AppendText($"\n@ 네이버 파트너센터 분석 중 @");
            try
            {
                var text = partnerCenterMessage?.Replace($"예약자 취소 사유{Environment.NewLine}", "예약자취소사유 ")
                                                .Replace("입금 대기", "입금대기");

                if (text.Contains("확정"))
                {
                    예약상태 = ReservationState.확정;
                }
                // 정확하게 "입금대기"가 맞는지 확인 필요 => 우선은 네이버메일로 처리함
                else if (text.Contains("입금대기"))
                {
                    예약상태 = ReservationState.입금대기;
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

                richTextBox.AppendText($"\n예약상태 {예약상태.ToString()}");
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
                        case "예약자":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            var 님제거성명 = 내용.Replace("님", string.Empty).Trim();
                            string 성명;
                            if (IsHangulOrSpace(님제거성명))
                            {
                                var 성 = 님제거성명[0];
                                var 명 = 님제거성명.Substring(1).Trim();
                                성명 = $"{성} {명}";
                                richTextBox.AppendText($"\n예약자명에서 성 띄어쓰기 : {성명}");
                            }
                            else
                            {
                                성명 = 님제거성명;
                            }
                            예약자명 = 성명;
                            richTextBox.AppendText($"\n님제거 예약자명 : {예약자명}");
                            break;
                        case nameof(예약자번호):
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            예약자번호 = 내용.Trim();
                            break;
                        case "전화번호":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            전화번호 = 내용;
                            var 전화번호분리 = 전화번호.Split("-");
                            전화번호식별번호 = 전화번호분리[0].Trim();
                            전화번호앞자리 = 전화번호분리[1].Trim();
                            전화번호뒷자리 = 전화번호분리[2].Trim();
                            richTextBox.AppendText($"\n전화번호 식별번호 : {전화번호식별번호}");
                            richTextBox.AppendText($"\n전화번호 앞자리 : {전화번호앞자리}");
                            richTextBox.AppendText($"\n전화번호 뒷자리 : {전화번호뒷자리}");
                            break;
                        case "예약번호":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            예약번호네이버 = 내용;
                            break;
                        case "예약유형":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            예약유형 = 내용;
                            break;
                        case "이메일":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            이메일 = 내용;
                            break;
                        case "객실":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            객실raw = 내용;
                            var 객실정리 = 객실raw.Replace(" ", string.Empty)
                                              .Replace("~", string.Empty)
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
                            var 이용기간분리 = 이용기간.Replace(".(", "~")
                                                     .Replace(")", "~")
                                                     .Replace(". ", ".")
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
                        case "옵션":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            옵션 = 내용;
                            // 이 부분은 다시 작성해야 함
                            break;
                        case "유입경로":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            유입경로 = 내용;
                            break;
                        case "결제상태":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            결제상태 = 내용;
                            break;
                        case "NPay주문":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            NPay주문 = 내용;
                            break;
                        case "결제수단":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            결제수단 = 내용;
                            break;
                        case "결제금액":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            결제금액raw = 내용;
                            var 결제금액string = 결제금액raw.Replace(",", string.Empty)
                                                        .Replace("원", string.Empty)
                                                        .Trim();
                            결제금액 = int.Parse(결제금액string);

                            메모용결제금액 = PriceTransform(결제금액string);

                            richTextBox.AppendText($"\n결제금액 추출 : {결제금액}");
                            richTextBox.AppendText($"\n메모용 결제금액 : {메모용결제금액}");
                            break;
                        case "환불금액":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            var 환불금액분리 = 내용.Split("(", StringSplitOptions.RemoveEmptyEntries);
                            환불금액 = 환불금액분리[0].Replace(",", string.Empty)
                                                    .Replace("원", string.Empty)
                                                    .Trim();
                            환불수수료율 = 환불금액분리[1].Replace(")", string.Empty)
                                                        .Replace("%", string.Empty)
                                                        .Trim();
                            richTextBox.AppendText($"\n환불금액 추출 : {환불금액}");
                            richTextBox.AppendText($"\n환불수수료율 추출 : {환불수수료율}");
                            break;
                        case "취소수수료":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            환불수수료 = 내용.Replace("원", string.Empty);
                            richTextBox.AppendText($"\n환불수수료 추출 : {환불수수료}");
                            break;
                        case "예약자취소사유":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            취소사유 = 내용;
                            break;
                        case "요청사항":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            요청사항 = 내용;
                            break;
                        default:
                            break;
                    }
                }
                if (IsInRange(결제금액, 0, 5000))
                {
                    예약상태 = ReservationState.소액테스트;
                }
            }
            catch (NotReservationMailException e)
            {
                richTextBox.AppendText($"\n{e}");
                richTextBox.AppendText($"\n파트너센터에서 자료 조회 중 예약 형식에 맞지 않는 자료 발견");
                richTextBox.AppendText($"\n예외발생 예약 텍스트 : {partnerCenterMessage}");
                예약상태 = ReservationState.예약메일아님;
            }
            catch (FormatException e)
            {
                richTextBox.AppendText($"\n{e}");
                richTextBox.AppendText($"\n파트너센터에서 자료 조회 중 예약 형식에 맞지 않는 자료 발견");
                richTextBox.AppendText($"\n예외발생 예약 텍스트 : {partnerCenterMessage}");
                예약상태 = ReservationState.예약메일아님;
            }
            catch (Exception e)
            {
                richTextBox.AppendText($"\n{e}");
                richTextBox.AppendText($"\n파트너센터에서 자료 조회 중 알 수 없는 오류 발생 => 오류메시지를 개발자에게 보내주세요.");
                richTextBox.AppendText($"\n예외발생 예약 텍스트 : {partnerCenterMessage}");
            }
        }
        public Reservation(MimeMessage mimeMessage, RichTextBox richTextBox, uint uniqueId)
        {
            richTextBox.AppendText($"\n################################################################");
            richTextBox.AppendText($"\n@ 메일 분석 중 @");
            richTextBox.AppendText($"\nUniqueId : {uniqueId}");
            richTextBox.AppendText($"\n{mimeMessage.Date} : {mimeMessage.Subject}");
            string textBody = string.Empty;
            try
            {
                textBody = mimeMessage.TextBody?.Replace("예약신청 일시", "예약신청일시")
                                                .Replace("예약취소 일시", "예약취소일시")
                                                .Replace("입금 대기", "입금대기");
                if (textBody.Contains("@@@"))
                {
                    테스트메일인가 = true;
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

                richTextBox.AppendText($"\n예약상태 {예약상태.ToString()}");
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

                    // 추출을 하는 과정은 프로퍼티에 집어 넣는 것이 보기 좋을 듯
                    switch (제목)
                    {
                        case "예약자명":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            var 님제거성명 = 내용.Replace("님", string.Empty).Trim();
                            string 성명;
                            if (IsHangulOrSpace(님제거성명))
                            {
                                var 성 = 님제거성명[0];
                                var 명 = 님제거성명.Substring(1).Trim();
                                성명 = $"{성} {명}";
                                richTextBox.AppendText($"\n예약자명에서 성 띄어쓰기 : {성명}");
                            }
                            else
                            {
                                성명 = 님제거성명;
                            }
                            예약자명 = 성명;
                            richTextBox.AppendText($"\n님제거 예약자명 : {예약자명}");
                            break;
                        case "예약신청일시":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            string 예약신청일시string = 내용;
                            예약신청일시 = DateTime.ParseExact(예약신청일시string, "yyyy.MM.dd. HH:mm:ss", null);
                            richTextBox.AppendText($"\n예약신청일시 추출 결과 : {예약신청일시}");
                            break;
                        case "예약취소일시":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            string 예약취소일시string = 내용.Replace(".", string.Empty);
                            예약취소일시 = DateTime.ParseExact(예약취소일시string, "yyyyMMdd HH:mm:ss", null);
                            richTextBox.AppendText($"\n이용취소일시 추출 결과 : {예약취소일시}");
                            break;
                        case "예약번호":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            예약번호네이버 = 내용.Split(" ")[0];
                            break;
                        case "예약상품":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            예약상품 = 내용;
                            var 예약상품정리 = 예약상품.Replace(" ", string.Empty)
                                                     .Replace("~", string.Empty)
                                                     .Replace("(스위트룸)", string.Empty)
                                                     // 몇 인실인지 안 나와 있는 경우도 있음!!
                                                     // 결제금액에서 읽어와야?
                                                     .Replace("인실", string.Empty)
                                                     .Replace("인", string.Empty);
                            var 예약상품분리 = 예약상품정리.Split("(");
                            객실 = 예약상품분리[0].Replace(" ", string.Empty).Trim();
                            try
                            {
                                인원수 = int.Parse(예약상품분리[1].Replace(")", string.Empty).Trim());
                            }
                            catch (IndexOutOfRangeException)
                            {
                                richTextBox.AppendText($"\n예약상품에서 객실 인원수 추출 불가 => 0으로 처리");
                            }
                            richTextBox.AppendText($"\n객실 추출 : {객실}");
                            richTextBox.AppendText($"\n객실 인원수 추출 : {인원수}");
                            break;
                        case "이용일시":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            이용기간 = 내용;
                            // 분리해야 할 일시 문자열 예시 ==> 이용일시 2021.05.27.(목)~2021.05.28.(금) (1박 2일)
                            var 이용일시분리 = 이용기간.Replace("(", "~")
                                                                      .Replace(")", "~")
                                                                      .Replace(".", string.Empty)
                                                                      .Split('~', StringSplitOptions.RemoveEmptyEntries);
                            이용시작일시 = DateTime.ParseExact(이용일시분리[0], "yyyyMMdd", null);
                            이용종료일시 = DateTime.ParseExact(이용일시분리[2], "yyyyMMdd", null);
                            이용일수 = (이용종료일시.Value - 이용시작일시.Value).Days;
                            richTextBox.AppendText($"\n이용시작일시 추출 : {이용시작일시}");
                            richTextBox.AppendText($"\n이용종료일시 추출: {이용종료일시}");
                            richTextBox.AppendText($"\n이용일수 추출 : {이용일수}박 {이용일수+1}일");
                            break;
                        case "결제상태":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            결제상태 = 내용;
                            break;
                        case "결제수단":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            결제수단 = 내용;
                            break;
                        case "결제금액":
                            // 예시
                            // 205호 (스위트룸) (2인)(1) 250,000원 + 바베큐 A 셋트(2) 80,000원 + 고기 추가 (B타입)(1) 22,000원 = 352,000원  
                            // 사과집(1) 280,000원 + 조식(방문식사)(4) 48,000원 = 328,000원
                            // 202호(2인실)(1) 160,000원 + 바베큐 A 셋트(2) 80,000원 = 240,000원
                            // 사과집(1) 380,000원 + 조식(딜리버리 서비스)(3) 49,500원 = 429,500원
                            // 사과집(1) 280,000원 + 바베큐 A 셋트(2) 80,000원 + 조식 (딜리버리 서비스)(2) 33,000원 + 고기 추가 (A타입)(1) 15,000원 + 고기 추가 (B타입)(1) 22,000원 + 쉐프조리 서비스(1) 0원 = 430,000원 
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            결제금액raw = 내용;
                            var 결제금액분리 = 결제금액raw.Split('=');


                            var 객실옵션분리 = 결제금액분리[0].Split("+");

                            // 객실요금 계산
                            var 객실및가격 = 객실옵션분리[0];
                            var 객실분리 = 객실및가격.Split(")");
                            var 객실분리된후몇개 = 객실분리.Length;
                            var 객실금액string = 객실분리[객실분리된후몇개 - 1].Replace("원", string.Empty)
                                                                            .Replace(",", string.Empty)
                                                                            .Trim();
                            객실금액 = int.Parse(객실금액string);
                            메모용객실이용금액 = PriceTransform(객실금액string);

                            // 옵션요금 계산
                            var 객실옵션개수 = 객실옵션분리.Length;
                            메모용옵션들 = new List<string>();
                            for (int i = 1; i <= 객실옵션개수 - 1; i++)
                            {
                                string 옵션string = 객실옵션분리[i].Trim();
                                string 옵션명 = string.Empty;
                                string 옵션금액 = string.Empty;
                                string 메모용옵션금액 = string.Empty;
                                string 옵션사람수 = string.Empty;
                                string 메모용옵션 = string.Empty;
                                string[] 옵션분리;

                                if (옵션string.Contains("바베큐"))
                                {
                                    옵션분리 = 옵션string.Replace("(", "~")
                                                      .Replace(")", "~")
                                                      .Split("~", StringSplitOptions.RemoveEmptyEntries);
                                    옵션명 = 옵션분리[0].Replace("셋트", string.Empty)
                                                       .Trim();
                                    옵션사람수 = 옵션분리[1].Trim();
                                    옵션금액 = 옵션분리[2].Replace(",", string.Empty)
                                                             .Replace("원", string.Empty)
                                                             .Trim();
                                    메모용옵션금액 = PriceTransform(옵션금액);
                                    메모용옵션 = $"{옵션명} {옵션사람수}명 {메모용옵션금액}";
                                }
                                else if (옵션string.Contains("조식"))
                                {
                                    옵션분리 = 옵션string.Replace("(방문식사)", "방문식사")
                                                      .Replace("(딜리버리 서비스)", "딜리버리 서비스")
                                                      .Replace("(", "~")
                                                      .Replace(")", "~")
                                                      .Replace("방문식사", "(방문식사)")
                                                      .Replace("딜리버리 서비스", "(딜리버리 서비스)")
                                                      .Split("~", StringSplitOptions.RemoveEmptyEntries);
                                    옵션명 = 옵션분리[0].Trim();
                                    옵션사람수 = 옵션분리[1].Trim();
                                    옵션금액 = 옵션분리[2].Replace(",", string.Empty)
                                                             .Replace("원", string.Empty)
                                                             .Trim();
                                    메모용옵션금액 = PriceTransform(옵션금액);
                                    메모용옵션 = $"{옵션명} {옵션사람수}명 {메모용옵션금액}";
                                }
                                else if (옵션string.Contains("고기"))
                                {
                                    옵션분리 = 옵션string.Replace("(A타입)", "A타입")
                                                      .Replace("(B타입)", "B타입")
                                                      .Replace("(", "~")
                                                      .Replace(")", "~")
                                                      .Split("~", StringSplitOptions.RemoveEmptyEntries);
                                    옵션명 = 옵션분리[0].Trim();
                                    옵션사람수 = 옵션분리[1].Trim();
                                    옵션금액 = 옵션분리[2].Replace(",", string.Empty)
                                                             .Replace("원", string.Empty)
                                                             .Trim();
                                    메모용옵션금액 = PriceTransform(옵션금액);
                                    메모용옵션 = $"{옵션명} {옵션사람수}개 {메모용옵션금액}";
                                }
                                else if (옵션string.Contains("쉐프"))
                                {
                                    옵션분리 = 옵션string.Replace("(", "~")
                                                      .Replace(")", "~")
                                                      .Split("~", StringSplitOptions.RemoveEmptyEntries);
                                    옵션명 = 옵션분리[0].Trim();
                                    옵션사람수 = 옵션분리[1].Trim();

                                    // 쉐프조리는 옵션금액이 무조건 0원으로 적혀서 메일로 날아옴.
                                    // 하지만, 1시간당 50,000원 기준으로 계산해서 떠나요에 기록해야 함
                                    메모용옵션금액 = string.Empty;
                                    int 옵션사람수int = int.Parse(옵션사람수);
                                    옵션금액 = (50000 * 옵션사람수int).ToString();
                                    if (옵션사람수int >= 2)
                                    {
                                        메모용옵션금액 = PriceTransform(옵션금액);
                                    }
                                    else if (옵션사람수int == 1)
                                    {
                                        메모용옵션금액 = "50,000원";  // 쉐프조리 가격은 미래에 가격이 상승할 수 있으므로 외부에서 입력받아야 함.
                                    }
                                    메모용옵션 = $"{옵션명} {옵션사람수}시간 {메모용옵션금액}";

                                }
                                옵션 옵션 = new 옵션()
                                {
                                    옵션명 = 옵션명,
                                    옵션총금액 = int.Parse(옵션금액),
                                    옵션사람수또는개수 = int.Parse(옵션사람수),
                                };
                                옵션.옵션단품금액 =(int)(옵션.옵션총금액 / 옵션.옵션사람수또는개수);
                                if (메모용옵션 is not null)
                                {
                                    메모용옵션들.Add(메모용옵션);
                                    옵션들.Add(옵션);
                                }
                            }

                            // 옵션재정렬
                            List<string> 옵션재정렬 = new List<string>();
                            메모용옵션들.ForEach(item =>
                            {
                                if (item.Contains("바베큐") && item.Contains("A"))
                                {
                                    옵션재정렬.Add(item);
                                }
                            });
                            메모용옵션들.ForEach(item =>
                            {
                                if (item.Contains("바베큐") && item.Contains("B"))
                                {
                                    옵션재정렬.Add(item);
                                }
                            });
                            메모용옵션들.ForEach(item =>
                            {
                                if (item.Contains("바베큐") && item.Contains("C"))
                                {
                                    옵션재정렬.Add(item);
                                }
                            });
                            메모용옵션들.ForEach(item =>
                            {
                                if (item.Contains("고기") && item.Contains("A"))
                                {
                                    옵션재정렬.Add(item);
                                }
                            });
                            메모용옵션들.ForEach(item =>
                            {
                                if (item.Contains("고기") && item.Contains("B"))
                                {
                                    옵션재정렬.Add(item);
                                }
                            });
                            메모용옵션들.ForEach(item =>
                            {
                                if (item.Contains("쉐프"))
                                {
                                    옵션재정렬.Add(item);
                                }
                            });
                            메모용옵션들.ForEach(item =>
                            {
                                if (item.Contains("조식") && item.Contains("방문"))
                                {
                                    옵션재정렬.Add(item);
                                }
                            });
                            메모용옵션들.ForEach(item =>
                            {
                                if (item.Contains("조식") && item.Contains("딜리버리"))
                                {
                                    옵션재정렬.Add(item);
                                }
                            });

                            메모용옵션들 = 옵션재정렬;

                            var 결제금액string = 결제금액분리[1].Replace(",", string.Empty)
                                                            .Replace("원", string.Empty)
                                                            .Trim();
                            결제금액 = int.Parse(결제금액string);
                            메모용결제금액 = PriceTransform(결제금액string);
                            richTextBox.AppendText($"\n결제금액 추출 : {결제금액}");
                            richTextBox.AppendText($"\n메모용 결제금액 : {메모용결제금액}");

                            break;
                        case "환불금액":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            환불금액 = 내용.Replace(",", string.Empty).Replace("원", string.Empty).Trim();
                            break;
                        case "환불수수료":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            var 환불수수료분리 = 내용.Replace("(", "~").Replace(" ", "~").Split("~", StringSplitOptions.RemoveEmptyEntries);
                            환불수수료 = 환불수수료분리[0].Replace("원", string.Empty).Trim();
                            환불수수료율 = 환불수수료분리[2].Replace("%", string.Empty).Trim();
                            richTextBox.AppendText($"\n환불수수료 추출 : {환불수수료}");
                            richTextBox.AppendText($"\n환불수수료율 추출 : {환불수수료율}");
                            break;
                        case "요청사항":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            요청사항 = 내용;
                            break;
                        case "취소사유":
                            richTextBox.AppendText($"\n{제목} : {내용}");
                            취소사유 = 내용.Replace("-", string.Empty);
                            break;
                        default:
                            break;
                    }
                }
                if (IsInRange(결제금액, 0, 5000))
                {
                    예약상태 = ReservationState.소액테스트;
                }
                유입경로떠나요기록인데실제내용은결제수단임 = GetFlowInSaleDomainHomepage(결제수단);
            }
            catch (NotReservationMailException)
            {
                richTextBox.AppendText($"\n예약메일 형식이 아님");
                예약상태 = ReservationState.예약메일아님;
            }
            catch (FormatException e)
            {
                richTextBox.AppendText($"\n예외발생 메일 : {mimeMessage?.Date}");
            }
            catch (Exception e)
            {
                richTextBox.AppendText($"\n{e}");
                richTextBox.AppendText($"\n알 수 없는 오류 발생 => 오류메시지를 개발자에게 보내주세요.");
                richTextBox.AppendText($"\n예외발생 메일 : {mimeMessage?.Date}");
            }
        }

        // 메일의 결제수단을 변형해서 떠나요의 유입경로에 기록
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

        public bool IsInRange(int price, int min, int max)
        {
            if (min <= price && price <= max)
            {
                return true;
            }
            return false;
        }
        public bool IsHangulOrSpace(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            Regex reg = new Regex(@"^[가-힣\s]+$");
            return reg.IsMatch(value);
        }


        public string PriceTransform(string priceString)
        {
            // 억대 이상 결제하는 경우는 없다고 가정
            priceString = priceString.Replace(",", string.Empty).Replace("원", string.Empty).Trim();

            var length = priceString.Length;
            string price앞자리 = string.Empty;
            string price뒷자리 = string.Empty;
            if (length > 4)
            {
                price앞자리 = priceString.Substring(0, length - 4);
                price뒷자리 = priceString.Substring(length - 4, 4);
            }
            else if (length <= 4)
            {
                price뒷자리 = priceString;
            }

            string priceTransformed = string.Empty;
            if (price앞자리 == string.Empty)
            {
                if (price뒷자리.Length == 4)
                {
                    price뒷자리 = price뒷자리.Insert(1, ",");
                }
                priceTransformed = $"{price뒷자리}원";
            }
            else if (price앞자리 != string.Empty && price뒷자리 == "0000")
            {
                switch (price앞자리.Length)
                {
                    case 4:  // 12,345,678
                        priceTransformed = price앞자리.Insert(1, ",") + "만원";
                        break;
                    case 5:  // 123,456,789
                        priceTransformed = price앞자리.Insert(2, ",") + "만원";
                        break;
                    default:
                        priceTransformed = price앞자리 + "만원";
                        break;
                }

                priceTransformed = $"{price앞자리}만원";
            }
            else if (price앞자리 != string.Empty && price뒷자리 != "0000")
            {
                switch (length)
                {
                    case 5:  // 12,345
                        priceTransformed = priceString.Insert(2, ",") + "원";
                        break;
                    case 6:  // 123,456
                        priceTransformed = priceString.Insert(3, ",") + "원";
                        break;
                    case 7:  // 1,234,567
                        priceTransformed = priceString.Insert(1, ",").Insert(4, ",") + "원";
                        break;
                    case 8:  // 12,345,678
                        priceTransformed = priceString.Insert(2, ",").Insert(5, ",") + "원";
                        break;
                    case 9:  // 123,456,789
                        priceTransformed = priceString.Insert(3, ",").Insert(6, ",") + "원";
                        break;
                    default:
                        break;
                }
            }

            return priceTransformed;
        }
    }

    public class 옵션
    {
        public string 옵션명;
        public int 옵션사람수또는개수;
        public int 옵션단품금액;
        public int 옵션총금액;
    }

    public class 옵션가격Info
    {
        public string 옵션명;
        public int 가격;
    }

    public class NotReservationMailException : Exception
    {
        public NotReservationMailException()
        {
        }
    }
}

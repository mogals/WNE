using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WNE.Parsing
{
    public class Setting
    {
        // 크롬
        public string ChromeProfileFolder { get; set; }

        // 지메일
        public string 지메일SslOptions {get; set;}
        public int 지메일ImapPort {get; set;} 
        public string 지메일Host {get; set;} 
        public string 지메일Username {get; set;} 
        public string 지메일Password {get; set;}

        // 네이버
        public string 네이버Username {get; set;} 
        public string 네이버Password {get; set;} 
        public string 네이버파트너센터고객정보RequestUrl {get; set;} 
        public string 네이버파트너센터고객정보XPathSelector {get; set;} 
        public string 네이버파트너센터고객정보CssSelector {get; set;} 
        public string 네이버파트너센터고객번호를구하기위한속성 {get; set;}
        public string 네이버파트너센터지난예약RequestUrl {get; set;} 
        public string 네이버파트너센터지난예약지난예약XpathSelector {get; set;}
        public string 네이버파트너센터지난예약지난예약CssSelector {get; set;} 
        public string 네이버파트너센터개인정보XPathSelector {get; set;}
        public string 네이버파트너센터개인정보CssSelector {get; set;} 
        public string 네이버파트너센터예약내역XPathSelector {get; set;}
        public string 네이버파트너센터예약내역CssSelector {get; set;}
        public string 네이버파트너센터결제정보XPathSelector {get; set;} 
        public string 네이버파트너센터결제정보CssSelector {get; set;}
        public string 네이버파트너센터툴팁XPathSelector {get; set;} 
        public string 네이버파트너센터툴팁CssSelector {get; set;}

        // 떠나요
        public string 떠나요Username { get; set; }
        public string 떠나요Password {get; set;} 
        public string 떠나요AuthLogin {get; set;} 
        public int 떠나요AccomodationId {get; set;} 
        public int 떠나요MaxStayDays {get; set;} 
        public string 떠나요LoginRequestUrl{get; set;} 
        public string 떠나요RegisterRequestUrl {get; set;}
        public string 떠나요CancelRequestUrl {get; set;}
        public string 떠나요DeleteRequestUrl {get; set;}
        public string 떠나요RetrieveRequestUrl {get; set;} 

        // Media 폴더
        public string 회사로고 {get; set;} 
        public string 메일도착알림음 {get; set;}
        public string 예약형식이아닌메일알림음 {get; set;} 
        public string 에약형식이아닌메일알림말 {get; set;} 
        public string 입금대기메일알림음 {get; set;}
        public string 입금대기메일알림말 {get; set;} 
        public string 예약무시알림음 {get; set;} 
        public string 예약무시알림말 {get; set;} 
        public string 예약등록알림음 {get; set;} 
        public string 예약등록알림말 {get; set;} 
        public string 예약위소알림음 {get; set;} 
        public string 예약취소알림말 {get; set;} 
        public string 오류발생알림음 {get; set;} 
        public string 오류발생알림말 {get; set;} 
        public string 거래내역서등록알림음 {get; set;}
        public string 거래내역서등록알림말 {get; set;} 
        public int 알림음과알림말사이재생시간간격 {get; set;}

        // 기타
        public string 테스트예약표기문자 {get; set;}
        public List<string> 오류발생시알림메일 { get; set; }
        public int 얼마이하를소액결제로볼것인가 { get; set; }
    }
}

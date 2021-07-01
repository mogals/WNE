using MailKit.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WNE
{
    public class Info
    {
        public int beforeTime { get; set; }
        public string saveFolder { get; set; }

        // Mail
        public SecureSocketOptions SslOptions { get; set; }
        public int imapMailPort { get; set; }
        public string mailHost { get; set; }
        public string mailUsername { get; set; }
        public string mailPassword { get; set; }

        // Naver 예약파트너 센터
        public string naverUsername { get; set; }
        public string naverPassword { get; set; }

        // Ddnayo Account
        public string ddnayoUsername { get; set; }
        public string ddnayoPassword { get; set; }
        public string ddnayoAuthLogin { get; set; }
        public int ddnayoAccomodationId { get; set; }
        public int ddnayoMaxStayDays { get; set; } //  최대 15박 16일

        // Ddnayo REST Request Url
        public string loginRequestUrl { get; set; }
        public string registerRequestUrl { get; set; }
        public string cancelRequestUrl { get; set; }
        public string deleteRequestUrl { get; set; }
        public string retrieveRequestUrl { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WNE.Ddnayo
{
    class DdnayoInfo
    {
        public static string username = "hs빌";
        public static string password = "moire8824!";
        public static string authLogin = "false";
        public static int accomodationId = 6850;
        public static int maxStayDays = 15; //  최대 15박 16일
        public static string loginRequestUrl = "https://partner.ddnayo.com/security/login";
        public static string registerRequestUrl = "https://partner.ddnayo.com/pms-api/reservation/ready";
        public static string cancelRequestUrl = string.Empty;
        public static string deleteRequestUrl = string.Empty;
        public static string retrieveRequestUrl = $"https://partner.ddnayo.com/pms-api/accommodation/{accomodationId}/reservation/management-list";
    }
}

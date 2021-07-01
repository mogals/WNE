using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WNE.Ddnayo.Content
{
    //  https://partner.ddnayo.com/pms-api/accommodation/6850/reservation/management-list
    class ManagementListRequestContent
    {
        public string accomodationName;
        public bool? isAgent;
        public bool? isSearchAll;
        public bool? isStay;
        public int? page;
        public string paymentCode;
        public string searchDateTypeCode;
        public string searchEndedAt;
        public string searchStartedAt;
        public string searchText;
        public int? size;
        public string stateCode;
    }
}

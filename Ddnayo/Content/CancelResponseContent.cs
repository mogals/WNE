using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WNE.Ddnayo.Content
{
    // https://partner.ddnayo.com/pms-api/accommodation/6850/reservation/{떠나요예약번호}/cancel
    class CancelResponseContent
    {
        public bool? success;
        public bool? data;
        public string errorString;
        public bool? failed;
    }
}

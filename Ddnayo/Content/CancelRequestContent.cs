using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WNE.Ddnayo.Content
{
    // https://partner.ddnayo.com/pms-api/accommodation/6850/reservation/{떠나요예약번호}/cancel
    class CancelRequestContent
    {
        public int? cancelPrice;
        public string cancelReason;
		//0040 : 전액환불
		//0050 : 현금일부환불
		//0060 : 환불없음(아마도? 확인 필요!)
		//0070 : 환불대기(아마도? 확인 필요!)
        public string cancelTypeCode;
        public bool? sendMessageOwner;
        public bool? sendMessageUser;
    }
}

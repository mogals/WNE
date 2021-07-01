using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddnayo.Ddnayo.Content
{
    // https://partner.ddnayo.com/pms-api/accommodation/6850/reservation/user/update
    class UserUpdateRequestContent
    {
        public bool? agent;  // false 무슨 뜻?
        public string arrivalTime; //
        public string birthDate;
        public string carNo;
        public string description; // 고객요청사항
        public string email; //
        public string flowInSaleDomainHomepage; // 유입경로
        public bool? isRequestPickup; //
        public string mobilePhoneNo; //
        public string phoneNo;
        public string pickupCheckin;
        public string pickupCheckout;
        public int? reservationNo; //
        public string reservationUserName; //
        public int? userId;
    }
}

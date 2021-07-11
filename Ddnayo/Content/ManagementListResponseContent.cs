using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WNE.Ddnayo.Content
{
    //  https;//partner.ddnayo.com/pms-api/accommodation/6850/reservation/management-list
    public class ManagementListResponseContent
    {
        public bool? success;
        public ManagementListResponseData data;
        public string errorString;
        public bool? failed;
    }
    public class ManagementListResponseData
    {
        public int? page;
        public int? size;
        public int? count;
        public int? totalCount;
        public List<ManagementListResponseDataListItem> list;
        public int? sumTotalPrice;
        public int? sumSettlementPrice;
    }

    public class ManagementListResponseDataListItem
    {
        public int? accommodationId;
        public string accommodationName;
        public int? reservationNo;
        public string reservationUserName;
        public string mobilePhoneNo;
        public string useStartDate;
        public string useEndDate;
        public string reservedDate;
        public string createdAt;
        public string stateCode;
        public string stateName;
        public string paymentCode;
        public string paymentName;
        public int? totalPrice;
        public int? paidPrice;
        public string settlementPrice;
        public int? margin;
        public int? profit;
        public string chCode;
        public string chName;
        public bool? isAgent;
        public string settlementCode;
        public string roomsName;
        public string expireDate;
        public string canceledDate;
        public string canceledTypeCode;
        public string canceledTypeName;
        public bool? isSettlement;
        public string settlementDate;
        public bool? isInMobile;
        public string flowInSaleDomainHomepage;
        public bool? isRequestPickup;
        public string pickupCheckin;
        public string pickupCheckout;
        public string description;
        public string memo;
        public int? optionCount;
        public int? userReservationCount;
        public bool? autoAssign;
        public int? countOfReserved;
        public int? countOfAssigned;
        public bool? resortType;
    }
}

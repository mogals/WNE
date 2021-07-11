using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddnayo.Ddnayo.Content
{
    // https://partner.ddnayo.com/pms-api/accommodation/6850/reservation/{주문번호}
    public class ReservationResponseContent
    {
        public bool? success;
        public ReservationResponseData data;
        public string errorString;
        public bool? failed;
    }

    public class ReservationResponseData
    {
        public int? vendorSettlement;
        public int? vendorId;
        public float? vendorCommissionRate;
        public int? userId;
        public int? userBizId;
        public string useStartDate;
        public bool? useSelfPg;
        public string useEndDate;
        public int? totalPrice;
        public string taxCode;
        public string stateCodeName;
        public float? settlementRate;
        public int? settlementPrice;
        public int? settlementFeeVat;
        public int? settlementFeeSupply;
        public float? settlementFeeRate;
        public int? settlementFee;
        public string settlementDate;
        public string settlementCodeName;
        public string settlementCode;
        public int? settlementAgentPrice;
        public string settlementAgentDate;
        public bool? settlement;
        public int? salePrice;
        public string saleDate;
        public string roomsName;
        public int? roomPrice;
        public string reservedDate;
        public string reservationUserName;
        public int? reservationNo;
        public int? reservationId;
        public int? reservationGoodsNo;
        public string requestCancelCodeName;
        public string requestCancelCode;
        public int? refundPrice;
        public int? profit;
        public string pickupCheckout;
        public string pickupChecki;
        public string phoneNo;
        public int? pgNo;
        public int? pgId;
        public string paymentCodeName;
        public string paymentCode;
        public int? paidPrice;
        public int? paidPoint;
        public int? paidExceedPrice;
        public string paidDate;
        public int? otherPrice;
        public string mobilePhoneNo;
        public string memo;
        public int? margin;
        public bool? isSocial;
        public bool? isSettlementAgent;
        public bool? isSendSmsAdminCheck;
        public bool? isRequestPickup;
        public bool? isNoTax;
        public bool? isInMobile;
        public bool? isHide;
        public bool? isGavePoint;
        public bool? isCheckedAdmin;
        public bool? isAgentFeeWithVat;
        public string ip;
        public int? givePoint;
        public string flowInSaleDomainHomepage;
        public string flowInSaleDomain;
        public string expireDate;
        public string email;
        public string description;
        public string createdAt;
        public int? commentCount;
        public float? channelCommissionRate;
        public string chName;
        public string chCodeName;
        public string chCode;
        public string carNo;
        public string canceledTypeCodeName;
        public string canceledTypeCode;
        public string canceledReason;
        public string canceledDate;
        public string birthDate;
        public string arrivalTime;
        public string agentMemo;
        public float? agentFeeRate;
        public int? agentFee;
        public bool? agent;
        public float? agencyCommissionRate;
        public int? accommodationUserId;
        public string accommodationTelNo;
        public string accommodationName;
        public int? accommodationId;

    }
}

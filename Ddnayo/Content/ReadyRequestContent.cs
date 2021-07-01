using System.Collections.Generic;

namespace WNE.Ddnayo.Content
{
        
    class ReadyRequestContent
    {
        public int accommodationId;
        public string arrivalTime;
        public string flowInSaleDomainHomepage; // 유입경로
        public bool isComplete;  // 결제완료
        public bool isSmsAccountToReservedUser;
        public bool isSmsToAdministrator;
        public bool isSmsToReservedUser;
        public string managerMemo;
        public List<string> others;  // ??
        public string pickupPos;
        public string pickupTime;
        public List<Room> roomsInfo;
        public int totalPaidPrice;
        public string userName;
        public string userPhone;
    }
    class Room
    {
        public string checkInDate;
        public int? numOfAdult;
        public int? numOfBaby;
        public int? numOfChild;
        public int numOfRoom;  // 무조건 1로 설정하면 됨(객실 예약당 방을 몇 개 예약했는가를 뜻하는 것 같음)
        public int roomId;
        public int stayDays;
    }



}


using Ddnayo.Ddnayo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WNE.Parsing;

namespace NE.Naver
{
    class PartnerCenterInfo
    {
        public List<RoomInfo> rooms = new List<RoomInfo>();
        public PartnerCenterInfo()
        {
            rooms.Add(new RoomInfo()
            {
                roomId = 2912232,
                객실 = "201호",
                adultOccupancy = 4
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912258,
                객실 = "202호",
                adultOccupancy = 2
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912269,
                객실 = "203호",
                adultOccupancy = 2
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912272,
                객실 = "204호",
                adultOccupancy = 2
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912273,
                객실 = "205호",
                adultOccupancy = 2
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912287,
                객실 = "206호",
                adultOccupancy = 2
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912292,
                객실 = "301호",
                adultOccupancy = 4
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912296,
                객실 = "302호",
                adultOccupancy = 2
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912297,
                객실 = "303호",
                adultOccupancy = 2
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912305,
                객실 = "F2",
                adultOccupancy = 10
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912308,
                객실 = "F3",
                adultOccupancy = 8
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912314,
                객실 = "라르고료칸",
                adultOccupancy = 18
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912319,
                객실 = "황토집",
                adultOccupancy = 10
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912326,
                객실 = "풀하우스",
                adultOccupancy = 14
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912330,
                객실 = "사과집",
                adultOccupancy = 4
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912337,
                객실 = "스위스빌라",
                adultOccupancy = 4
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912340,
                객실 = "디톡스",
                adultOccupancy = 12
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 2912346,
                객실 = "강가의집",
                adultOccupancy = 10
            });
        }
    }
}

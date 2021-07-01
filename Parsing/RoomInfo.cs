using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WNE.Parsing { 
    class RoomInfo
    {
        public int roomId;
        public string 객실;
        public int adultOccupancy;
        public int babyOccupancy = 0;
        public int childOccupancy = 0;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WNE.Ddnayo.Content
{
    class ReadyResponseContent
    {
        public bool? success;
        public ReadyResponseData data;
        public string errorString;
        public bool? failed;
    }

    public class ReadyResponseData
    {
        public int? reservationNo;
        public bool? isSuccess;
        public string errorMessage;
        public ReadyResponseDataErrorCode? errorcode;
    }

    public enum ReadyResponseDataErrorCode
    {
        에러0, 에러1, 에러2, 에러3, 예약불가
    }
}

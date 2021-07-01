using Ddnayo.Ddnayo;
using Ddnayo.Ddnayo.Content;
using WNE.Ddnayo.Content;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WNE.Parsing;

namespace WNE.Ddnayo
{
    class DdnayoClient
    {
        CookieContainer cookies;
        HttpClientHandler handler;
        public HttpClient httpClient;
        // DB에 저장?
        public List<RoomInfo> rooms = new List<RoomInfo>();

        public DdnayoClient()
        {
            rooms.Add(new RoomInfo()
            {
                roomId = 90420,
                객실 = "201호"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 90421,
                객실 = "202호"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 90422,
                객실 = "203호"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 90423,
                객실 = "204호"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 90424,
                객실 = "205호"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 90425,
                객실 = "206호"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 90426,
                객실 = "301호"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 90427,
                객실 = "302호"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 90428,
                객실 = "303호"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 80532,
                객실 = "F2"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 80533,
                객실 = "F3"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 80534,
                객실 = "라르고료칸"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 80535,
                객실 = "황토집"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 80536,
                객실 = "풀하우스"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 80537,
                객실 = "사과집"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 80538,
                객실 = "스위스빌라"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 80539,
                객실 = "디톡스"
            });
            rooms.Add(new RoomInfo()
            {
                roomId = 80540,
                객실 = "강가의집"
            });
        }

        public async Task Login()
        {
            cookies = new CookieContainer();
            handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            httpClient = new HttpClient(handler);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, DdnayoInfo.loginRequestUrl);
            requestMessage.Headers.UserAgent
                                  .ParseAdd(" Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36");

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
                                {
                                    {"id", DdnayoInfo.username},
                                    {"password",DdnayoInfo.password},
                                    {"authLogin", DdnayoInfo.authLogin}
                                });
            requestMessage.Content = content;
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            // response 결과 분석 => accomodationId 추출해야함.



            //var uri = new Uri("https://partner.ddnayo.com/dashboard?accommodationId=6850"); 
            //var responseCookies = cookies.GetCookies(uri).Cast<Cookie>();
            //foreach (Cookie cookie in responseCookies)
            //{
            //    Console.WriteLine(cookie.Name + ": " + cookie.Value);
            //}

            Console.WriteLine("떠나요 로그인함");
        }

        public async Task Ready(Reservation reservationFromMail, Reservation reservationFromPartnerCenter)
        {
            try
            {
                await Login();
                await Task.Delay(1000);
                ReadyRequestContent readyRequestContent = new ReadyRequestContent
                {

                    accommodationId = DdnayoInfo.accomodationId,
                    arrivalTime = string.Empty,
                    flowInSaleDomainHomepage = GetFlowInSaleDomainHomepage(reservationFromMail.결제수단),
                    isComplete = true,
                    isSmsAccountToReservedUser = false,
                    isSmsToAdministrator = reservationFromMail.테스트메일인가 ? false : true,
                    isSmsToReservedUser = reservationFromMail.테스트메일인가 ? false : true,
                    managerMemo = GetManagerMemo(reservationFromMail, reservationFromPartnerCenter),
                    others = new List<string>(),
                    pickupPos = string.Empty,
                    pickupTime = string.Empty,
                    roomsInfo = new List<Room>(),
                    userName = reservationFromMail.테스트메일인가? "@@@"+reservationFromPartnerCenter.예약자명 : reservationFromPartnerCenter.예약자명,
                    userPhone = reservationFromMail.테스트메일인가 ? "010-0000-0000" : reservationFromPartnerCenter.전화번호,
                    totalPaidPrice = reservationFromMail.결제금액
                };
                int? roomId = rooms.Where(room => room.객실.Equals(reservationFromMail.객실)).FirstOrDefault()?.roomId;
                if (!roomId.HasValue)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("메일에서 읽어들인 객실명과 떠나요의 객실명이 일치하지 않음");
                    Console.WriteLine($"메일에서 읽어들인 객실명 : {reservationFromMail.객실}");
                    Console.WriteLine($"{reservationFromMail.예약자명}님");
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                var room = new Room
                {
                    checkInDate = reservationFromMail.테스트메일인가 ? reservationFromMail.이용시작일시.Value.AddYears(1).ToString("yyyy-MM-dd")
                                                                    : reservationFromMail.이용시작일시.Value.ToString("yyyy-MM-dd"),
                    numOfAdult = reservationFromMail.인원수, // 방의 수용가능 인원수를 어른수에 대입, 떠나요에서 읽어와서 대입하는 방식으로 바꾸는 게 좋을 듯!
                    numOfBaby = 0,
                    numOfChild = 0,
                    numOfRoom = 1,
                    roomId = roomId.Value,
                    stayDays = reservationFromMail.이용일수.Value
                };
                readyRequestContent.roomsInfo.Add(room);

                string jsonStringContent = JsonConvert.SerializeObject(readyRequestContent);
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, DdnayoInfo.registerRequestUrl);
                StringContent stringContent = new StringContent(jsonStringContent, Encoding.UTF8, "application/json");
                requestMessage.Content = stringContent;
                requestMessage.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36");

                HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
                string responseBody = await response.Content.ReadAsStringAsync();

                // RESP API 예외처리 및 재시도 패턴 구현 필요함!

                ReadyResponseContent jsonResponse = JsonConvert.DeserializeObject<ReadyResponseContent>(responseBody);

                var success = jsonResponse.success.Value;
                var data = jsonResponse.data;
                var errorString = jsonResponse.errorString;
                var failed = jsonResponse.failed.Value;
                if (success)  // 정상처리된 응답
                {
                    if (data.isSuccess.Value)
                    {
                        Console.WriteLine($"등록성공 : 떠나요 예약번호 {data.reservationNo.Value}");
                        Console.WriteLine("1초대기");
                        await Task.Delay(1000);
                        var ddnayoReservation = await Reservation(data.reservationNo.Value.ToString());
                        await UserUpdate(ddnayoReservation, reservationFromMail, reservationFromPartnerCenter);
                    }
                    else
                    {
                        Console.WriteLine(data.errorcode.HasValue);
                        Console.WriteLine($"등록실패 : {data.errorMessage}");

                        // 각 상황별로 어떻게 할 지 나중에 결정
                        switch (data.errorcode)
                        {
                            case ReadyResponseDataErrorCode.에러0:
                                break;
                            case ReadyResponseDataErrorCode.에러1:
                                break;
                            case ReadyResponseDataErrorCode.에러2:
                                break;
                            case ReadyResponseDataErrorCode.에러3:
                                break;
                            case ReadyResponseDataErrorCode.예약불가:
                                break;
                            default:
                                break;
                        }

                    }
                }
                else  // 404, 503등 처리과정 중 오류 났을 때???
                {
                    Console.WriteLine("**");
                    Console.WriteLine(errorString);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("예약 등록과정 중 예외 발생");
                Console.WriteLine(e);
            }
        }


        public async Task<ManagementListResponseContent> ManagementList(Reservation reservationFromMail, Reservation reservationFromPartnerCenter, DateTime start, DateTime end, int size)
        {
            await Login();
            await Task.Delay(1000);

            ManagementListRequestContent managementListRequestContent = new ManagementListRequestContent
            {
                accomodationName = string.Empty,
                isAgent = null,
                isSearchAll = null,
                isStay = null,
                page = 1,
                paymentCode = null,
                searchDateTypeCode = "0000",
                searchEndedAt = end.ToString("yyyy-MM-dd"),
                searchStartedAt = start.ToString("yyyy-MM-dd"),



                searchText = reservationFromMail.테스트메일인가? "0000" : ( reservationFromPartnerCenter.전화번호뒷자리 ?? string.Empty) ,



                size = size,
                stateCode = null
            };

            Console.WriteLine($"전화번호 뒷자리 {(reservationFromMail.테스트메일인가 ? "0000" : reservationFromPartnerCenter.전화번호뒷자리 ?? string.Empty)}로 예약 검색 : ");

            string requestUrl = $"https://partner.ddnayo.com/pms-api/accommodation/6850/reservation/management-list";
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            string jsonStringContent = JsonConvert.SerializeObject(managementListRequestContent);
            StringContent stringContent = new StringContent(jsonStringContent, Encoding.UTF8, "application/json");
            requestMessage.Content = stringContent;
            requestMessage.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36");

            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            string responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseBody);

            ManagementListResponseContent jsonResponse = JsonConvert.DeserializeObject<ManagementListResponseContent>(responseBody);
            return jsonResponse;
        }

        public async Task<ReservationResponseContent> Reservation(string ddnayoReservationNo)
        {
            await Login();
            await Task.Delay(1000);

            string requestUrl = $"https://partner.ddnayo.com/pms-api/accommodation/6850/reservation/{ddnayoReservationNo}";
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            requestMessage.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36");

            Console.WriteLine("0.5초 정지 ##");
            await Task.Delay(500);

            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            string responseBody = await response.Content.ReadAsStringAsync();

            ReservationResponseContent jsonResponse = JsonConvert.DeserializeObject<ReservationResponseContent>(responseBody);

            var success = jsonResponse.success.Value;
            var data = jsonResponse.data;
            var errorString = jsonResponse.errorString;
            var failed = jsonResponse.failed.Value;
            if (success)  // 정상처리된 응답
            {
                Console.WriteLine($"예약정보 읽어 옮 : 떠나요 예약번호 {data.reservationNo.Value}");
                Console.WriteLine(responseBody);

            }
            else  // 404, 503등 처리과정 중 오류 났을 때?
            {
                Console.WriteLine("##");
                Console.WriteLine(errorString);
            }
            return jsonResponse;
        }

        public async Task Cancel(Reservation reservationFromMail, Reservation reservationFromPartnerCenter)
        {
            var managementList = await ManagementList(reservationFromMail, reservationFromPartnerCenter, DateTime.Now.AddMonths(-1), DateTime.Now, 50);

            await Login();
            await Task.Delay(1000);

            var success = managementList.success.Value;
            var errorString = managementList.errorString;
            var failed = managementList.failed.Value;
            if (success)  // 정상적으로 검색 결과 나옴
            {
                ManagementListResponseDataListItem 네이버예약과일치하는항목 = null;

                네이버예약과일치하는항목 = managementList.data.list.Where(item => item.description.Contains(reservationFromMail.예약번호네이버))
                                                                     .FirstOrDefault();
                if (네이버예약과일치하는항목 is null)
                {
                    네이버예약과일치하는항목 = managementList.data.list.Where(item => item.roomsName.Contains(reservationFromMail.객실))
                                                                     .Where(item => item.useStartDate.Equals(reservationFromMail.이용시작일시.Value.ToString("yyyy-MM-dd")))
                                                                     //.Where(item => item.totalPrice.Value.Equals(reservationFromPartnerCenter.결제금액))
                                                                     .FirstOrDefault();
                }
                if (네이버예약과일치하는항목 is null)
                {
                    Console.WriteLine("취소하고자 하는 예약을 발견하지 못 함");
                    Console.WriteLine("데이터 확인 또는 검색로직 수정 필요");
                    return;
                }

                // 실제 취소 과정
                CancelRequestContent cancelRequestContent = new CancelRequestContent()
                {
                    cancelPrice = 네이버예약과일치하는항목.totalPrice,
                    cancelReason = $"네이버 취소({reservationFromMail.취소사유})",
                    cancelTypeCode = "0040", // 전액환불
                    sendMessageOwner = reservationFromMail.테스트메일인가? false : true,
                    sendMessageUser = reservationFromMail.테스트메일인가? false : true,
                };

                string requestUrl = $"https://partner.ddnayo.com/pms-api/accommodation/6850/reservation/{네이버예약과일치하는항목.reservationNo}/cancel";
                Console.WriteLine("취소URL");
                Console.WriteLine(requestUrl);
                string jsonStringContent = JsonConvert.SerializeObject(cancelRequestContent);
                Console.WriteLine();
                Console.WriteLine("취소용 json출력");
                Console.WriteLine(jsonStringContent);

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                StringContent stringContent = new StringContent(jsonStringContent, Encoding.UTF8, "application/json");
                requestMessage.Content = stringContent;
                requestMessage.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36");


                HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonConvert.DeserializeObject<CancelResponseContent>(responseBody);
                if (jsonResponse.success.Value)
                {
                    Console.WriteLine("취소 성공");
                }
                else
                {
                    Console.WriteLine("취소 실패");
                    Console.WriteLine(jsonResponse.errorString);
                }
            }
            else  // 404, 503등 처리과정 중 오류 났을 때?
            {
                Console.WriteLine("취소 실패 ##");
                Console.WriteLine("전화번호 뒷자리 검색 결과 없음??");
                Console.WriteLine(errorString);
            }
        }
        public async Task UserUpdate(ReservationResponseContent ddnayoReservation, Reservation reservationFromMail, Reservation reservationFromPartnerCenter)
        {
            // 요청 처리
            UserUpdateRequestContent userUpdateRequestContent = new UserUpdateRequestContent()
            {
                agent = false,
                arrivalTime = ddnayoReservation.data.arrivalTime,
                birthDate = string.Empty,
                carNo = string.Empty,
                description = $"네이버 예약번호 : {reservationFromMail.예약번호네이버}{Environment.NewLine}" +
                              $"{(reservationFromMail.요청사항.Trim() is "-" ? string.Empty : reservationFromMail.요청사항)}",
                email = reservationFromPartnerCenter.이메일 ?? string.Empty,
                flowInSaleDomainHomepage = ddnayoReservation.data.flowInSaleDomainHomepage,
                isRequestPickup = false,
                mobilePhoneNo = ddnayoReservation.data.mobilePhoneNo,
                phoneNo = string.Empty,
                pickupCheckin = string.Empty,
                pickupCheckout = string.Empty,
                reservationNo = ddnayoReservation.data.reservationNo,
                reservationUserName = ddnayoReservation.data.reservationUserName,
                userId = ddnayoReservation.data.userId
            };

            string requestUrl = $"https://partner.ddnayo.com/pms-api/accommodation/6850/reservation/user/update";
            string jsonStringContent = JsonConvert.SerializeObject(userUpdateRequestContent);
            Console.WriteLine("업데이트용 json출력");
            Console.WriteLine(jsonStringContent);
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            StringContent stringContent = new StringContent(jsonStringContent, Encoding.UTF8, "application/json");
            requestMessage.Content = stringContent;
            requestMessage.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36");

            // RESP API 예외처리 및 재시도 패턴 구현 필요함!
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            Console.WriteLine("1초 정지");
            await Task.Delay(1000);
            // 응답 처리 => 정장적인 응답이 왔다고 가정
            string responseBody = await response.Content.ReadAsStringAsync();
            UserUpdateResponseContent jsonResponse = JsonConvert.DeserializeObject<UserUpdateResponseContent>(responseBody);

            var success = jsonResponse.success.Value;
            var data = jsonResponse.data;
            var errorString = jsonResponse.errorString;
            var failed = jsonResponse.failed.Value;
            if (success)  // 정상처리된 응답
            {
                Console.WriteLine("업데이트 성공");
            }
            else
            {
                Console.WriteLine("업데이트 실패");
                Console.WriteLine(errorString);
                Console.WriteLine("업데이트 실패 문자 출력 완료");
            }

            await Task.Delay(1000);
        }
        public async Task HideUpdate()
        {
            await Login();
            await Task.Delay(1000);
        }
        public async Task CancelPenalty()
        {
            await Login();
            await Task.Delay(1000);
        }
        public async Task PriceCalculator()
        {
            await Login();
            await Task.Delay(1000);
        }

        public string GetFlowInSaleDomainHomepage(string 결제수단)
        {
            switch (결제수단)
            {
                case "신용카드":
                    return "네이버 카드결제";
                    break;
                case "무통장입금":
                    return $"네이버 {결제수단}";
                    break;
                case "포인트결제":
                    return $"네이버 {결제수단}";
                    break;
                case "계좌이체":
                    return $"네이버 {결제수단}";
                    break;
                default:
                    return $"{결제수단} : 예상하지 못한 결제수단 발생";
                    break;
            }
        }
        private string GetManagerMemo(Reservation reservationFromMail, Reservation reservationFromPartnerCenter)
        {
            string managerMemo = $"{reservationFromMail.예약신청일시.Value.ToString("M월 d일")} {GetFlowInSaleDomainHomepage(reservationFromMail.결제수단)}{Environment.NewLine}" +
                              $"{reservationFromPartnerCenter.예약자명}{Environment.NewLine}" +
                              $"{reservationFromMail.이용시작일시.Value.ToString("M월 d일 dddd")} {reservationFromMail.객실} {reservationFromMail.이용일수}박{reservationFromMail.이용일수 + 1}일 {reservationFromMail.메모용객실이용금액}{Environment.NewLine}" +
                              $"{string.Join(Environment.NewLine, reservationFromMail.메모용옵션들)}";
            return managerMemo;
        }
    }
}

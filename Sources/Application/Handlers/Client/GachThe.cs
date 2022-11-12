using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Generic;
using NRO_Server.Main.Menu;
using NRO_Server.Application.Constants;
using NRO_Server.Application.IO;
using NRO_Server.Application.Main;
using NRO_Server.Application.Threading;
using NRO_Server.DatabaseManager;
using NRO_Server.DatabaseManager.Player;
using NRO_Server.Model.Template;
using Newtonsoft.Json;

namespace NRO_Server.Application.Handlers.Client
{
    public static class GachThe
    {
        public static async void SendCard(Model.Character.Character character, string loaiThe, long menhGia, string soSeri, string maPin)
        {
            string partnerId = "1119984661";
            string partnerKey = "15a2201f6d51cc98c4a2cd83a4efa387";
            string url = "https://card24h.com/chargingws/v2";
            // string url = "http://183.81.33.159/chuyentiepthe.php";

            var sign = ServerUtils.MD5Hash("" + partnerKey + maPin + soSeri);
            short npcId = (short)(character.ShopId - DataCache.SHOP_ID_NAPTHE);

            // Console.WriteLine("sign: " + sign);

            if (sign == "")
            {
                character.CharacterHandler.SendMessage(Service.ServerMessage("Đã có lỗi xảy ra, vui lòng thử lại"));
                return;
            }

            if (GachTheDB.IsAlreadyExist(sign))
            {
                character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Thông tin thẻ này đã có trong hệ thống con nhé."));
                return;
            }

            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var requestID = ServerUtils.RandomNumber(100000000, 999999999) +"_"+ character.Player.Username;
                var httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.Method = HttpMethod.Post;
                httpRequestMessage.RequestUri = new Uri(url);

                var parameters = new List<KeyValuePair<string,string>>();
                parameters.Add(new KeyValuePair<string,string>("telco",loaiThe));
                parameters.Add(new KeyValuePair<string,string>("code",maPin));
                parameters.Add(new KeyValuePair<string,string>("serial",soSeri));
                parameters.Add(new KeyValuePair<string,string>("amount",menhGia + string.Empty));
                parameters.Add(new KeyValuePair<string,string>("request_id", requestID));
                parameters.Add(new KeyValuePair<string,string>("partner_id",partnerId));
                parameters.Add(new KeyValuePair<string,string>("sign", sign));
                parameters.Add(new KeyValuePair<string,string>("command", "charging"));

                var content = new FormUrlEncodedContent(parameters);
                httpRequestMessage.Content = content;
                
                // string jsoncontent = "{\"telco\": \""+ loaiThe +"\", \"code\": \""+maPin+"\", \"serial\": \""+soSeri+"\", \"amount\": \""+menhGia+"\", \"request_id\": \"" + requestID +"\", \"partner_id\": \""+partnerId+"\", \"sign\": \""+sign+"\", \"command\": \"charging\"}";
                // var httpContent = new StringContent(jsoncontent, Encoding.UTF8, "application/json");
                // httpRequestMessage.Content = httpContent;

                // Console.WriteLine("content: " + httpContent);
                // Thực hiện Post
                var response = await httpClient.SendAsync(httpRequestMessage);

                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine("RES: " + responseContent.ToString());
                GachTheResponse jsonResult = JsonConvert.DeserializeObject<GachTheResponse>(responseContent);

                // Console.WriteLine("maPin : " + maPin + " | seri: " + soSeri + " jsonResult.status: " + jsonResult.status + " menhGia: " + menhGia  + " loaiThe + " + loaiThe + " request: " + requestID + " res: " + responseContent.ToString());
                // Console.WriteLine("jsonResult.status: " + jsonResult.status);
                if (jsonResult.status == 1) {
                    character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Con đã nạp thẻ thành công. Đợi một vài phút rồi hãy đến gặp Quý Ngài Santa để đổi vàng nhé."));
                }
                else if (jsonResult.status == 2) {
                    character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Con đã nạp thẻ thành công. Những sai mệnh giá, con sẽ bị trừ 50% tiền phí nhé. Đợi một vài phút rồi hãy đến gặp Quý Ngài Santa để đổi vàng nhé."));
                }
                else if (jsonResult.status == 3) {
                    character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Thẻ của con bị lỗi rồi."));
                    return;
                }
                else if (jsonResult.status == 4) {
                    character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Hệ thống đang bảo trì rồi con à."));
                    return;
                }
                else if (jsonResult.status == 99) {
                    character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Đã gửi thẻ thành công, con hãy đợi một lát để hệ thống xử lý nhé."));
                }
                else if (jsonResult.status == 100) {
                    character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Con đã nhập sai mã pin và seri, hãy kiểm tra và nhập đúng Seri ở trên và Mã thẻ ở dưới nhé."));
                }
                else {
                    character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, jsonResult.message));
                    return;
                }

                // Insert DB
                GachTheDB.Create(sign, requestID, loaiThe, soSeri, maPin, menhGia, jsonResult.status);
            }
            catch (Exception e)
            {
                character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Đã có lỗi xảy ra khi gạch thẻ, vui lòng thử lại sau"));
                Server.Gi().Logger.Error($"Error Gach The: {e.Message}\n{e.StackTrace}");
            }

            
        }
    }
}
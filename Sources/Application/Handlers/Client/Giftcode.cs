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
using NRO_Server.Model.Option;
using NRO_Server.Model.Template;
using NRO_Server.Model.Character;
using Newtonsoft.Json;

namespace NRO_Server.Application.Handlers.Client
{
    public static class Giftcode
    {
        public static void HandleUseGiftcode(Model.Character.Character character, string code)
        {
            var timeServer = ServerUtils.CurrentTimeMillis();
            if (character.Delay.UseGiftCode > timeServer)
            {
                var delay = (character.Delay.UseGiftCode - timeServer) / 1000;
                if (delay < 1)
                {
                    delay = 1;
                }

                character.CharacterHandler.SendMessage(Service.DialogMessage(string.Format(TextServer.gI().DELAY_SEC,
                        delay)));
                return;
            }
            // kiểm tra hạn gift code
            // kiểm tra đã dùng gift code chưa
            var codeType = GiftcodeDB.CheckCodeValidType(code);
            if (codeType == -1)
            {
                character.Delay.UseGiftCode = timeServer + 30000;
                character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, "Giftcode đã hết hạn hoặc hết lượt sử dụng."));
                return;
            }

            var isUsedThisCode = GiftcodeDB.CheckCharacterAlreadyUsedCode(code, character.Name, codeType);

            if (isUsedThisCode)
            {
                character.Delay.UseGiftCode = timeServer + 30000;
                character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, "Bạn đã dùng Giftcode này rồi."));
                return;
            }
            // Sử dụng gift code
            character.Delay.UseGiftCode = timeServer + 60000;
            UseCode(character, code, codeType);
            
        }

        private static void UseCode(Model.Character.Character character, string code, int codeType)
        {
            if (codeType == 0)//Tân thủ
            {
                // 20 thoỉ vàng
                // Tặng 10 thỏi vàng
                var thoivang = ItemCache.GetItemDefault((short)457);
                thoivang.Quantity = 10;
                character.CharacterHandler.AddItemToBag(true, thoivang, "Giftcode");
                // 10 vé quay
                character.PlusDiamondLock(50000);
                // 3678
                character.CharacterHandler.SendMessage(Service.SendBag(character));
                character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công 10 thỏi vàng, 50k ngọc khóa. Chúc bạn vui vẻ cùng Ngọc Rồng SUPER HERO"));
            } 
            else if (codeType == 1)//Tân thủ
            {
                character.PlusDiamond(100000);
                // 3678
                character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công 100k Ngọc"));
            }
            // else if (codeType == 1)
            // {
            //     // 10 thoỉ vàng
            //     var thoivang = ItemCache.GetItemDefault((short)457);
            //     thoivang.Quantity = 10;
            //     character.CharacterHandler.AddItemToBag(true, thoivang, "Giftcode");
            //     // 5 vé quay
            //     var vequayngocvang = ItemCache.GetItemDefault((short)821);
            //     vequayngocvang.Quantity = 5;
            //     character.CharacterHandler.AddItemToBag(true, vequayngocvang, "Giftcode");

            //     //1 vien 3s
            //     var nr3s = ItemCache.GetItemDefault((short)16);
            //     nr3s.Quantity = 1;
            //     character.CharacterHandler.AddItemToBag(true, nr3s, "Giftcode");

            //     character.CharacterHandler.SendMessage(Service.SendBag(character));
            //     character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, "Đã nhận thành công 10 thỏi vàng, 5 vé quay thượng đế, 1 ngọc rồng 3 sao. Chúc bạn chơi game vui vẻ tại Ngọc Rồng SUPER HERO"));
            // }
            // else if (codeType == 2)//SUPER HERO Bao tri code bảo trì reset tài khoản
            // {
            //     var charId = character.Id;
            //     if (charId > 14506)
            //     {
            //         character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, "Tài khoản của bạn không đủ điều kiện để nhận giftcode này."));
            //         return;
            //     }
            //     var random = ServerUtils.RandomNumber(1, 100);
            //     if (random >= 5)
            //     {
            //         // 20 thoỉ vàng
            //         var thoivang = ItemCache.GetItemDefault((short)457);
            //         thoivang.Quantity = 20;
            //         character.CharacterHandler.AddItemToBag(true, thoivang, "Giftcode");
            //         // 20 vé quay
            //         var vequayngocvang = ItemCache.GetItemDefault((short)821);
            //         vequayngocvang.Quantity = 20;
            //         character.CharacterHandler.AddItemToBag(true, vequayngocvang, "Giftcode");
            //         // 5 viên 3 sao
            //         var nr3s = ItemCache.GetItemDefault((short)16);
            //         nr3s.Quantity = 5;
            //         character.CharacterHandler.AddItemToBag(true, nr3s, "Giftcode");
            //         // 3678
            //         character.CharacterHandler.SendMessage(Service.SendBag(character));
            //         character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công 20 thỏi vàng, 20 vé quay thượng đế, 5 ngọc rồng 3 sao."));
            //     }
            //     else 
            //     {
            //         if (character.InfoChar.ThoiGianTrungMaBu > 0)
            //         {
            //             character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DA_CO_TRUNG_MABU));
            //             return;
            //         }
            //         character.InfoChar.ThoiGianTrungMaBu = (DataCache.TRUNG_MA_BU_TIME + ServerUtils.CurrentTimeMillis());
            //         // Mảnh vỡ
            //         var itemManhVoBongTai = character.CharacterHandler.GetItemBagById(933);
            //         if (itemManhVoBongTai != null) 
            //         {
            //             var soLuongManhVoBongTaiHT = itemManhVoBongTai.Options.FirstOrDefault(opt => opt.Id == 31); //Số lượng bông tai
            //             if (soLuongManhVoBongTaiHT != null)
            //             {
            //                 soLuongManhVoBongTaiHT.Param += 500;
            //             }
            //         }
            //         else 
            //         {
            //             var manhVo = ItemCache.GetItemDefault((short)933);
            //             manhVo.Quantity = 1;
            //             var soLuongManhVoBongTaiHT = manhVo.Options.FirstOrDefault(opt => opt.Id == 31); //Số lượng bông tai
            //             soLuongManhVoBongTaiHT.Param = 500;
            //             character.CharacterHandler.AddItemToBag(true, manhVo, "Giftcode");
            //         }
            //         character.CharacterHandler.SendMessage(Service.SendBag(character));
            //         character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn đã nhận được một trứng đệ Ma bư và 500 mảnh vỡ bông tai"));
            //     }
                
            // }
            // else if (codeType == 3)
            // {
            //     // 20 thoỉ vàng
            //     var thoivang = ItemCache.GetItemDefault((short)457);
            //     thoivang.Quantity = 20;
            //     character.CharacterHandler.AddItemToBag(true, thoivang, "Giftcode");
            //     // 20 vé quay
            //     var vequayngocvang = ItemCache.GetItemDefault((short)821);
            //     vequayngocvang.Quantity = 20;
            //     character.CharacterHandler.AddItemToBag(true, vequayngocvang, "Giftcode");
            //     // 3678
            //     character.CharacterHandler.SendMessage(Service.SendBag(character));
            //     character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công 20 thỏi vàng, 20 vé quay thượng đế."));
            // }
            // else if (codeType == 4)
            // {
            //     // 230 thoỉ vàng
            //     var userId = character.Player.Id;
            //     if (userId < 9122)
            //     {
            //         character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, "Tài khoản của bạn không đủ điều kiện để nhận giftcode này."));
            //         return;
            //     }
            //     var thoivang = ItemCache.GetItemDefault((short)457);
            //     thoivang.Quantity = 20;
            //     character.CharacterHandler.AddItemToBag(true, thoivang, "Giftcode");
            //     character.CharacterHandler.SendMessage(Service.SendBag(character));
            //     character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công 20 thỏi vàng."));
            // }
            // else if (codeType == 5)
            // {
            //     try
            //     {
            //         var userId = character.Player.Id;
            //         var soLuongThoiVang = 80;
            //         if (userId >= 9122)
            //         {
            //             character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, "Tài khoản của bạn không đủ điều kiện để nhận giftcode này."));
            //             return;
            //         }

            //         if (character.Disciple == null && !DiscipleDB.IsAlreadyExist(-character.Id))
            //         {
            //             var disciple = new Disciple();
            //             disciple.CreateNewDisciple(character);
            //             disciple.Player = character.Player;
            //             disciple.CharacterHandler.SetUpInfo();
            //             character.Disciple = disciple;
            //             character.InfoChar.IsHavePet = true;
            //             character.CharacterHandler.SendMessage(Service.Disciple(1, null));
            //             DiscipleDB.Create(disciple);
            //         }

            //         var thoivang = ItemCache.GetItemDefault((short)457);
            //         thoivang.Quantity = soLuongThoiVang;
            //         character.CharacterHandler.AddItemToBag(true, thoivang, "Giftcode");
            //         character.CharacterHandler.SendMessage(Service.SendBag(character));
            //         character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công "+soLuongThoiVang+" thỏi vàng."));
            //     }
            //     catch (Exception)
            //     {
                    
            //     }
            // }
            // else if (codeType == 6)
            // {
            //     try
            //     {
            //         // 20 thoỉ vàng
            //         var thoivang = ItemCache.GetItemDefault((short)457);
            //         thoivang.Quantity = 20;
            //         character.CharacterHandler.AddItemToBag(true, thoivang, "Giftcode");
            //         // 20 vé quay
            //         var vequayngocvang = ItemCache.GetItemDefault((short)821);
            //         vequayngocvang.Quantity = 20;
            //         character.CharacterHandler.AddItemToBag(true, vequayngocvang, "Giftcode");
            //         // 10 viên 3 sao
            //         var nr3s = ItemCache.GetItemDefault((short)16);
            //         nr3s.Quantity = 10;
            //         character.CharacterHandler.AddItemToBag(true, nr3s, "Giftcode");
            //         // 3678
            //         character.CharacterHandler.SendMessage(Service.SendBag(character));
            //         character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công 20 thỏi vàng, 20 vé quay thượng đế, 10 ngọc rồng 3 sao."));
            //     }
            //     catch (Exception)
            //     {
                    
            //     }
            // }
            // else if (codeType == 8)
            // {
            //     var thoivang = ItemCache.GetItemDefault((short)457);
            //     thoivang.Quantity = 10;
            //     character.CharacterHandler.AddItemToBag(true, thoivang, "Giftcode");
            //     // 10 viên 3 sao
            //     var nr3s = ItemCache.GetItemDefault((short)16);
            //     nr3s.Quantity = 15;
            //     character.CharacterHandler.AddItemToBag(true, nr3s, "Giftcode");
            //     // 3678
            //     character.CharacterHandler.SendMessage(Service.SendBag(character));
            //     character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công 10 thỏi vàng, 15 ngọc rồng 3 sao."));
            // }
            // else if (codeType == 9)
            // {
            //     var thoivang = ItemCache.GetItemDefault((short)457);
            //     thoivang.Quantity = 20;
            //     character.CharacterHandler.AddItemToBag(true, thoivang, "Giftcode");
            //     // 3678
            //     character.CharacterHandler.SendMessage(Service.SendBag(character));
            //     character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công 20 thỏi vàng."));
            // }
            // else if (codeType == 10)
            // {
            //     var itemAdd = ItemCache.GetItemDefault(555);
            //     itemAdd.Options.Add(new OptionItem()
            //     {
            //         Id = 107,
            //         Param = 5,
            //     });
            //     character.CharacterHandler.AddItemToBag(false, itemAdd, "Giftcode");

            //     var itemadd2 = ItemCache.GetItemDefault(245);
            //     itemadd2.Options.Add(new OptionItem()
            //     {
            //         Id = 107,
            //         Param = 5,
            //     });
            //     character.CharacterHandler.AddItemToBag(false, itemadd2, "Giftcode");

            //     character.CharacterHandler.SendMessage(Service.SendBag(character));
            //     character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công lại vật phẩm."));
            // }
            // else if (codeType == 11)
            // {
            //     if (character.LengthBagNull() < 1)
            //     {
            //         character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG + "\nCần có 1 ô trống"));
            //         return;
            //     }
            //     if (!character.InfoChar.IsPremium)
            //     {
            //         character.CharacterHandler.SendMessage(
            //             Service.ServerMessage(TextServer.gI().NOT_PREMIUM));
            //         return;
            //     }
            //     var thoivang = ItemCache.GetItemDefault((short)457);
            //     thoivang.Quantity = 30;
            //     character.CharacterHandler.AddItemToBag(true, thoivang, "Giftcode");
            //     // 3678
            //     character.CharacterHandler.SendMessage(Service.SendBag(character));
            //     character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công 30 thỏi vàng từ Youtuber Duyên Hiệp Gaming, chúc bạn chơi game vui vẻ."));
            // }
            // else if (codeType == 12)
            // {
            //     if (character.LengthBagNull() < 2)
            //     {
            //         character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG + "\nCần có 2 ô trống"));
            //         return;
            //     }
            //     var thoivang = ItemCache.GetItemDefault((short)457);
            //     thoivang.Quantity = 4;
            //     character.CharacterHandler.AddItemToBag(true, thoivang, "Giftcode");
            //     // 10 viên 3 sao
            //     var dabaove = ItemCache.GetItemDefault((short)987);
            //     dabaove.Quantity = 9;
            //     character.CharacterHandler.AddItemToBag(true, dabaove, "Giftcode");

            //     // 3678
            //     character.CharacterHandler.SendMessage(Service.SendBag(character));
            //     character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công 4 thỏi vàng, 9 đá bảo vệ, chúc bạn chơi game và có một kỳ nghĩ lễ vui vẻ."));
            // }
            // else if (codeType == 14)
            // {
            //     if (character.LengthBagNull() < 2)
            //     {
            //         character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG + "\nCần có 2 ô trống"));
            //         return;
            //     }
            //     var thoivang = ItemCache.GetItemDefault((short)457);
            //     thoivang.Quantity = 10;
            //     character.CharacterHandler.AddItemToBag(true, thoivang, "Giftcode");
            //     // 10 viên 3 sao
            //     var dabaove = ItemCache.GetItemDefault((short)987);
            //     dabaove.Quantity = 5;
            //     character.CharacterHandler.AddItemToBag(true, dabaove, "Giftcode");

            //     var cstrungthu = ItemCache.GetItemDefault((short)737);
            //     cstrungthu.Quantity = 10;
            //     character.CharacterHandler.AddItemToBag(true, cstrungthu, "Giftcode");

            //     var hlt = ItemCache.GetItemDefault((short)1048);
            //     hlt.Quantity = 99;
            //     character.CharacterHandler.AddItemToBag(true, hlt, "Giftcode");

            //     // 3678
            //     character.CharacterHandler.SendMessage(Service.SendBag(character));
            //     character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công 10 thỏi vàng, 5 đá bảo vệ, 10 cs trung thu, 99 hồn linh thú."));
            // }

            // else if (codeType == 15)
            // {
            //     if (character.LengthBagNull() < 6)
            //     {
            //         character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG + "\nCần có 6 ô trống"));
            //         return;
            //     }
            //     var banhtrungthu = ItemCache.GetItemDefault((short)891);
            //     banhtrungthu.Quantity = 30;
            //     character.CharacterHandler.AddItemToBag(true, banhtrungthu, "Giftcode");

            //     var banhtrungthu1trung = ItemCache.GetItemDefault((short)465);
            //     banhtrungthu1trung.Quantity = 20;
            //     character.CharacterHandler.AddItemToBag(true, banhtrungthu1trung, "Giftcode");
            //     // 10 viên 3 sao
            //     var dabaove = ItemCache.GetItemDefault((short)987);
            //     dabaove.Quantity = 5;
            //     character.CharacterHandler.AddItemToBag(true, dabaove, "Giftcode");

            //     var cstrungthu = ItemCache.GetItemDefault((short)737);
            //     cstrungthu.Quantity = 20;
            //     character.CharacterHandler.AddItemToBag(true, cstrungthu, "Giftcode");

            //     var hlt = ItemCache.GetItemDefault((short)1048);
            //     hlt.Quantity = 99;
            //     character.CharacterHandler.AddItemToBag(true, hlt, "Giftcode");

            //     if (ServerUtils.RandomNumber(0, 100) <= 4)
            //     {
            //         var ntk = ItemCache.GetItemDefault((short)992);
            //         ntk.Quantity = 1;
            //         character.CharacterHandler.AddItemToBag(true, ntk, "Giftcode");
            //     }

            //     // 3678
            //     character.CharacterHandler.SendMessage(Service.SendBag(character));
            //     character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, @"Đã nhận thành công 30 bánh thập cẩm, 20 bánh 1 trứng, 5 đá bảo vệ, 20 cs trung thu, 99 hồn linh thú, có thể nhận nhẫn hãy mở hành trang kiểm tra."));
            // }
            GiftcodeDB.UsedCode(code, character.Name, codeType);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Linq.Extras;
using NRO_Server.Application.Constants;
using NRO_Server.Application.IO;
using NRO_Server.Application.Main;
using NRO_Server.Application.Manager;
using NRO_Server.Application.Threading;
using NRO_Server.Application.Handlers.Skill;
using NRO_Server.DatabaseManager;
using NRO_Server.DatabaseManager.Player;
using NRO_Server.Model.Character;
using NRO_Server.Model.Info;
using NRO_Server.Model.Info.Radar;
using NRO_Server.Model.Item;
using NRO_Server.Model.Option;
using NRO_Server.Model.SkillCharacter;
using NRO_Server.Model.Template;
using NRO_Server.Application.Helper;
using NRO_Server.Main.Menu;

namespace NRO_Server.Application.Handlers.Item
{
    public class ItemHandler
    {
        public static Model.Item.Item ConvertItem(ItemShop itemShop)
        {
            var item = new Model.Item.Item()
            {
                Id = itemShop.Id,
                SaleCoin = ItemCache.ItemTemplate(itemShop.Id).SaleCoin,
                BuyPotential = itemShop.Power,
                Quantity = itemShop.Quantity,
            };
            
            if (itemShop.Options.Count < 1)
            {
                item.Options.Add(new OptionItem()
                {
                    Id = 73,
                    Param = 0,
                }); 
            }
            else
            {
                item.Options = itemShop.Options.Copy();
            }
            
            return item;
        }
        
        public static Model.Item.Item Clone(Model.Item.Item item)
        {
            var itemClone = new Model.Item.Item()
            {
                Id = item.Id,
                SaleCoin = item.BuyCoin,
                BuyPotential = item.BuyPotential,
                Quantity = item.Quantity,
            };
            if (item.Options.Count < 1)
            {
                itemClone.Options.Add(new OptionItem()
                {
                    Id = 73,
                    Param = 0,
                }); 
            }
            else
            {
                itemClone.Options = item.Options.Copy();
            }
            
            return itemClone;
        }

        public static void BuyItem(Model.Character.Character character, int typeBuy, short id, short quantity = 1)
        {
            try
            {
                var shopTemplates = Cache.Gi().SHOP_TEMPLATES.FirstOrDefault(s => s.Key == character.ShopId).Value;
                if (character.ShopId is >= 7 and <= 11)
                {
                    ItemShop itemShop = null;
                    foreach (var shopTemplate in shopTemplates)
                    {
                        itemShop = shopTemplate.ItemShops.FirstOrDefault(item => item.Id == id);
                        if(itemShop != null) break;
                    }
                    if (itemShop == null) return;
                    if (itemShop.Power > character.InfoChar.Potential)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BUY_PPOINT));
                        character.CharacterHandler.SendMessage(Service.BuyItem(character));
                        return;
                    }
                    var itemTemplate = ItemCache.ItemTemplate(itemShop.Id);
                    if (itemTemplate.Require > character.InfoChar.Power)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_POWER));
                        character.CharacterHandler.SendMessage(Service.BuyItem(character));
                        return;
                    }

                    var itemNew = ConvertItem(itemShop);
                    var skillTemplate = UseSkill(character, itemNew);
                    if(skillTemplate == null) return;
                    var levelBook = itemTemplate.Level;
                    var timeStudy = "";
                    var timeLong = DataCache.TimeUseSkill[levelBook - 1];
                    timeStudy = levelBook switch
                    {
                        < 3 => ServerUtils.ConvertMilisecondToMinute(timeLong),
                        3 => ServerUtils.ConvertMilisecondToHour(timeLong),
                        _ => ServerUtils.ConvertMilisecondToDay(timeLong)
                    };
                    var money = ServerUtils.GetMoney((int)itemShop.Power);
                    character.InfoChar.LearnSkillTemp = new LearnSkill()
                    {
                        ItemSkill = itemNew,
                        Time = timeLong,
                        ItemTemplateSkillId = (short)skillTemplate.Id,
                    };
                    var menu = string.Format(TextServer.gI().DO_YOU_ADD_SKILL, skillTemplate.Name, money, timeStudy);
                    switch (character.TypeMenu)
                    {
                        case 0:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(13, menu, new List<string>() {"Đồng ý", "Từ chối"}, character.InfoChar.Gender));
                            character.TypeMenu = 2;
                            break;
                        }
                        case 1:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(14, menu, new List<string>() {"Đồng ý", "Từ chối"}, character.InfoChar.Gender));
                            character.TypeMenu = 1;
                            break;
                        }
                        case 2:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(15, menu, new List<string>() {"Đồng ý", "Từ chối"}, character.InfoChar.Gender));
                            character.TypeMenu = 1;
                            break;
                        }
                    }
                    
                }
                else if (character.ShopId == 1111)
                {
                    switch (typeBuy)
                    {
                        //Lấy item từ luckyBox
                        case 0:
                        {
                            if (character.LengthBagNull() <= 0)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                                return;
                            }

                            var itemcheck = character.CharacterHandler.GetItemLuckyBoxByIndex(id);
                            if(itemcheck == null) return;
                            
                            using(var itemclone = ItemHandler.Clone(itemcheck))
                            {
                                var luckyBox = character.LuckyBox;
                                var itemTemplate = ItemCache.ItemTemplate(itemclone.Id);
                                if (itemTemplate.Type == 9)
                                {
                                    var gold = itemclone.Options.FirstOrDefault(opt => opt.Id == 171);
                                    if (gold != null && gold.Param > 0)
                                    {
                                        character.PlusGold(gold.Param*1000);
                                        character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                                    }
                                }
                                else 
                                {
                                    character.CharacterHandler.AddItemToBag(true, itemclone, "Lấy từ lucky box");
                                    character.CharacterHandler.SendMessage(Service.SendBag(character));
                                }
                                
                                character.CharacterHandler.RemoveItemLuckyBox(id);
                                character.CharacterHandler.SendMessage(Service.SubBox(luckyBox));
                                character.ShopId = 1111;
                            }
                            break;
                        }
                        // Xóa Item LuckyBox
                        case 1:
                        {
                            var itemcheck = character.CharacterHandler.GetItemLuckyBoxByIndex(id);
                            if(itemcheck == null) return;
                            var luckyBox = character.LuckyBox;
                            character.CharacterHandler.RemoveItemLuckyBox(id);
                            character.CharacterHandler.SendMessage(Service.SubBox(luckyBox));
                            character.ShopId = 1111;
                            break;
                        }
                        case 2:
                        {
                            if (character.LengthBagNull() <= 0)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                                return;
                            }

                            foreach (var item in character.LuckyBox.ToList())
                            {
                                if(item == null) return;
                                using(var itemclone = ItemHandler.Clone(item))
                                {
                                    var itemTemplate = ItemCache.ItemTemplate(itemclone.Id);
                                    if (itemTemplate.Type == 9)
                                    {
                                        var gold = itemclone.Options.FirstOrDefault(opt => opt.Id == 171);
                                        if (gold != null && gold.Param > 0)
                                        {
                                            character.PlusGold(gold.Param*1000);
                                            character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                                        }
                                    }
                                    else 
                                    {
                                        if (character.LengthBagNull() <= 0)
                                        {
                                            character.CharacterHandler.SendMessage(Service.SubBox(character.LuckyBox));
                                            character.ShopId = 1111;
                                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                                            return;
                                        }
                                        character.CharacterHandler.AddItemToBag(true, itemclone, "Lấy từ lucky box");
                                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                                    }
                                    
                                    character.CharacterHandler.RemoveItemLuckyBox(item.IndexUI, false);
                                }
                            }
                            var luckyBox = character.LuckyBox;
                            character.CharacterHandler.SendMessage(Service.SubBox(luckyBox));
                            character.ShopId = 1111;
                            break;
                        }
                        //Ignore
                    }
                }
                else {
                    switch (character.TypeMenu)
                    {
                        case 0:
                        {
                            if (DataCache.IdDauThanx30.Contains(id))
                            {
                                id = (short)DataCache.IdDauThanx30[0];
                            }
                            ItemShop itemShop = null;
                            foreach (var shopTemplate in shopTemplates)
                            {
                                itemShop = shopTemplate.ItemShops.FirstOrDefault(item => item.Id == id);
                                if(itemShop != null) break;
                            }
                            if (itemShop != null)
                            {
                                if (character.ShopId - 3 == character.InfoChar.Gender)
                                {
                                    character.InfoChar.Hair = itemShop.HeadTemp;
                                    character.CharacterHandler.SendMessage(Service.ClosePanel());
                                    character.CharacterHandler.SendZoneMessage(Service.UpdateBody(character));
                                    character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn đã được cắt quả đầu mới siêu cấp Vip Pro"));
                                    return;
                                }

                                var itemTemplate = ItemCache.ItemTemplate(itemShop.Id);
                                if (itemTemplate.Require > character.InfoChar.Power)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_POWER));
                                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                    return;
                                }

                                var bGold = itemShop.BuyCoin;
                                var bDiamond = itemShop.BuyGold;
                                Model.Item.Item itemNew;
                                if (DataCache.IdDauThanx30.Contains(itemShop.Id))
                                {
                                    var levelMagic = MagicTreeManager.Get(character.Id).Level;
                                    if (levelMagic == 1) levelMagic = 2;
                                    var idNew = (short)DataCache.IdDauThanx30[levelMagic - 2];
                                    var index = DataCache.IdDauThanx30.IndexOf(idNew);
                                    itemNew = ItemCache.GetItemDefault((short)DataCache.IdDauThan[index]);
                                    itemTemplate = ItemCache.ItemTemplate(itemNew.Id);
                                    itemNew.Quantity = 30;
                                    if (index == 0) index = 1;
                                    bDiamond *= index;
                                }
                                else
                                {
                                    itemNew = ConvertItem(itemShop);
                                    if (itemShop.Id == 193) itemNew.Quantity = 10;
                                }

                                
                                switch (itemShop.Id)
                                {
                                    //Kỹ năng đệ tử 1
                                    case 402:
                                    {
                                        if (character.Disciple != null)
                                        {
                                            bDiamond *= character.Disciple.Skills[0].Point;
                                        }
                                        break;
                                    }
                                    //Kỹ năng đệ tử 2
                                    case 403:
                                    {
                                        if (character.Disciple != null)
                                        {
                                            if (character.Disciple.Skills.Count >= 2)
                                            {
                                                bDiamond *= character.Disciple.Skills[1].Point;
                                            }
                                        }
                                        break;
                                    }
                                    //Kỹ năng đệ tử 3
                                    case 404:
                                    {
                                        if (character.Disciple != null)
                                        {
                                            if (character.Disciple.Skills.Count >= 3)
                                            {
                                                bDiamond *= character.Disciple.Skills[2].Point;
                                            }
                                        }
                                        break;
                                    }
                                    //Kỹ năng đệ tử 4
                                    case 759:
                                    {
                                        if (character.Disciple != null)
                                        {
                                            if (character.Disciple.Skills.Count >= 4)
                                            {
                                                bDiamond *= character.Disciple.Skills[3].Point;
                                            }
                                        }
                                        break;
                                    }
                                    case 517:
                                    {
                                        if (character.PlusBag >= DataCache.MAX_PLUS_BAG)
                                        {
                                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().MAX_NUMBERS_BAG));
                                            character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                            return;
                                        }
                                        bGold *= (character.PlusBag + 1);
                                        break;
                                    }
                                    case 518:
                                    {
                                        if (character.PlusBox >= DataCache.MAX_PLUS_BAG)
                                        {
                                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().MAX_NUMBERS_BOX));
                                            character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                            return;
                                        }
                                        bGold *= (character.PlusBox + 1);
                                        break;
                                    }
                                    default:
                                    {
                                        if (character.ShopId > 2)
                                        {
                                            var itemCheck = character.ItemBag.FirstOrDefault(item =>
                                                item.Id == itemTemplate.Id && item.Quantity + itemNew.Quantity < 99);
                                            if ((!itemTemplate.IsUpToUp  || itemCheck == null) && character.LengthBagNull() < 1)
                                            {
                                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                                                character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                                return;
                                            }
                                        }
                                        break;
                                    }
                                }
                            
                                if (bGold > character.InfoChar.Gold)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                    return;
                                }
                            
                                if (bDiamond > character.AllDiamond())
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                    return;
                                }

                                switch (character.ShopId)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                    {
                                        var timePlus = (character.ShopId != 0
                                            ? character.ShopId != 1 ? DataCache._1MONTH : DataCache._8HOURS
                                            : DataCache._1HOUR);;
                                        if (character.InfoChar.ItemAmulet.ContainsKey(itemNew.Id))
                                        {
                                            if (character.InfoChar.ItemAmulet[itemNew.Id] < ServerUtils.CurrentTimeMillis())
                                            {
                                                character.InfoChar.ItemAmulet[itemNew.Id] = timePlus + ServerUtils.CurrentTimeMillis();
                                            }
                                            else 
                                            {
                                                character.InfoChar.ItemAmulet[itemNew.Id] += timePlus;
                                            }
                                        }
                                        else
                                        {
                                            character.InfoChar.ItemAmulet.TryAdd(itemNew.Id, timePlus + ServerUtils.CurrentTimeMillis());
                                        }
                                        // Setup bùa
                                        // character.CharacterHandler.SendMessage(Service.ClosePanel());
                                        character.CharacterHandler.SendMessage(Service.Shop(character, 0, character.ShopId));
                                        character.SetupAmulet();
                                        break;
                                    }
                                    default:
                                    {
                                        switch (itemShop.Id)
                                        {
                                            case 453:
                                            {
                                                character.InfoChar.Teleport = 3;
                                                break;
                                            }
                                            case 517:
                                            {
                                                character.PlusBag += 1;
                                                character.CharacterHandler.SendMessage(Service.SendBag(character));
                                                character.CharacterHandler.SendMessage(Service.ClosePanel());
                                                break;
                                            }
                                            case 518:
                                            {
                                                character.PlusBox += 1;
                                                character.CharacterHandler.SendMessage(Service.SendBox(character));
                                                character.CharacterHandler.SendMessage(Service.ClosePanel());
                                                break;
                                            }
                                            // Xử lý mua mảnh vỡ ở đây
                                            case 933:
                                            {
                                                // Kiểm tra trong túi có item chưa
                                                var itemManhVoBongTai = character.CharacterHandler.GetItemBagById(933);
                                                if (itemManhVoBongTai != null) 
                                                {
                                                    var soLuongManhVoBongTaiHT = itemManhVoBongTai.Options.FirstOrDefault(opt => opt.Id == 31); //Số lượng bông tai
                                                    var soLuongManhVoBongTaiCuaHang = itemNew.Options.FirstOrDefault(opt => opt.Id == 31);
                                                    if (soLuongManhVoBongTaiHT != null && soLuongManhVoBongTaiCuaHang != null)
                                                    {
                                                        soLuongManhVoBongTaiHT.Param += soLuongManhVoBongTaiCuaHang.Param;
                                                    }
                                                    else 
                                                    {
                                                        soLuongManhVoBongTaiHT.Param += 10;//default
                                                    }
                                                }
                                                else 
                                                {
                                                    if (!character.CharacterHandler.AddItemToBag(true, itemNew, "Mua mảnh vỡ từ cửa hàng")) return;
                                                }
                                                character.CharacterHandler.SendMessage(Service.SendBag(character));
                                                break;
                                            }
                                            // Xử lý mua tự động luyện tập
                                            case 521:
                                            {
                                                // Kiểm tra trong túi có item chưa
                                                var itemTuDongLuyenTap = character.CharacterHandler.GetItemBagById(521);
                                                if (itemTuDongLuyenTap != null) 
                                                {
                                                    var soLuongTuDongLuyenTap = itemTuDongLuyenTap.Options.FirstOrDefault(opt => opt.Id == 1); //Số lượng bông tai
                                                    var soLuongTDLTCuaHang = itemNew.Options.FirstOrDefault(opt => opt.Id == 1);
                                                    if (soLuongTuDongLuyenTap != null && soLuongTDLTCuaHang != null)
                                                    {
                                                        soLuongTuDongLuyenTap.Param += soLuongTDLTCuaHang.Param;
                                                    }
                                                    else 
                                                    {
                                                        soLuongTuDongLuyenTap.Param += 20;//default
                                                    }
                                                }
                                                else 
                                                {
                                                    if (!character.CharacterHandler.AddItemToBag(true, itemNew, "Mua TDTL từ cửa hàng")) return;
                                                }
                                                character.CharacterHandler.SendMessage(Service.SendBag(character));
                                                break;
                                            }
                                            // 
                                            default:
                                            {
                                                if (character.ShopId == 23)//trung thu 
                                                {
                                                    // HSD 15 ngay
                                                    var timeServerSecs = ServerUtils.CurrentTimeSecond();
                                                    var expireDay = 15;
                                                    var expireTime = timeServerSecs + (expireDay*86400);
                                                    itemNew.Options.Add(new OptionItem()
                                                    {
                                                        Id = 93,
                                                        Param = expireDay
                                                    });

                                                    var optionHiden = itemNew.Options.FirstOrDefault(option => option.Id == 73);
                                                    if (optionHiden != null) 
                                                    {
                                                        optionHiden.Param = expireTime;
                                                    }
                                                    else 
                                                    {
                                                        itemNew.Options.Add(new OptionItem()
                                                        {
                                                            Id = 73,
                                                            Param = expireTime,
                                                        });
                                                    }
                                                }

                                                if (!character.CharacterHandler.AddItemToBag(true, itemNew, "Mua từ cửa hàng")) return;
                                                character.CharacterHandler.SendMessage(Service.SendBag(character));
                                                if(itemTemplate.Type is 23 or 24) character.CharacterHandler.UpdateMountId();
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }

                                switch (typeBuy)
                                {
                                    case 0:
                                        character.MineGold(bGold);
                                        break;
                                    case 1:
                                        character.MineDiamond(bDiamond);
                                        break;
                                }
                                character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().ADD_ITEM, itemTemplate.Name)));
                                character.CharacterHandler.SendMessage(Service.BuyItem(character));
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().ERROR_SERVER));
                                character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                return;
                            }
                            break;
                        }
                        case 3: 
                        {
                            switch (character.ShopId)
                            {
                                case 21://Shop Bill
                                {
                                    var fullThucAn = character.ItemBag.FirstOrDefault(item => DataCache.ListThucAn.Contains(item.Id) && item.Quantity >= 99);
                                    if (fullThucAn == null)
                                    {
                                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_ITEM));
                                        character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                        return;
                                    }
                                    break;
                                }
                            }
                            // special icon id
                            
                            ItemShop itemShop = null;
                            foreach (var shopTemplate in shopTemplates)
                            {
                                itemShop = shopTemplate.ItemShops.FirstOrDefault(item => item.Id == id);
                                if(itemShop != null) break;
                            }
                            if (itemShop != null)
                            {
                                var itemTemplate = ItemCache.ItemTemplate(itemShop.Id);
                                if (itemTemplate.Require > character.InfoChar.Power)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_POWER));
                                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                    return;
                                }

                                var price = itemShop.BuyCoin;
                                var typePrice = itemShop.BuyGold;
                                var itemIdCheck = 0;
                                // kiểm tra loại
                                switch(typePrice)
                                {
                                    case 4028:
                                    {
                                        itemIdCheck = 457;
                                        break;
                                    }
                                }

                                var itemRemove = character.ItemBag.FirstOrDefault(item => item.Id == itemIdCheck && item.Quantity >= price);

                                if (itemRemove == null)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_ITEM));
                                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                    return;
                                }

                                Model.Item.Item itemNew = ConvertItem(itemShop);
                                
                                var itemCheck = character.ItemBag.FirstOrDefault(item =>
                                                item.Id == itemTemplate.Id && item.Quantity + itemNew.Quantity < 99);

                                if ((!itemTemplate.IsUpToUp  || itemCheck == null) && character.LengthBagNull() < 1)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                    return;
                                }

                                if (character.ShopId == 21)//bill tăng phần trăm chỉ số góc
                                {
                                    var randomRate = ServerUtils.RandomNumber(0.0, 100.0);
                                    var phanTramCongThem = 0;
                                    if (randomRate <= 0.8) //14-15%
                                    {
                                        //0.8%
                                        phanTramCongThem = ServerUtils.RandomNumber(14,15);
                                    }
                                    else if (randomRate <= 1.6) //13-14%
                                    {
                                        //0.8%
                                        phanTramCongThem = ServerUtils.RandomNumber(13,14);
                                    }
                                    else if (randomRate <= 3.2) //11-12%
                                    {
                                        // 1.6%
                                        phanTramCongThem = ServerUtils.RandomNumber(11,12);
                                    }
                                    else if (randomRate <= 5.4) //9-10%
                                    {
                                        //2.2%
                                        phanTramCongThem = ServerUtils.RandomNumber(9,10);
                                    }
                                    else if (randomRate <= 8.4) //7-8%
                                    {
                                        // 3%
                                        phanTramCongThem = ServerUtils.RandomNumber(7,8);
                                    }
                                    else if (randomRate <= 18.4) //5-6%
                                    {
                                        // 10%
                                        phanTramCongThem = ServerUtils.RandomNumber(5,6);
                                    }
                                    else if (randomRate <= 38.4) //3-4% 
                                    {
                                        //20%
                                        phanTramCongThem = ServerUtils.RandomNumber(3,4);
                                    }
                                    else if (randomRate <= 92.4) //1-2% 
                                    {
                                        // 44%
                                        phanTramCongThem = ServerUtils.RandomNumber(1,2);
                                    }
                                    itemNew.Options.Where(option => DataCache.IdOptionGoc.Contains(option.Id)).ToList().ForEach(
                                    option => {
                                        option.Param += option.Param*phanTramCongThem/100;
                                    });
                                }

                                if (!character.CharacterHandler.AddItemToBag(true, itemNew, "Mua từ bill")) return;
                                itemRemove = character.ItemBag.FirstOrDefault(item => item.Id == itemIdCheck && item.Quantity >= price);
                                character.CharacterHandler.RemoveItemBagByIndex(itemRemove.IndexUI, price, reason:"Mua đồ hủy diệt");


                                switch (character.ShopId)
                                {
                                    case 21://Shop Bill
                                    {
                                        var fullThucAn = character.ItemBag.FirstOrDefault(item => DataCache.ListThucAn.Contains(item.Id) && item.Quantity >= 99);
                                        if (fullThucAn != null)
                                        {
                                            character.CharacterHandler.RemoveItemBagByIndex(fullThucAn.IndexUI, fullThucAn.Quantity, reason:"Mua đồ hủy diệt");
                                        }
                                        break;
                                    }
                                }
                                character.CharacterHandler.SendMessage(Service.SendBag(character));
                                if(itemTemplate.Type is 23 or 24) character.CharacterHandler.UpdateMountId();
                                character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().ADD_ITEM, itemTemplate.Name)));
                                character.CharacterHandler.SendMessage(Service.BuyItem(character));
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().ERROR_SERVER));
                                character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                return;
                            }
                            break;
                        }
                        case 1: break;
                    }
                }
            }
            catch (Exception e)
            {
                
                Server.Gi().Logger.Error($"Error buy item in ItemHandler.cs: {e.Message} \n {e.StackTrace}", e);
                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().ERROR_SERVER));
                character.CharacterHandler.SendMessage(Service.BuyItem(character));

                UserDB.BanUser(character.Player.Id);
                ClientManager.Gi().KickSession(character.Player.Session);
                ServerUtils.WriteLog("hackmuado", $"Tên tài khoản {character.Player.Username} (ID:{character.Player.Id}) hack hackmuado");

                var temp = ClientManager.Gi().GetPlayer(character.Player.Id);
                if (temp != null)
                {
                    ClientManager.Gi().KickSession(temp.Session);
                }
            }
        }

        public static void SellItem(Model.Character.Character character, int action, int type, short index)
        {
            try
            {
                Server.Gi().Logger.Debug($"Sell Item -------------------------- action: {action} - type: {type} - index: {index}");
                switch (action)
                {
                    //Hỏi bán item
                    case 0:
                    {
                        Model.Item.Item itemSell = null;
                        switch (type)
                        {
                            //Cải trang
                            case 0 when index == 5:
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DO_NOT_SELL_ITEM));
                                return;
                            //Body
                            case 0:
                                itemSell = character.ItemBody[index];
                                break;
                            //Bag
                            case 1:
                                itemSell = character.CharacterHandler.GetItemBagByIndex(index);
                                break;
                        }
                        if(itemSell == null) return;
                        var template = ItemCache.ItemTemplate(itemSell.Id);
                        var gold = template.SaleCoin;
                        if (gold == -1 || template.Type == 5)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DO_NOT_SELL_ITEM));
                            return;
                        }

                        if (itemSell.Id == 457)
                        {
                            gold = DataCache.GiaBanThoiVang;
                        }

                        var quantity = 1;
                        if (itemSell.Quantity > 1 && !ItemCache.IsItemSellOnlyOne(itemSell.Id))
                        {
                            quantity = itemSell.Quantity;
                        }
                        gold *= quantity;
                        var info = string.Format(TextServer.gI().DO_YOU_WANT, quantity,
                            template.Name, ServerUtils.GetMoneys(gold));
                        character.CharacterHandler.SendMessage(Service.SellItem(1, index, info));
                        break;
                    }
                    //Đồng ý bán item
                    case 1:
                    {
                        Model.Item.Item itemSell = null;
                        switch (type)
                        {
                            //Cải trang
                            case 0 when index == 5:
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DO_NOT_SELL_ITEM));
                                return;
                            //Body
                            case 0:
                                itemSell = character.ItemBody[index];
                                break;
                            //Bag
                            case 1:
                                itemSell = character.CharacterHandler.GetItemBagByIndex(index);
                                break;
                        }
                        if(itemSell == null) return;
                        var template = ItemCache.ItemTemplate(itemSell.Id);
                        var gold = template.SaleCoin;
                        if (gold == -1 || template.Type == 5)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DO_NOT_SELL_ITEM));
                            return;
                        }

                        if (itemSell.Id == 457)
                        {
                            gold = DataCache.GiaBanThoiVang;
                        }

                        var quantity = 1;
                        if (itemSell.Quantity > 1 && !ItemCache.IsItemSellOnlyOne(itemSell.Id))
                        {
                            quantity = itemSell.Quantity;
                        }
                        
                        gold *= quantity;


                        if (character.InfoChar.Gold + gold > character.InfoChar.LimitGold)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().GOLD_BAR_TO_GOLD_LIMIT));
                            return;
                        }

                        character.PlusGold(gold);
                        
                        if (type == 0)
                        {
                            character.ItemBody[index] = null;
                            character.Delay.NeedToSaveBody = true;
                            // character.Delay.SaveData += 1000;
                            var timeServer = ServerUtils.CurrentTimeMillis();
                            character.Delay.InvAction = timeServer + 1000;
                            if ((character.InfoChar.ThoiGianDoiMayChu - timeServer) < 180000)
                            {
                                character.InfoChar.ThoiGianDoiMayChu = timeServer + 300000;
                            }

                            character.CharacterHandler.UpdateInfo();

                        }
                        else
                        {
                            character.CharacterHandler.RemoveItemBagByIndex(index, quantity, reason:"Bán cho shop");
                        }
                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                        character.CharacterHandler.SendMessage(Service.BuyItem(character));
                        character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().SELL_ITEM_GOLD, ServerUtils.GetMoneys(gold))));
                        break;
                    }
                }
               
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error sell item in ItemHandler.cs: {e.Message} \n {e.StackTrace}", e);
                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().ERROR_SERVER));
                character.CharacterHandler.SendMessage(Service.BuyItem(character));

                UserDB.BanUser(character.Player.Id);
                ClientManager.Gi().KickSession(character.Player.Session);
                ServerUtils.WriteLog("hackbando", $"Tên tài khoản {character.Player.Username} (ID:{character.Player.Id}) hack hackbando");

                var temp = ClientManager.Gi().GetPlayer(character.Player.Id);
                if (temp != null)
                {
                    ClientManager.Gi().KickSession(temp.Session);
                }
            }
        }

        public static void TradeItem(Message message, Model.Character.Character character)
        {
            try
            {
                var action = message.Reader.ReadByte();
                var map = character.Zone.Map;

                if (map != null)
                {
                    var zone = map.GetZoneById(character.InfoChar.ZoneId);
                    if (zone == null)
                    {
                        if (character.Trade.IsTrade)
                        {
                            var charTemp =
                                (Model.Character.Character) ClientManager.Gi()
                                    .GetCharacter(character.Trade.CharacterId);
                            if (charTemp != null && charTemp.Trade.CharacterId == character.Id)
                            {
                                charTemp.CloseTrade(false);
                            }

                            character.CloseTrade(false);
                        }

                        return;
                    }

                    switch (action)
                    {
                        //send invite trade me to player
                        case 0:
                        {
                            var delayTrade = character.Delay.Trade;
                            var timeServer = ServerUtils.CurrentTimeMillis();
                            if (delayTrade > timeServer)
                            {
                                var time = (delayTrade - timeServer) / 1000;
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(string.Format(TextServer.gI().DELAY_TRADE, time)));
                                return;
                            }

                            if (character.Trade.IsTrade || character.InfoChar.IsDie)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().NOT_TRADE_WITH_PLAYER));
                                return;
                            }

                            var charId = message.Reader.ReadInt();
                            var player = (Model.Character.Character) zone.ZoneHandler.GetCharacter(charId);
                            if (player != null)
                            {
                                if (!player.InfoChar.IsPremium)
                                {
                                    if (player.InfoChar.SoLanGiaoDich > DataCache.LIMIT_NOT_PREMIUM_TRADE_DAY)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.ServerMessage(TextServer.gI().NOT_PREMIUM));
                                        return;
                                    }

                                    if (player.InfoChar.ThoiGianGiaoDich > timeServer)
                                    {
                                        var delay = (player.InfoChar.ThoiGianGiaoDich - timeServer) / 1000;
                                        if (delay < 1)
                                        {
                                            delay = 1;
                                        }

                                        character.CharacterHandler.SendMessage(Service.DialogMessage(string.Format(TextServer.gI().DELAY_SEC,
                                                delay)));
                                        return;
                                    }
                                }

                                if (player.Trade.IsTrade || player.InfoChar.IsDie)
                                {
                                    character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(TextServer.gI().NOT_TRADE_WITH_PLAYER));
                                    return;
                                }

                                player.CharacterHandler.SendMessage(Service.Trade01(0, character.Id));
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                            }

                            break;
                        }
                        //accept trade player to me
                        case 1:
                        {
                            if (character.Trade.IsTrade || character.InfoChar.IsDie)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().NOT_TRADE_WITH_PLAYER));
                                return;
                            }

                            var charId = message.Reader.ReadInt();
                            var player = (Model.Character.Character) zone.ZoneHandler.GetCharacter(charId);
                            if (player != null)
                            {
                                if (player.Trade.IsTrade || player.InfoChar.IsDie)
                                {
                                    character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(TextServer.gI().NOT_TRADE_WITH_PLAYER));
                                    return;
                                }

                                player.CharacterHandler.SendMessage(Service.Trade01(1, character.Id));
                                character.CharacterHandler.SendMessage(Service.Trade01(1, player.Id));
                                //setup trade me
                                character.Trade.CharacterId = player.Id;
                                character.Trade.IsTrade = true;

                                //setup trade player
                                player.Trade.CharacterId = character.Id;
                                player.Trade.IsTrade = true;
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                            }

                            break;
                        }
                        //Add item to trade
                        case 2:
                        {
                            var index = message.Reader.ReadByte();
                            var quantity = message.Reader.ReadInt();
                            Server.Gi().Logger
                                .Debug(
                                    $"Client: {character.Player.Session.Id} -------------------- index: {index} - quantity: {quantity}");
                            try
                            {
                                if (!character.Trade.IsTrade || character.Trade.IsLock) return;
                                var player =
                                    (Model.Character.Character) zone.ZoneHandler.GetCharacter(character.Trade
                                        .CharacterId);
                                if (player != null && player.Trade.CharacterId == character.Id)
                                {
                                    if (index == -1)
                                    {
                                        if (character.InfoChar.Gold < quantity) return;
                                        // Acc thường không giao dịch vàng được quá 100tr 1 lần
                                        if (!character.InfoChar.IsPremium && quantity >= DataCache.LIMIT_NOT_PREMIUM_TRADE_GOLD_AMOUNT)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().NOT_PREMIUM_TRADE_GOLD));
                                            return;
                                        }

                                        if (!player.InfoChar.IsPremium && quantity >= DataCache.LIMIT_NOT_PREMIUM_TRADE_GOLD_AMOUNT)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().PLAYER_NOT_PREMIUM_TRADE_GOLD));
                                            return;
                                        }

                                        var checkItemGold =
                                            character.Trade.Items.FirstOrDefault(item => item.Id == 76);
                                        if (checkItemGold == null)
                                        {
                                            var itemGold = ItemCache.GetItemDefault(76);
                                            itemGold.Quantity = quantity;
                                            itemGold.IndexUI = index;
                                            character.Trade.Items.Add(itemGold);
                                        }
                                        else
                                        {
                                            checkItemGold.Quantity = quantity;
                                        }
                                    }
                                    else
                                    {
                                        var itemTrade = character.CharacterHandler.GetItemBagByIndex(index);
                                        if (itemTrade == null) return;

                                        if (itemTrade.Quantity < quantity || quantity > 99)
                                        {
                                            if (itemTrade.Id == 457)
                                            {
                                                character.CharacterHandler.SendMessage(Service.DialogMessage(TextServer.gI().SPLIT_GOLD_FIRST));
                                            }
                                            character.CharacterHandler.SendMessage(Service.Trade2(index));
                                            return;
                                        }

                                        var template = ItemCache.ItemTemplate(itemTrade.Id);
                                        if (DataCache.IsIdItemNotTrade(itemTrade.Id) ||
                                            (!DataCache.TypeItemTrade.Contains(template.Type) && !DataCache.ItemPremiumTrade.Contains(itemTrade.Id) && !DataCache.ItemNormalTrade.Contains(itemTrade.Id)))
                                        {
                                            character.CharacterHandler.SendMessage(Service.Trade2(index));
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().NOT_TRADE_ITEM));
                                            return;
                                        }
                                        // ko có VIP thì ko giao dịch được item có SPL
                                        if (!character.InfoChar.IsPremium && itemTrade.Options.FirstOrDefault(option => option.Id == 107) != null && Server.Gi().LockCloneGiaoDich)
                                        {
                                            character.CharacterHandler.SendMessage(Service.Trade2(index));
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().NOT_PREMIUM_TRADE_SPL_ITEM));
                                            return;
                                        }
                                        
                                        if (!player.InfoChar.IsPremium && itemTrade.Options.FirstOrDefault(option => option.Id == 107) != null)
                                        {
                                            character.CharacterHandler.SendMessage(Service.Trade2(index));
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().PLAYER_NOT_PREMIUM_TRADE_SPL_ITEM));
                                            return;
                                        }

                                        // ko có VIP thì ko giao dịch được item có cấp bậc
                                        if (!character.InfoChar.IsPremium && itemTrade.Options.FirstOrDefault(option => option.Id == 72) != null && Server.Gi().LockCloneGiaoDich)
                                        {
                                            character.CharacterHandler.SendMessage(Service.Trade2(index));
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().NOT_PREMIUM_TRADE_LEVEL_ITEM));
                                            return;
                                        }
                                        
                                        if (!player.InfoChar.IsPremium && itemTrade.Options.FirstOrDefault(option => option.Id == 72) != null)
                                        {
                                            character.CharacterHandler.SendMessage(Service.Trade2(index));
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().PLAYER_NOT_PREMIUM_TRADE_LEVEL_ITEM));
                                            return;
                                        }

                                        // ko có VIP thì ko giao dịch được thỏi vàng
                                        if (!character.InfoChar.IsPremium && DataCache.ItemPremiumTrade.Contains(itemTrade.Id))
                                        {
                                            character.CharacterHandler.SendMessage(Service.Trade2(index));
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(String.Format(TextServer.gI().NOT_PREMIUM_TRADE_GOLD_BAR, template.Name)));
                                            return;
                                        }
                                        if (!player.InfoChar.IsPremium && DataCache.ItemPremiumTrade.Contains(itemTrade.Id))
                                        {
                                            character.CharacterHandler.SendMessage(Service.Trade2(index));
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(String.Format(TextServer.gI().PLAYER_NOT_PREMIUM_TRADE_GOLD_BAR, template.Name)));
                                            return;
                                        }

                                        var itemOptionNotTrade = itemTrade.Options.FirstOrDefault(option => option.Id == 30);
                                        if (itemOptionNotTrade != null)
                                        {
                                            character.CharacterHandler.SendMessage(Service.Trade2(index));
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().NOT_TRADE_ITEM));
                                            return;
                                        }

                                        var itemSKH = itemTrade.Options.FirstOrDefault(option => option.Id >= 127 && option.Id <= 135);
                                        if (itemSKH != null)
                                        {
                                            character.CharacterHandler.SendMessage(Service.Trade2(index));
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().NOT_TRADE_ITEM));
                                            return;
                                        }

                                        var checkItemCheck = character.Trade.Items.FirstOrDefault(item =>
                                            item.Id == itemTrade.Id && item.IndexUI == itemTrade.IndexUI);
                                        if (checkItemCheck == null)
                                        {
                                            var itemClone = ItemHandler.Clone(itemTrade);
                                            if (quantity == 0)
                                            {
                                                quantity = 1;
                                            }
                                            itemClone.Quantity = quantity;
                                            itemClone.IndexUI = index;
                                            character.Trade.Items.Add(itemClone);
                                        }
                                        else
                                        {
                                            checkItemCheck.Quantity = quantity;
                                        }
                                    }
                                }
                                else
                                {
                                    character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                                    character.CloseTrade(true);
                                }
                            }
                            catch (Exception)
                            {
                                var charTemp =
                                    (Model.Character.Character) zone.ZoneHandler.GetCharacter(character.Trade
                                        .CharacterId);
                                if (charTemp != null && charTemp.Trade.CharacterId == character.Id)
                                {
                                    charTemp.CloseTrade(true);
                                }
                                character.CloseTrade(true);
                            }

                            break;
                        }
                        //Huỷ giao dịch
                        case 3:
                        {
                            if (!character.Trade.IsTrade) return;
                            var player =
                                (Model.Character.Character) zone.ZoneHandler.GetCharacter(character.Trade.CharacterId);
                            if (player != null && player.Trade.CharacterId == character.Id)
                            {
                                player.CloseTrade(true);
                                player.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().CLOSE_TRADE));
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                            }

                            character.CloseTrade(true);
                            break;
                        }
                        //Khoá giao dịch
                        case 5:
                        {
                            try
                            {
                                if (!character.Trade.IsTrade) return;
                                character.Trade.IsLock = true;
                                var player =
                                    (Model.Character.Character) zone.ZoneHandler.GetCharacter(character.Trade
                                        .CharacterId);
                                if (player != null && player.Trade.CharacterId == character.Id)
                                {
                                    player.CharacterHandler.SendMessage(Service.Trade6(character.Trade.Items));
                                }
                                else
                                {
                                    character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                                    character.CloseTrade(true);
                                }
                            }
                            catch (Exception)
                            {
                                var charTemp =
                                    (Model.Character.Character) zone.ZoneHandler.GetCharacter(character.Trade
                                        .CharacterId);
                                if (charTemp != null && charTemp.Trade.CharacterId == character.Id)
                                {
                                    charTemp.CloseTrade(true);
                                }

                                character.CloseTrade(true);
                            }

                            break;
                        }
                        //Hoàn thành giao dịch
                        case 7:
                        {
                            try
                            {
                                var logPlayer = "";
                                var logCharacter = "";
                                var tongGiaoDichThoiVang = 0;
                                if (!character.Trade.IsTrade) return;
                                character.Trade.IsHold = true;
                                var player =
                                    (Model.Character.Character) zone.ZoneHandler.GetCharacter(character.Trade
                                        .CharacterId);
                                if (player != null && player.Trade.CharacterId == character.Id)
                                {
                                    if (player.Trade.IsHold)
                                    {
                                        var itemGoldMe = character.Trade.Items.FirstOrDefault(item => item.IndexUI == -1);
                                        var itemGoldPlayer = player.Trade.Items.FirstOrDefault(item => item.IndexUI == -1);
                                        var goldMe = 0;
                                        var goldPlayer = 0;
                                        if (itemGoldMe != null)
                                        {
                                            goldMe = itemGoldMe.Quantity;
                                        }
                                        if (itemGoldPlayer != null)
                                        {
                                            goldPlayer = itemGoldPlayer.Quantity;
                                        }

                                        var listItemMe = character.Trade.Items.Where(item => item.IndexUI != -1).ToList();
                                        var listItemPlayer = player.Trade.Items.Where(item => item.IndexUI != -1).ToList();

                                        if (listItemMe.Count > player.LengthBagNull())
                                        {
                                            player.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().TRADE_ERROR));
                                            player.CloseTrade(false);
                                            character.CloseTrade(true);
                                            return;
                                        }

                                        if (listItemPlayer.Count > character.LengthBagNull())
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                                            player.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().TRADE_ERROR));
                                            player.CloseTrade(false);
                                            character.CloseTrade(true);
                                            return;
                                        }

                                        if (goldMe > character.InfoChar.Gold)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                                            player.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().TRADE_ERROR));
                                            player.CloseTrade(false);
                                            character.CloseTrade(true);
                                            return;
                                        }

                                        if (goldPlayer > player.InfoChar.Gold)
                                        {
                                            player.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().TRADE_ERROR));
                                            player.CloseTrade(false);
                                            character.CloseTrade(true);
                                            return;
                                        }

                                        //Check error item trade

                                        #region Check error item trade
                                        var itemCheck = listItemMe.FirstOrDefault(item =>
                                        {
                                            var itemBag = character.CharacterHandler.GetItemBagByIndex(item.IndexUI);
                                            return itemBag.Id != item.Id || itemBag.Quantity < item.Quantity;
                                        });
                                        if (itemCheck != null)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().TRADE_ERROR));
                                            player.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().TRADE_ERROR));
                                            player.CloseTrade(false);
                                            character.CloseTrade(true);
                                            return;
                                        }

                                        itemCheck = listItemPlayer.FirstOrDefault(item =>
                                        {
                                            var itemBag = player.CharacterHandler.GetItemBagByIndex(item.IndexUI);
                                            return itemBag.Id != item.Id || itemBag.Quantity < item.Quantity;
                                        });
                                        if (itemCheck != null)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().TRADE_ERROR));
                                            player.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().TRADE_ERROR));
                                            player.CloseTrade(false);
                                            character.CloseTrade(true);
                                            return;
                                        }
                                        #endregion
                                        
                                        logPlayer = $"{player.Name} đã giao dịch với {character.Name}: ";
                                        logCharacter = $"{character.Name} đã giao dịch với {player.Name}: ";
                                        //Remove item
                                        listItemMe.ForEach(item =>
                                        {
                                            if (item == listItemMe.LastOrDefault())
                                            {
                                                if (item != null)
                                                {
                                                    var itemTemplate = ItemCache.ItemTemplate(item.Id);
                                                    logCharacter += $"cho: ({item.IndexUI}){item.Quantity}x{itemTemplate.Name},";
                                                    character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI,
                                                        item.Quantity, reason:"Giao dịch với " + player.Name);
                                                }
                                            }
                                            else
                                            {
                                                var itemTemplate = ItemCache.ItemTemplate(item.Id);
                                                logCharacter += $"cho: ({item.IndexUI}){item.Quantity}x{itemTemplate.Name},";
                                                character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI,
                                                    item.Quantity, reset: false, reason:"Giao dịch với " + player.Name);
                                            }
                                        });

                                        listItemPlayer.ForEach(item =>
                                        {
                                            if (item == listItemPlayer.LastOrDefault())
                                            {
                                                if (item != null)
                                                {
                                                    var itemTemplate = ItemCache.ItemTemplate(item.Id);
                                                    logPlayer += $"cho: ({item.IndexUI}){item.Quantity}x{itemTemplate.Name},";
                                                    player.CharacterHandler.RemoveItemBagByIndex(item.IndexUI,
                                                        item.Quantity, reason:"Giao dịch với " + character.Name);
                                                }
                                            }
                                            else
                                            {
                                                var itemTemplate = ItemCache.ItemTemplate(item.Id);
                                                logPlayer += $"cho: ({item.IndexUI}){item.Quantity}x{itemTemplate.Name},";
                                                player.CharacterHandler.RemoveItemBagByIndex(item.IndexUI,
                                                    item.Quantity, reset: false, reason:"Giao dịch với " + character.Name);
                                            }
                                        });

                                        //Add item
                                        listItemPlayer.ForEach(item =>
                                        {
                                            if (item.Id == 457)
                                            {
                                                tongGiaoDichThoiVang += item.Quantity;
                                            }
                                            var itemTemplate = ItemCache.ItemTemplate(item.Id);
                                            logCharacter += $"nhận: {item.Quantity}x{itemTemplate.Name},";
                                            character.CharacterHandler.AddItemToBag(false, item, "(GD) Nhận từ " + player.Name);
                                        });

                                        listItemMe.ForEach(item =>
                                        {
                                            if (item.Id == 457)
                                            {
                                                tongGiaoDichThoiVang += item.Quantity;
                                            }
                                            var itemTemplate = ItemCache.ItemTemplate(item.Id);
                                            logPlayer += $"nhận: {item.Quantity}x{itemTemplate.Name},";
                                            player.CharacterHandler.AddItemToBag(false, item, "(GD) Nhận từ " + character.Name);
                                        });

                                        if (goldMe > 0)
                                        {
                                            character.MineGold(goldMe);
                                            player.PlusGold(goldMe);
                                            logPlayer += $"+G: {goldMe},";
                                            logCharacter += $"-G: {goldMe},";
                                            player.CharacterHandler.SendMessage(Service.BuyItem(player));
                                            character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                        }

                                        if (goldPlayer > 0)
                                        {
                                            player.MineGold(goldPlayer);
                                            character.PlusGold(goldPlayer);
                                            logPlayer += $"-G: {goldPlayer},";
                                            logCharacter += $"+G: {goldPlayer},";
                                            character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                            player.CharacterHandler.SendMessage(Service.BuyItem(player));
                                        }
                                        var timeServer = ServerUtils.CurrentTimeMillis();

                                        if (!character.Delay.IsSavingInventory)
                                        {
                                            character.Delay.IsSavingInventory = true;
                                            character.Delay.SaveInvData = 10000 + timeServer;
                                            character.Delay.InvAction = timeServer + 15000;
                                            if (CharacterDB.SaveInventory(character, true, true, true))
                                            {
                                                character.Delay.InvAction = timeServer;
                                            }
                                            character.Delay.IsSavingInventory = false;
                                        }

                                        if (!player.Delay.IsSavingInventory)
                                        {
                                            player.Delay.IsSavingInventory = true;
                                            player.Delay.SaveInvData = 10000 + timeServer;
                                            player.Delay.InvAction = timeServer + 15000;
                                            if (CharacterDB.SaveInventory(player, true, true, true))
                                            {
                                                player.Delay.InvAction = timeServer;
                                            }
                                            player.Delay.IsSavingInventory = false;
                                        }

                                        character.InfoChar.SoLanGiaoDich++;
                                        player.InfoChar.SoLanGiaoDich++;
                                        character.InfoChar.ThoiGianGiaoDich = timeServer + DataCache.LIMIT_NOT_PREMIUM_TRADE_TIME;
                                        player.InfoChar.ThoiGianGiaoDich = timeServer + DataCache.LIMIT_NOT_PREMIUM_TRADE_TIME;
                                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                                        player.CharacterHandler.SendMessage(Service.SendBag(player));
                                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().TRADE_SUCCESS));
                                        player.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().TRADE_SUCCESS));
                                        character.CloseTrade(true);
                                        player.CloseTrade(false);
                                        // CharacterDB.Update(player);
                                        // CharacterDB.Update(character);
                                        ServerUtils.WriteTradeLog(logPlayer, tongGiaoDichThoiVang);
                                        ServerUtils.WriteTradeLog(logCharacter, tongGiaoDichThoiVang);
                                    }
                                    else
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.ServerMessage(TextServer.gI().TRADE_HOLD));
                                    }
                                }
                                else
                                {
                                    character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(TextServer.gI().TRADE_ERROR));
                                    character.CloseTrade(true);
                                }
                            }
                            catch (Exception)
                            {
                                var charTemp =
                                    (Model.Character.Character) zone.ZoneHandler.GetCharacter(character.Trade
                                        .CharacterId);
                                if (charTemp != null && charTemp.Trade.CharacterId == character.Id)
                                {
                                    charTemp.CloseTrade(true);
                                }

                                character.CloseTrade(true);
                            }

                            break;
                        }
                    }
                }
                else
                {
                    character.CharacterHandler.SendMessage(
                        Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Trade Item in itemHandler.cs: {e.Message} \n {e.StackTrace}", e);
            }
            finally
            {
                message?.CleanUp();
            }
        }

        public static void ConfirmUseItemBag(Model.Character.Character character, int index, short template = -1)
        {
            Model.Item.Item itemUse;
            if (template != -1 && DataCache.IdDauThan.Contains(template))
            {
                itemUse = character.ItemBag.FirstOrDefault(item => item.Id == template);
                if(itemUse != null) UsePea(character, itemUse, 0);
                return;
            }

            itemUse = character.CharacterHandler.GetItemBagByIndex(index);
            if (itemUse == null) return;
            
            var itemTemplate = ItemCache.ItemTemplate(itemUse.Id);
            if (itemTemplate.Require > character.InfoChar.Power)
            {
                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_POWER));
                return;
            }

            if (itemTemplate.Gender != 3 && itemTemplate.Gender != character.InfoChar.Gender)
            {
                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_GENDER));
                return;
            }

            if (itemTemplate.Type == 22)
            {
                UseAuraItem(character, itemUse);
                return;
            }
        }

        public static void UseItemBag(Model.Character.Character character, int index, short template = -1)
        {
            try
            {
                Model.Item.Item itemUse;
                if (template != -1 && DataCache.IdDauThan.Contains(template))
                {
                    itemUse = character.ItemBag.FirstOrDefault(item => item.Id == template);
                    if(itemUse != null) UsePea(character, itemUse, 0);
                    return;
                }

                itemUse = character.CharacterHandler.GetItemBagByIndex(index);
                if (itemUse == null) return;
                
                var itemTemplate = ItemCache.ItemTemplate(itemUse.Id);
                if (itemTemplate.Require > character.InfoChar.Power)
                {
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_POWER));
                    return;
                }

                if (itemTemplate.Gender != 3 && itemTemplate.Gender != character.InfoChar.Gender)
                {
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_GENDER));
                    return;
                }

                var yeuCauSucManhTi = itemUse.Options.FirstOrDefault(opt => opt.Id == 21);
                if (yeuCauSucManhTi != null)
                {
                    if ((long)((long)yeuCauSucManhTi.Param * 1000000000) > character.InfoChar.Power)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_POWER));
                        return;
                    }
                }

                // Gold Bar
                if (itemTemplate.Id == 457)
                {
                    if (itemUse.Quantity > 99)
                    {
                        if (character.LengthBagNull() <= 0)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                            return;
                        }
                        character.CharacterHandler.RemoveItemBagByIndex(itemUse.IndexUI, 99, reason: "Tách thỏi vàng");
                        var itemAdd = ItemCache.GetItemDefault(457);
                        itemAdd.Quantity = 99;
                        character.CharacterHandler.AddItemToBag(false, itemAdd, "Tách thỏi vàng");
                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                    }
                    return;
                }
                //Use Card
                if (itemTemplate.Type == 33)
                {
                    UseCard(character, itemUse);
                    return;
                }

                if (itemTemplate.Type == 22)
                {
                    character.CharacterHandler.SendMessage(Service.UseItem(1, 3, index, String.Format(TextServer.gI().CONFIRM_USE_ITEM, itemTemplate.Name)));
                    return;
                }


                if (itemTemplate.IsTypeBody())
                {
                    // Nếu mặc vào giáp luyện tập sẽ xóa bỏ mọi hiệu ứng cộng thêm dmg
                    if (ItemCache.ItemIsGiapLuyenTap(itemUse.Id))
                    {
                        character.InfoMore.LastGiapLuyenTapItemId = 0;
                        character.Delay.GiapLuyenTap = -1;
                    }
                    
                    var indexInBody = itemTemplate.Type == 32 ? 6 : itemTemplate.Type;
                    var itemBody = character.ItemBody[indexInBody];
                    var itemClone = Clone(itemUse);
                    itemClone.IndexUI = indexInBody;
                    character.ItemBody[indexInBody] = itemClone;
                    if (itemBody != null)
                    {
                        itemBody.IndexUI = index;
                        character.ItemBag[index] = itemBody;
                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                    }
                    else
                    {
                        character.CharacterHandler.RemoveItemBag(index, reason:"Mặc vào người");
                    }
                    character.Delay.NeedToSaveBody = true;
                    var timeServer = ServerUtils.CurrentTimeMillis();
                    character.Delay.InvAction = timeServer + 1000;
                    if ((character.InfoChar.ThoiGianDoiMayChu - timeServer) < 180000)
                    {
                        character.InfoChar.ThoiGianDoiMayChu = timeServer + 300000;
                    }
                    // character.Delay.SaveData += 1000;
                    character.CharacterHandler.UpdateInfo();
                    return;
                }

                if (itemTemplate.Type == 7 && UseBook(character, itemUse))
                {                             
                    character.CharacterHandler.RemoveItemBag(index, reason:"Sách kĩ năng");
                    return;
                }
                if (itemTemplate.Type == 11)
                {
                    character.CharacterHandler.UpdatePhukien();
                    return;
                }
                if (itemTemplate.Type == 12)
                {
                    // Ngọc rồng thường
                    if (itemTemplate.Id <= 20)
                    {
                        UseDragonBall(character, itemUse);
                    }
                    return;
                }

                if (ItemCache.IsPetItem(itemTemplate.Id))
                {
                    UsePetItem(character, itemUse);
                    return;
                }

                switch (itemTemplate.Id)
                {
                    //Dau than
                    case 13:
                    case 60:
                    case 61:
                    case 62:
                    case 63:
                    case 64:
                    case 65:
                    case 352:
                    case 523:
                    case 595:
                    {
                        UsePea(character, itemUse, 0);
                        return;
                    }
                    //Capsule
                    case 193:
                    case 194: {
                        character.CharacterHandler.SendMessage(Service.MapTranspot(character.MapTranspots));
                        if (itemUse.Id == 193)
                        {
                            character.IsItem193 = true;
                        }
                        return;
                    }
                    //Đổi đệ tử
                    case 401:
                    {
                        var disciple = character.Disciple;
                        if (disciple == null)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DONT_FIND_DISCIPLE));
                            return;
                        }

                        var itemDiscipleBody = disciple.ItemBody.FirstOrDefault(item => item != null);

                        if (itemDiscipleBody != null)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().PLEASE_EMPTY_DISCIPLE_BODY));
                            return;
                        }

                        var oldStatus = disciple.Status;

                        if (oldStatus < 3)
                        {
                            character.Zone.ZoneHandler.RemoveDisciple(character.Disciple);
                        }

                        disciple = new Disciple();
                        disciple.CreateNewDisciple(character);
                        disciple.Player = character.Player;
                        disciple.Zone = character.Zone;
                        disciple.CharacterHandler.SetUpInfo();
                        character.Disciple = disciple;

                        if (!character.InfoChar.Fusion.IsFusion && oldStatus < 3)
                        {
                            character.Zone.ZoneHandler.AddDisciple(disciple);
                        }
                        else
                        {
                            character.CharacterHandler.SetUpInfo();
                            character.CharacterHandler.SendMessage(Service.MeLoadPoint(character));
                            character.CharacterHandler.SendMessage(Service.SendHp((int)character.InfoChar.Hp));
                            character.CharacterHandler.SendMessage(Service.SendMp((int)character.InfoChar.Mp));
                            character.CharacterHandler.SendZoneMessage(Service.PlayerLevel(character));
                        }
                        character.CharacterHandler.RemoveItemBagByIndex(itemUse.IndexUI, 1, reason:"Dùng đổi đệ tử");
                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                        DiscipleDB.Update(disciple);
                        break;
                    }
                    //Nâng kỹ năng đệ tử 1
                    case 402:
                    case 403:
                    case 404:
                    case 759:
                    {
                        var disciple = character.Disciple;
                        if (disciple == null)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DONT_FIND_DISCIPLE));
                            return;
                        }
                        var skill1 = disciple.Skills[0];
                        switch (itemUse.Id)
                        {
                            case 403:
                            {
                                if (disciple.Skills.Count < 2)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_FOUND_SKILL_DISCIPLE));
                                    return;
                                }

                                skill1 = disciple.Skills[1];
                                break;
                            }
                            case 404:
                            {
                                if (disciple.Skills.Count < 3)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_FOUND_SKILL_DISCIPLE));
                                    return;
                                }
                                skill1 = disciple.Skills[2];
                                break;
                            }
                            case 759:
                            {
                                if (disciple.Skills.Count < 4)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_FOUND_SKILL_DISCIPLE));
                                    return;
                                }
                                skill1 = disciple.Skills[3];
                                break;
                            }
                        }

                        if (skill1.Point >= 7)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().MAX_POINT_SKILL_DISCIPLE));
                            return;
                        }
                        skill1.Point++;
                        skill1.CoolDown = -1;
                        skill1.SkillId++;
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().SUCCESS_POINT_SKILL_DISCIPLE));
                        character.CharacterHandler.SendMessage(Service.ClosePanel());
                        // character.CharacterHandler.RemoveItemBag(index);
                        character.CharacterHandler.RemoveItemBagByIndex(itemUse.IndexUI, 1, reason:"Dùng sách kĩ năng đệ tử");
                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                        break;
                    }
                    //Bông tai
                    case 454:
                    {
                        var disciple = character.Disciple;
                        if (disciple == null)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DONT_FIND_DISCIPLE));
                            return;
                        }

                        if (character.Zone.Map.IsMapCustom())
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DONT_NOT_ACTION_DISCIPLE_HERE));
                            return;
                        }


                        var timeServer = ServerUtils.CurrentTimeMillis();

                        if (character.InfoChar.Fusion.IsFusion)
                        {
                            if (character.InfoChar.Fusion.IsPorata2)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().PLEASE_USE_PORATA_2));
                                // Hợp thể cấp 2 không thể mở bằng item cấp 1
                                return;
                            }
                            disciple.CharacterHandler.SetUpPosition(isRandom: true);
                            character.Zone.ZoneHandler.AddDisciple(disciple);
                            character.CharacterHandler.SendZoneMessage(Service.Fusion(character.Id, 1));
                            lock (character.InfoChar.Fusion)
                            {
                                character.InfoChar.Fusion.IsFusion = false;
                                character.InfoChar.Fusion.IsPorata = false;
                                character.InfoChar.Fusion.TimeStart = timeServer;
                                character.InfoChar.Fusion.DelayFusion = timeServer + 600000;
                                character.InfoChar.Fusion.TimeUse = 0;
                            }
                            
                            disciple.Status = 0;
                            character.Delay.BongTaiPorata = 5000 + timeServer;
                        }
                        else
                        {
                            if (disciple.InfoChar.IsDie) //disciple.Status >= 3 || 
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CAN_NOT_USE_FUSION));
                                return;
                            }  

                            if (character.Delay.BongTaiPorata > timeServer)
                            {
                                var delay = (character.Delay.BongTaiPorata - timeServer) / 1000;
                                if (delay < 1)
                                {
                                    delay = 1;
                                }

                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(string.Format(TextServer.gI().DELAY_FUSION_SEC,
                                        delay)));
                                return;
                            }

                            if (disciple.InfoSkill.HuytSao.IsHuytSao)
                            {
                                SkillHandler.RemoveHuytSao(disciple);
                            }

                            if (disciple.InfoSkill.Monkey.MonkeyId == 1)
                            {
                                SkillHandler.HandleMonkey(disciple,false);
                            }
                            
                            if (disciple.Status < 3 && disciple.Zone != null && disciple.Zone.ZoneHandler != null) 
                            {
                                disciple.Zone.ZoneHandler.RemoveDisciple(disciple);
                            }
                            
                            character.CharacterHandler.SendZoneMessage(Service.Fusion(character.Id, 6));
                            lock (character.InfoChar.Fusion)
                            {
                                character.InfoChar.Fusion.IsFusion = true;
                                character.InfoChar.Fusion.IsPorata = true;
                                character.InfoChar.Fusion.TimeStart = timeServer;
                                character.InfoChar.Fusion.TimeUse = 600000;
                            }
                            disciple.Status = 4;
                        }
                        character.CharacterHandler.SetUpInfo();
                        character.CharacterHandler.PlusHp((int)character.HpFull);
                        character.CharacterHandler.PlusMp((int)character.MpFull);
                        character.CharacterHandler.SendZoneMessage(Service.UpdateBody(character));
                        character.CharacterHandler.SendMessage(Service.PlayerLoadSpeed(character));
                        character.CharacterHandler.SendMessage(Service.MeLoadPoint(character));
                        character.CharacterHandler.SendMessage(Service.SendHp((int)character.HpFull));
                        character.CharacterHandler.SendMessage(Service.SendMp((int)character.MpFull));
                        character.CharacterHandler.SendZoneMessage(Service.PlayerLevel(character));
                        break;
                    }
                    //Bông tai cấp 2
                    case 921:
                    {
                        var disciple = character.Disciple;
                        if (disciple == null)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DONT_FIND_DISCIPLE));
                            return;
                        }

                        if (character.Zone.Map.IsMapCustom())
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DONT_NOT_ACTION_DISCIPLE_HERE));
                            return;
                        }

                        var timeServer = ServerUtils.CurrentTimeMillis();

                        if (character.InfoChar.Fusion.IsFusion)
                        {
                            if (character.InfoChar.Fusion.IsPorata)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().PLEASE_USE_PORATA));
                                // Hợp thể cấp 1 không thể mở bằng item cấp 2
                                return;
                            }

                            disciple.CharacterHandler.SetUpPosition(isRandom: true);
                            character.Zone.ZoneHandler.AddDisciple(disciple);
                            character.CharacterHandler.SendZoneMessage(Service.Fusion(character.Id, 1));
                            lock (character.InfoChar.Fusion)
                            {
                                character.InfoChar.Fusion.IsFusion = false;
                                character.InfoChar.Fusion.IsPorata2 = false;
                                character.InfoChar.Fusion.TimeStart = timeServer;
                                character.InfoChar.Fusion.DelayFusion = timeServer + 600000;
                                character.InfoChar.Fusion.TimeUse = 0;
                            }
                            
                            disciple.Status = 0;
                            character.Delay.BongTaiPorata = 5000 + timeServer;
                        }
                        else
                        {
                            if (disciple.InfoChar.IsDie) //disciple.Status >= 3 || 
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CAN_NOT_USE_FUSION));
                                return;
                            }  

                            if (character.Delay.BongTaiPorata > timeServer)
                            {
                                var delay = (character.Delay.BongTaiPorata - timeServer) / 1000;
                                if (delay < 1)
                                {
                                    delay = 1;
                                }

                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(string.Format(TextServer.gI().DELAY_FUSION_SEC,
                                        delay)));
                                return;
                            }

                            if (disciple.InfoSkill.HuytSao.IsHuytSao)
                            {
                                SkillHandler.RemoveHuytSao(disciple);
                            }

                            if (disciple.InfoSkill.Monkey.MonkeyId == 1)
                            {
                                SkillHandler.HandleMonkey(disciple,false);
                            }
                            
                            if (disciple.Status < 3 && disciple.Zone != null && disciple.Zone.ZoneHandler != null) 
                            {
                                disciple.Zone.ZoneHandler.RemoveDisciple(disciple);
                            }

                            character.CharacterHandler.SendZoneMessage(Service.Fusion(character.Id, 6));
                            lock (character.InfoChar.Fusion)
                            {
                                character.InfoChar.Fusion.IsFusion = true;
                                character.InfoChar.Fusion.IsPorata2 = true;
                                character.InfoChar.Fusion.TimeStart = timeServer;
                                character.InfoChar.Fusion.TimeUse = 600000;
                            }
                            disciple.Status = 4;
                        }
                        character.CharacterHandler.SetUpInfo();
                        character.CharacterHandler.PlusHp((int)character.HpFull);
                        character.CharacterHandler.PlusMp((int)character.MpFull);
                        character.CharacterHandler.SendZoneMessage(Service.UpdateBody(character));
                        character.CharacterHandler.SendMessage(Service.PlayerLoadSpeed(character));
                        character.CharacterHandler.SendMessage(Service.MeLoadPoint(character));
                        character.CharacterHandler.SendMessage(Service.SendHp((int)character.HpFull));
                        character.CharacterHandler.SendMessage(Service.SendMp((int)character.MpFull));
                        character.CharacterHandler.SendZoneMessage(Service.PlayerLevel(character));
                        break;
                    }
                    case 521:
                    {
                        var timeServer = ServerUtils.CurrentTimeMillis();
                        // Kiểm tra xem đã có dùng chưa,
                        // nếu đã có dùng thì xóa hiệu ứng và trả lại thời gian
                        var itemOption = itemUse.Options.FirstOrDefault(option => option.Id == 1);
                        if (character.InfoChar.TimeAutoPlay > 0)
                        {
                            // đã có dùng
                            var giayConLai = (character.InfoChar.TimeAutoPlay - timeServer)/1000;
                            itemUse.Quantity = 1;
                            if (giayConLai > 60)
                            {
                                var phutConLai = giayConLai/60;
                                itemOption.Param = (int)phutConLai;
                            }
                            else 
                            {
                                itemOption.Param = 0;
                            }
                            character.InfoChar.TimeAutoPlay = 0;
                            character.CharacterHandler.SendMessage(Service.ItemTime(4387, 0));
                            character.CharacterHandler.SendMessage(Service.AutoPlay(false));
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().END_AUTO_PLAY));
                        }
                        else
                        {
                            var soPhutSuDung = itemOption.Param;
                            var soGiaySuDung = soPhutSuDung*60;
                            character.InfoChar.TimeAutoPlay = timeServer + (soGiaySuDung*1000);
                            
                            itemOption.Param = 0;
                            character.CharacterHandler.SendMessage(Service.ItemTime(4387, soGiaySuDung));
                            character.CharacterHandler.SendMessage(Service.AutoPlay(true));
                            character.Delay.AutoPlay = 60000 + timeServer;
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().START_AUTO_PLAY));
                        }

                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                        break;
                    }
                    case 379:
                    {
                        UseMayDoCapsuleKiBi(character, itemUse);
                        break;
                    }
                    case 380:
                    {
                        UseCapsuleKiBi(character, itemUse);
                        break;
                    }
                    case 818:
                    {
                        UseCapsuleHalloween(character, itemUse);
                        break;
                    }
                    case 663:
                    case 664:
                    case 665:
                    case 666:
                    case 667:
                    case 670:
                    {
                        UseThucAn(character, itemUse);
                        break;
                    }
                    case 465:
                    case 466:
                    case 472:
                    case 473:
                    {
                        UseBanhTrungThuBuff(character, itemUse);
                        break;
                    }
                    case 381:
                    case 382:
                    case 383:
                    case 384:
                    case 385:
                    case 462:
                    {
                        UseBuffItem(character, itemUse);
                        break;
                    }
                    case 891:
                    {
                        UseBanhTrungThu(character, itemUse);
                        break;
                    }
                    case 737:
                    {
                        UseCapsuleTrungThu(character, itemUse);
                        break;
                    }
                    case 992:
                    {
                        character.InfoMore.TransportMapId = 160;
                        character.CharacterHandler.SendMessage(Service.Transport(3, 1));
                        break;
                    }
                }
                
                
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Use item in ItemHandler.cs: {e.Message} \n {e.StackTrace}", e);
                throw;
            }
            
        }

        public static void UseItemBox(Model.Character.Character character, int index, short template = -1)
        {
            try
            {
                Model.Item.Item itemUse;
                if (template != -1) return;
                
                itemUse = character.CharacterHandler.GetItemBoxByIndex(index);
                if (itemUse == null) return;
                
                var itemTemplate = ItemCache.ItemTemplate(itemUse.Id);
                if (itemTemplate.Require > character.InfoChar.Power)
                {
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_POWER));
                    return;
                }

                if (itemTemplate.Gender != 3 && itemTemplate.Gender != character.InfoChar.Gender)
                {
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_GENDER));
                    return;
                }

                var yeuCauSucManhTi = itemUse.Options.FirstOrDefault(opt => opt.Id == 21);
                if (yeuCauSucManhTi != null)
                {
                    if ((long)((long)yeuCauSucManhTi.Param * 1000000000) > character.InfoChar.Power)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_POWER));
                        return;
                    }
                }

                if (itemTemplate.IsTypeBody())
                {
                    var indexInBody = itemTemplate.Type == 32 ? 6 : itemTemplate.Type;
                    var itemBody = character.ItemBody[indexInBody];
                    var itemClone = Clone(itemUse);
                    itemClone.IndexUI = indexInBody;
                    character.ItemBody[indexInBody] = itemClone;
                    if (itemBody != null)
                    {
                        itemBody.IndexUI = index;
                        character.ItemBox[index] = itemBody;
                        character.CharacterHandler.SendMessage(Service.SendBox(character));
                    }
                    else
                    {
                        character.CharacterHandler.RemoveItemBox(index);
                    }
                    character.Delay.NeedToSaveBody = true;
                    var timeServer = ServerUtils.CurrentTimeMillis();
                    character.Delay.InvAction = timeServer + 1000;
                    if ((character.InfoChar.ThoiGianDoiMayChu - timeServer) < 180000)
                    {
                        character.InfoChar.ThoiGianDoiMayChu = timeServer + 300000;
                    }
                    // character.Delay.SaveData += 1000;
                    character.CharacterHandler.UpdateInfo();
                    return;
                }

                //Use Card
                if (itemTemplate.Type == 33)
                {
                    UseCard(character, itemUse, true);
                    return;
                }

                if (itemTemplate.Type == 7 && UseBook(character, itemUse))
                {
                    character.CharacterHandler.RemoveItemBox(index);
                    return;
                }

                switch (itemTemplate.Id)
                {
                    //Dau than
                    case 13:
                    case 60:
                    case 61:
                    case 62:
                    case 63:
                    case 64:
                    case 65:
                    case 352:
                    case 523:
                    case 595:
                    {
                        UsePea(character, itemUse, 1);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Use item in ItemHandler.cs: {e.Message} \n {e.StackTrace}", e);
                throw;
            }
            
        }

        public static void UseItemForDisciple(Model.Character.Character character, int index, short template = -1)
        {
            try
            {
                var disciple = character.Disciple;
                if (disciple == null || disciple.InfoChar.IsDie || disciple.Status >= 3)
                {
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_GENDER));
                    return;
                }
                
                Model.Item.Item itemUse;
                itemUse = character.CharacterHandler.GetItemBagByIndex(index);
                if (itemUse == null) return;

                var itemTemplate = ItemCache.ItemTemplate(itemUse.Id);
                
                if (itemTemplate.Gender != 3 && itemTemplate.Gender != disciple.InfoChar.Gender)
                {
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_GENDER));
                    return;
                }

                if (itemTemplate.Require > disciple.InfoChar.Power)
                {
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DISCIPLE_NOT_ENOUGH_POWER));
                    return;
                }

                var yeuCauSucManhTi = itemUse.Options.FirstOrDefault(opt => opt.Id == 21);
                if (yeuCauSucManhTi != null)
                {
                    if ((long)((long)yeuCauSucManhTi.Param * 1000000000) > disciple.InfoChar.Power)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DISCIPLE_NOT_ENOUGH_POWER));
                        return;
                    }
                }

                if (itemTemplate.IsTypeBody())
                {
                    var indexInBody = itemTemplate.Type == 32 ? 6 : itemTemplate.Type;
                    var itemBody = disciple.ItemBody[indexInBody];
                    var itemClone = Clone(itemUse);
                    itemClone.IndexUI = indexInBody;
                    disciple.ItemBody[indexInBody] = itemClone;
                    if (itemBody != null)
                    {
                        itemBody.IndexUI = index;
                        character.ItemBag[index] = itemBody;
                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                    }
                    else
                    {
                        character.CharacterHandler.RemoveItemBag(index,  reason:"Mặc cho đệ tử");
                    }
                    disciple.CharacterHandler.UpdateInfo();
                    character.CharacterHandler.SendMessage(Service.Disciple(2, disciple));
                    DiscipleDB.SaveInventory(disciple);
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Use item in ItemHandler.cs: {e.Message} \n {e.StackTrace}", e);
                throw;
            }
        }

        public static void UsePea(Model.Character.Character character, Model.Item.Item item, int type)
        {
            if(character.Delay.UsePea > ServerUtils.CurrentTimeMillis()) return;
            if (character.HpFull == character.InfoChar.Hp &&
                character.MpFull == character.InfoChar.Mp &&
                (!character.InfoChar.IsHavePet || character.InfoChar.IsHavePet &&
                    character.Disciple.HpFull == character.Disciple.InfoChar.Hp &&
                    character.Disciple.MpFull == character.Disciple.InfoChar.Mp &&
                    character.Disciple.InfoChar.Stamina == character.Disciple.InfoChar.MaxStamina)) return;

            switch (type)
            {
                case 0:
                    character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI, 1, reason:"Ăn đậu");
                    character.CharacterHandler.SendMessage(Service.SendBag(character));
                    break;
                case 1:
                    character.CharacterHandler.RemoveItemBoxByIndex(item.IndexUI, 1);
                    character.CharacterHandler.SendMessage(Service.SendBox(character));
                    break;
            }

            var plus = item.GetParamOption(2) * 1000 + item.GetParamOption(48);
            if (character.InfoChar.Hp < character.HpFull)
            {
                character.CharacterHandler.PlusHp(plus);
                character.CharacterHandler.SendMessage(Service.SendHp((int)character.InfoChar.Hp));  
                character.CharacterHandler.SendZoneMessage(Service.PlayerLevel(character));  
            }

            if (character.InfoChar.Mp < character.MpFull)
            {
                character.CharacterHandler.PlusMp(plus);
                character.CharacterHandler.SendMessage(Service.SendMp((int)character.InfoChar.Mp));  
            }
            character.Delay.UsePea = 10000 + ServerUtils.CurrentTimeMillis();

            var disciple = character.Disciple;
            if (disciple != null && !disciple.InfoChar.IsDie && disciple.Status < 3)
            {
                if (disciple.InfoChar.Hp < disciple.HpFull)
                {
                    disciple.CharacterHandler.PlusHp(plus);
                    disciple.CharacterHandler.SendZoneMessage(Service.PlayerLevel(disciple));  
                }

                if (disciple.InfoChar.Mp < disciple.MpFull)
                {
                    disciple.CharacterHandler.PlusMp(plus);
                }

                if (disciple.InfoChar.Stamina < disciple.InfoChar.MaxStamina)
                {
                    disciple.CharacterHandler.PlusStamina(100 * (DataCache.IdDauThan.IndexOf(item.Id) + 1));
                }
            }
        }

        private static bool UseBook(Model.Character.Character character, Model.Item.Item item)
        {
            var itemTemplate = ItemCache.ItemTemplate(item.Id);
            if (itemTemplate.Gender != character.InfoChar.Gender && itemTemplate.Gender != 3)
            {
                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DONOT_USE_SKILL));
                return false;
            }

            var skillTemplate = Cache.Gi().SKILL_TEMPLATES.FirstOrDefault(skill => skill.Id == itemTemplate.Skill);
            if (skillTemplate == null) return false;
            {
                var levelSkillBook = itemTemplate.Level;
                var skillChar = character.Skills.FirstOrDefault(skill => skill.Id == skillTemplate.Id);
                if (skillChar != null)
                {
                    if (levelSkillBook <= skillChar.Point)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DUPLICATE_USE_SKILL));
                        return false;
                    }
                    if (levelSkillBook - skillChar.Point != 1)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANT_YET_USE_SKILL));
                        return false;
                    }

                    var skillAdd =
                        skillTemplate.SkillDataTemplates.FirstOrDefault(option => option.Point == levelSkillBook);
                    skillChar.SkillId = skillAdd!.SkillId;
                    skillChar.CoolDown = 0;
                    skillChar.Point++;
                    character.CharacterHandler.SendMessage(Service.UpdateSkill((short)skillAdd.SkillId));
                    character.BoughtSkill.Add(item.Id);
                    return true;
                }

                if (character.BoughtSkill.Contains(item.Id))
                {
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DUPLICATE_USE_SKILL));
                    return false;
                }
                switch (itemTemplate.Skill)
                    {
                        case 21:
                        {
                            var skilCharCheck = character.Skills.FirstOrDefault(skill => skill.Id == 13);
                            if (skilCharCheck == null || skilCharCheck.Point < 7)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANT_YET_USE_SKILL));
                                return false;
                            }
                            break;
                        }
                        case 18:
                        {
                            var skilCharCheck = character.Skills.FirstOrDefault(skill => skill.Id == 12);
                            if (skilCharCheck == null || (skilCharCheck?.Point < 7))
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANT_YET_USE_SKILL));
                                return false;
                            }
                            break;
                        }
                        case 22:
                        {
                            var skilCharCheck = character.Skills.FirstOrDefault(skill => skill.Id == 9);
                            if (skilCharCheck == null || (skilCharCheck.Point < 7))
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANT_YET_USE_SKILL));
                                return false;
                            }
                            break;
                        }
                    }

                if (itemTemplate.Level == 1)
                {
                    var skillAdd =
                        skillTemplate.SkillDataTemplates.FirstOrDefault(option => option.Point == 1);
                    if (skillAdd == null)
                    {
                        return false;
                    }
                    character.Skills.Add(new SkillCharacter()
                    {
                        Id = skillTemplate.Id,
                        SkillId = skillAdd.SkillId,
                        CoolDown = 0,
                        Point = 1,
                    });
                    character.CharacterHandler.SendMessage(Service.AddSkill((short) skillAdd.SkillId));
                    character.BoughtSkill.Add(item.Id);
                    return true;
                }
                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANT_YET_USE_SKILL));
                return false; 
            }
        }

        private static void UseMayDoCapsuleKiBi(Model.Character.Character character, Model.Item.Item item)
        {
            var template = ItemCache.ItemTemplate(item.Id);
            character.InfoBuff.MayDoCSKB = true;
            character.InfoBuff.MayDoCSKBTime = ServerUtils.CurrentTimeMillis() + 1800000;
            character.CharacterHandler.SendMessage(Service.ItemTime(template.IconId, 1800));
            character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI, 1, reason:"Dùng máy dò");
            character.CharacterHandler.SendMessage(Service.SendBag(character));
        }

        private static void UseCapsuleHalloween(Model.Character.Character character, Model.Item.Item item)
        {
            try
            {
                character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI, 1, reason:"CSKB");
                var randomPercent = ServerUtils.RandomNumber(0, 100);
                if (randomPercent <= 0.3)
                {
                // - đồ kích hoạt 0.3%
                    var gender = character.InfoChar.Gender;
                    // TD hiếm Găng và RADAR 21,
                    var listItem = new List<short>(){0,6,27};
                    // Sôn gô ku hiếm 129
                    var listSKH = new List<int>(){127,128,127,128,129};

                    if (gender == 1)
                    {
                        // NM hiếm Giầy và Radar 28
                        listItem = new List<short>(){1,7,22};
                        // bộ picolo hiếm
                        listSKH = new List<int>(){131,132,130,131,132};
                    }
                    else if (gender == 2)
                    {
                        //XD hiếm quần và Radar 8
                        listItem = new List<short>(){2,23,29};
                        // bộ nappa hiếm 135
                        listSKH = new List<int>(){133,134,133,135,134};
                    }

                    var itemAdd = ItemCache.GetItemDefault(listItem[ServerUtils.RandomNumber(listItem.Count)]);
                    itemAdd.Quantity = 1;
                    var idSKH = listSKH[ServerUtils.RandomNumber(listSKH.Count)];
                    itemAdd.Options.Add(new OptionItem()
                    {
                        Id = idSKH,
                        Param = 0,
                    });
                    itemAdd.Options.Add(new OptionItem()
                    {
                        Id = LeaveItemHandler.GetSKHDescOption(idSKH),
                        Param = 0,
                    });
                    itemAdd.Options.Add(new OptionItem()
                    {
                        Id = 30,
                        Param = 0,
                    });

                    character.CharacterHandler.AddItemToBag(true, itemAdd, "CSKB");
                    var template = ItemCache.ItemTemplate(itemAdd.Id);
                    character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().CSKB_GET, template.Name)));

                } else if (randomPercent <= 1.0)
                {
                // - đồ thần linh 0.7%
                    var random = new Random();
                    int index = random.Next(DataCache.ListDoThanLinh.Count);
                    short idDoThanLinh = DataCache.ListDoThanLinh[index];
                    var itemAdd = ItemCache.GetItemDefault(idDoThanLinh);
                    itemAdd.Quantity = 1;
                    character.CharacterHandler.AddItemToBag(true, itemAdd, "CSKB");
                    var template = ItemCache.ItemTemplate(itemAdd.Id);
                    character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().CSKB_GET, template.Name)));

                } else if (randomPercent <= 5.0)
                {
                // - đá bảo vệ 4%
                    var itemAdd = ItemCache.GetItemDefault(987);
                    itemAdd.Quantity = 1;
                    character.CharacterHandler.AddItemToBag(true, itemAdd, "CSKB");
                    var template = ItemCache.ItemTemplate(itemAdd.Id);
                    character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().CSKB_GET, template.Name)));

                } else if (randomPercent <= 40.0)
                {
                // - ngọc rồng 7s-3s = 15%
                    var ListDo = new List<short>() { 16,17,18,19,20 };
                    var random = new Random();
                    int index = random.Next(ListDo.Count);
                    short idVatPham = ListDo[index];
                    var itemAdd = ItemCache.GetItemDefault(idVatPham);
                    itemAdd.Quantity = 1;
                    character.CharacterHandler.AddItemToBag(true, itemAdd, "CSKB");
                    var template = ItemCache.ItemTemplate(itemAdd.Id);
                    character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().CSKB_GET, template.Name)));

                } 
                // else if (randomPercent <= 40.0)
                // {
                // // - quần , giày , rada , găng = 20%
                //     var ListDo = new List<short>() { 0,1,2,3,4,5,33,34,41,42,49,50,
                //     136,137,138,139,152,153,154,155,168,169,170,171,230,231
                //         ,232,233,234,235,236,237,238,239,240,241,6,7,8,9,10,11,35,36,43,44,51,52,
                //     140,141,142,143,156,157,158,159,172,173,174,175,
                //     242,243,244,245,246,247,248,249,250,251,252,253,
                //     253,256,257,258,259,260,261,262,263,264,265,21,22,23,24,25,26,37,38,45,46,53,54,
                //     144,145,160,161,162,163,176,177,178,179,254,255,266,267,268,269,270,271,272,273,274,
                //     275,276,277,27,28,29,30,31,32,39,40,47,48,55,56,149,150,151,164,165,166,167,180,181,
                //     182,183,12,57,58,59 };
                //     var random = new Random();
                //     int index = random.Next(ListDo.Count);
                //     short idVatPham = ListDo[index];
                //     var itemAdd = ItemCache.GetItemDefault(idVatPham);
                //     itemAdd.Quantity = 1;
                //     character.CharacterHandler.AddItemToBag(true, itemAdd, "CSKB");
                //     var template = ItemCache.ItemTemplate(itemAdd.Id);
                //     character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().CSKB_GET, template.Name)));

                // } 
                else if (randomPercent <= 70.0)
                {
                // - sao pha lê các loại  = 30%
                    var random = new Random();
                    int index = random.Next(DataCache.ListSaoPhaLe.Count);
                    short idSaoPhaLe = DataCache.ListSaoPhaLe[index];
                    var itemAdd = ItemCache.GetItemDefault(idSaoPhaLe);
                    itemAdd.Quantity = 1;
                    character.CharacterHandler.AddItemToBag(true, itemAdd, "CSKB");
                    var template = ItemCache.ItemTemplate(itemAdd.Id);
                    character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().CSKB_GET, template.Name)));
                }
                else if (randomPercent <= 100.0)
                {
                // - đá nâng cấp = 30%
                    var random = new Random();
                    int index = random.Next(DataCache.ListDaNangCap.Count);
                    short idDaNangCap = DataCache.ListDaNangCap[index];
                    var itemAdd = ItemCache.GetItemDefault(idDaNangCap);
                    itemAdd.Quantity = 1;
                    character.CharacterHandler.AddItemToBag(true, itemAdd, "CSKB");
                    var template = ItemCache.ItemTemplate(itemAdd.Id);
                    character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().CSKB_GET, template.Name)));

                }
                character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                character.CharacterHandler.SendMessage(Service.SendBag(character));
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UseCapsuleHalloween in ItemHandler.cs: {e.Message} \n {e.StackTrace}", e);
                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().ERROR_SERVER));
            }
            
        }
        private static void UseCapsuleKiBi(Model.Character.Character character, Model.Item.Item item)
        {
            character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI, 1, reason:"CSKB");
            var randomPercent = ServerUtils.RandomNumber(0, 100);
            if (randomPercent <= 75)
            {
                var gold = ServerUtils.RandomNumber(20000, 1000000);
                var randomRate = ServerUtils.RandomNumber(0.0, 100.0);
                if (randomRate <= 1.0) 
                {
                    //1%
                    gold = 10000000;
                }
                else if (randomRate <= 2.0) 
                {
                    //1%
                    gold = 8000000;
                }
                else if (randomRate <= 3.0)
                {
                    // 1%
                    gold = 5000000;
                }
                else if (randomRate <= 5.0)
                {
                    //2%
                    gold = 3000000;
                }
                else if (randomRate <= 30)
                {
                    gold = ServerUtils.RandomNumber(20000, 2000000);
                }
                character.PlusGold(gold);
                character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().SELL_ITEM_GOLD, ServerUtils.GetMoneys(gold))));
            }
            else if (randomPercent <= 80) // an danh
            {
                var itemAdd = ItemCache.GetItemDefault(385);
                itemAdd.Quantity = 1;
                character.CharacterHandler.AddItemToBag(true, itemAdd, "CSKB");
                var template = ItemCache.ItemTemplate(itemAdd.Id);
                character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().CSKB_GET, template.Name)));
            }
            else if (randomPercent <= 85) //giap xen
            {
                var itemAdd = ItemCache.GetItemDefault(384);
                itemAdd.Quantity = 1;
                character.CharacterHandler.AddItemToBag(true, itemAdd, "CSKB");
                var template = ItemCache.ItemTemplate(itemAdd.Id);
                character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().CSKB_GET, template.Name)));
            }
            else if (randomPercent <= 90) //bo khi
            {
                var itemAdd = ItemCache.GetItemDefault(383);
                itemAdd.Quantity = 1;
                character.CharacterHandler.AddItemToBag(true, itemAdd, "CSKB");
                var template = ItemCache.ItemTemplate(itemAdd.Id);
                character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().CSKB_GET, template.Name)));
            }
            else if (randomPercent <= 95) //bo huyet
            {
                var itemAdd = ItemCache.GetItemDefault(382);
                itemAdd.Quantity = 1;
                character.CharacterHandler.AddItemToBag(true, itemAdd, "CSKB");
                var template = ItemCache.ItemTemplate(itemAdd.Id);
                character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().CSKB_GET, template.Name)));
            }
            else if (randomPercent <= 100) //cuong no
            {
                var itemAdd = ItemCache.GetItemDefault(381);
                itemAdd.Quantity = 1;
                character.CharacterHandler.AddItemToBag(true, itemAdd, "CSKB");
                var template = ItemCache.ItemTemplate(itemAdd.Id);
                character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().CSKB_GET, template.Name)));
            }
            
            character.CharacterHandler.SendMessage(Service.SendBag(character));
        }

        private static void UseCapsuleTrungThu(Model.Character.Character character, Model.Item.Item item)
        {
            if (character.CharacterHandler.GetItemBagById(737) == null) return;
            character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI, 1, reason:"CSTT");
            var tile = ServerUtils.RandomNumber(100);
            //50 % vàng, 20% nr, 20% item ngẫu nhiên, 8% cải trang, 2% cải trang v.v
            if (tile < 50)
            {
                var gold = ServerUtils.RandomNumber(50000000,80000000);
                character.CharacterHandler.SendMessage(
                    Service.ServerMessage(string.Format($"Bạn nhận được {ServerUtils.GetMoney(gold)} vàng")));
                character.PlusGold(gold);
                character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
            } 
            else if (tile < 70)
            {
                var listitem = new List<short>() {16,16,17,17,17,18,18,18,18,19,19,19,19,19}; // ngoc rồng
                var itemrand = listitem[ServerUtils.RandomNumber(listitem.Count)];
                var itemAdd = ItemCache.GetItemDefault(itemrand);
                var temp = ItemCache.ItemTemplate(itemAdd.Id);
                character.CharacterHandler.AddItemToBag(true, itemAdd, "CSTT");
                character.CharacterHandler.SendMessage(
                    Service.ServerMessage(string.Format(TextServer.gI().ADD_ITEM,
                        $"{temp.Name}")));
            } 
            else if (tile < 90)
            {
                var listitem = new List<short>() {467,468,469,470,471,800,801,802,803,804,733,734,735,993,998,999,1000,1001}; // item ngẫu nhiên
                var itemrand = listitem[ServerUtils.RandomNumber(listitem.Count)];
                var itemAdd = ItemCache.GetItemDefault(itemrand);
                var timeServer = ServerUtils.CurrentTimeSecond();
                var expireDay = ServerUtils.RandomNumber(2,5);
                var expireTime = timeServer + (expireDay*86400);
                itemAdd.Options.Add(new OptionItem()
                {
                    Id = 93,
                    Param = expireDay
                });

                var optionHiden = itemAdd.Options.FirstOrDefault(option => option.Id == 73);
                if (optionHiden != null) 
                {
                    optionHiden.Param = expireTime;
                }
                else 
                {
                    itemAdd.Options.Add(new OptionItem()
                    {
                        Id = 73,
                        Param = expireTime,
                    });
                }

                itemAdd.Options.Add(new OptionItem()
                {
                    Id = 30,
                    Param = 0,
                });

                character.CharacterHandler.AddItemToBag(true, itemAdd, "CSTT");

                var temp = ItemCache.ItemTemplate(itemAdd.Id);
                character.CharacterHandler.SendMessage(
                    Service.ServerMessage(string.Format(TextServer.gI().ADD_ITEM,
                        $"{temp.Name}")));
            }
            else
            {
                var listitem = new List<short>() {463,464}; // item ngẫu nhiên
                var itemrand = listitem[ServerUtils.RandomNumber(listitem.Count)];
                var itemAdd = ItemCache.GetItemDefault(itemrand);

                if (tile < 98)
                {
                    var timeServer = ServerUtils.CurrentTimeSecond();
                    var expireDay = ServerUtils.RandomNumber(2,3);
                    var expireTime = timeServer + (expireDay*86400);
                    itemAdd.Options.Add(new OptionItem()
                    {
                        Id = 93,
                        Param = expireDay
                    });

                    var optionHiden = itemAdd.Options.FirstOrDefault(option => option.Id == 73);
                    if (optionHiden != null) 
                    {
                        optionHiden.Param = expireTime;
                    }
                    else 
                    {
                        itemAdd.Options.Add(new OptionItem()
                        {
                            Id = 73,
                            Param = expireTime,
                        });
                    }
                }

                itemAdd.Options.Add(new OptionItem()
                {
                    Id = 30,
                    Param = 0,
                });

                character.CharacterHandler.AddItemToBag(true, itemAdd, "CSTT");

                var temp = ItemCache.ItemTemplate(itemAdd.Id);
                character.CharacterHandler.SendMessage(
                    Service.ServerMessage(string.Format(TextServer.gI().ADD_ITEM,
                        $"{temp.Name}")));
            }
            character.CharacterHandler.SendMessage(Service.SendBag(character));
        }

        private static void UseBuffItem(Model.Character.Character character, Model.Item.Item item)
        {
            switch(item.Id)
            {
                case 381:
                {
                    character.InfoBuff.CuongNo = true;
                    character.InfoBuff.CuongNoTime = ServerUtils.CurrentTimeMillis() + 600000;
                    break;
                }
                case 382:
                {
                    character.InfoBuff.BoHuyet = true;
                    character.InfoBuff.BoHuyetTime = ServerUtils.CurrentTimeMillis() + 600000;
                    break;
                }
                case 383:
                {
                    character.InfoBuff.BoKhi = true;
                    character.InfoBuff.BoKhiTime = ServerUtils.CurrentTimeMillis() + 600000;
                    break;
                }
                case 384:
                {
                    character.InfoBuff.GiapXen = true;
                    character.InfoBuff.GiapXenTime = ServerUtils.CurrentTimeMillis() + 600000;
                    break;
                }
                case 385:
                {
                    character.InfoBuff.AnDanh = true;
                    character.InfoBuff.AnDanhTime = ServerUtils.CurrentTimeMillis() + 600000;
                    break;
                }
                case 462:
                {
                    character.InfoBuff.CuCarot = true;
                    character.InfoBuff.CuCarotTime = ServerUtils.CurrentTimeMillis() + 600000;
                    break;
                }
            }
            var template = ItemCache.ItemTemplate(item.Id);
            // character.CharacterHandler.SendMessage(Service.ItemTime(template.IconId, 600));
            character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI, 1, reason:"Dùng buff");
            character.CharacterHandler.SendMessage(Service.SendBag(character));
            character.CharacterHandler.SetUpInfo();
            character.CharacterHandler.SendMessage(Service.MeLoadPoint(character));
        }

        private static void UseBanhTrungThu(Model.Character.Character character, Model.Item.Item item)
        {
            if (character.CharacterHandler.GetItemBagById(891) == null) return;
            character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI, 1, reason:"Ăn bánh tt");
            var tile = ServerUtils.RandomNumber(100);
            //50 % vàng,20% nr, 10% item ngẫu nhiên, 20% không ra gì
            if (tile < 50)
            {
                var gold = ServerUtils.RandomNumber(10000000,12000000);
                character.CharacterHandler.SendMessage(
                    Service.ServerMessage(string.Format($"Bạn nhận được {ServerUtils.GetMoney(gold)} vàng")));
                character.PlusGold(gold);
                character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
            } else if (tile < 65)
            {
                var listitem = new List<short>() {16,16,17,17,17,18,18,18,18,19,19,19,19,19}; // ngoc rồng
                var itemrand = listitem[ServerUtils.RandomNumber(listitem.Count)];
                var itemAdd = ItemCache.GetItemDefault(itemrand);
                var temp = ItemCache.ItemTemplate(itemAdd.Id);
                character.CharacterHandler.AddItemToBag(true, itemAdd, "CSTT");
                character.CharacterHandler.SendMessage(
                    Service.ServerMessage(string.Format(TextServer.gI().ADD_ITEM,
                        $"{temp.Name}")));
            } else if (tile < 75)
            {
                var listitem = new List<short>() {467,468,469,470,471,800,801,802,803,804,733,734,735}; // item ngẫu nhiên
                var itemrand = listitem[ServerUtils.RandomNumber(listitem.Count)];
                var itemAdd = ItemCache.GetItemDefault(itemrand);
                var timeServer = ServerUtils.CurrentTimeSecond();
                var expireDay = ServerUtils.RandomNumber(2,3);
                var expireTime = timeServer + (expireDay*86400);
                itemAdd.Options.Add(new OptionItem()
                {
                    Id = 93,
                    Param = expireDay
                });

                var optionHiden = itemAdd.Options.FirstOrDefault(option => option.Id == 73);
                if (optionHiden != null) 
                {
                    optionHiden.Param = expireTime;
                }
                else 
                {
                    itemAdd.Options.Add(new OptionItem()
                    {
                        Id = 73,
                        Param = expireTime,
                    });
                }

                itemAdd.Options.Add(new OptionItem()
                {
                    Id = 30,
                    Param = 0,
                });

                character.CharacterHandler.AddItemToBag(true, itemAdd, "CSTT");

                var temp = ItemCache.ItemTemplate(itemAdd.Id);
                character.CharacterHandler.SendMessage(
                    Service.ServerMessage(string.Format(TextServer.gI().ADD_ITEM,
                        $"{temp.Name}")));
            }
            character.DiemSuKien += 1;
            character.CharacterHandler.SendMessage(
                Service.ServerMessage(string.Format("Bạn nhận được 1 điểm sự kiện trung thu")));
            character.CharacterHandler.SendMessage(Service.SendBag(character));
        }

        private static void UseThucAn(Model.Character.Character character, Model.Item.Item item)
        {
            // Nếu chưa có thức ăn, thì set thời gian
            if (character.InfoBuff.ThucAnId != -1)
            {
                var oldTemplate = ItemCache.ItemTemplate(character.InfoBuff.ThucAnId);
                character.CharacterHandler.SendMessage(Service.ItemTime(oldTemplate.IconId, 0));
            }
            // Nếu đã có thức ăn thì xóa item thức ăn cũ.
            var template = ItemCache.ItemTemplate(item.Id);
            character.InfoBuff.ThucAnId = item.Id;
            character.InfoBuff.ThucAnTime = ServerUtils.CurrentTimeMillis() + 600000;
            // character.CharacterHandler.SendMessage(Service.ItemTime(template.IconId, 600));
            character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI, 1, reason:"Ăn thức ăn");
            character.CharacterHandler.SendMessage(Service.SendBag(character));
            character.CharacterHandler.SetUpInfo();
            character.CharacterHandler.SendMessage(Service.MeLoadPoint(character));
        }

        private static void UseBanhTrungThuBuff(Model.Character.Character character, Model.Item.Item item)
        {
            // Nếu chưa có thức ăn, thì set thời gian
            if (character.InfoBuff.BanhTrungThuId != -1)
            {
                var oldTemplate = ItemCache.ItemTemplate(character.InfoBuff.BanhTrungThuId);
                character.CharacterHandler.SendMessage(Service.ItemTime(oldTemplate.IconId, 0));
            }
            // Nếu đã có thức ăn thì xóa item thức ăn cũ.
            var template = ItemCache.ItemTemplate(item.Id);
            character.InfoBuff.BanhTrungThuId = item.Id;
            character.InfoBuff.BanhTrungThuTime = ServerUtils.CurrentTimeMillis() + 3600000;
            // character.CharacterHandler.SendMessage(Service.ItemTime(template.IconId, 600));
            character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI, 1, reason:"Ăn bánh tt buff");
            character.CharacterHandler.SendMessage(Service.SendBag(character));
            character.CharacterHandler.SetUpInfo();
            character.CharacterHandler.SendMessage(Service.MeLoadPoint(character));
        }

        public static void UseAuraItem(Model.Character.Character character, Model.Item.Item item)
        {
            // Tạo vệ tinh ngoài đất
            var itemMap = new ItemMap(-2, item);
            itemMap.AuraPlayerId = character.Id;
            itemMap.X = character.InfoChar.X;
            itemMap.Y = character.InfoChar.Y;
            itemMap.R = 200;
            character.Zone.ZoneHandler.LeaveItemMap(itemMap);
            character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI, 1, reason:"Dùng vệ tinh");
            character.CharacterHandler.SendMessage(Service.SendBag(character));
        }

        private static void UseDragonBall(Model.Character.Character character, Model.Item.Item item)
        {
            var timeServer = ServerUtils.CurrentTimeMillis();

            if (MapManager.delayCallDragon > timeServer)
            {
                var delay = (MapManager.delayCallDragon - timeServer) / 1000;
                if (delay < 1)
                {
                    delay = 1;
                }

                character.CharacterHandler.SendMessage(
                    Service.ServerMessage(string.Format(TextServer.gI().DELAY_CALL_DRAGON_SEC,
                        delay)));
                return;
            }

            // if (MapManager.IsDragonHasAppeared())
            // {
            //     character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().RONG_THAN_DANG_XUAT_HIEN));
            //     return;
            // }

            if (character.InfoChar.CountGoiRong >= DataCache.LIMIT_SO_LAN_GOI_RONG[(character.InfoChar.IsPremium ? 1 : 0)])
            {
                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().LIMIT_GOI_RONG));
                return;
            }
            // Kiểm tra có đứng ở đúng làng không
            if ((character.InfoChar.Gender == 0 && character.InfoChar.MapId != 0) ||
                (character.InfoChar.Gender == 1 && character.InfoChar.MapId != 7) ||
                (character.InfoChar.Gender == 2 && character.InfoChar.MapId != 14))
            {
                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().INVALID_PLACE_CALL_DRAGON));
                return;
            }
            
            // kiểm tra thử xem đủ 7 viên ngọc rồng không
            for (int dball = 14; dball <= 20; dball++)
            {
                if (character.CharacterHandler.GetItemBagById(dball) == null)
                {
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_GENDER));
                    return;
                }
            }
            // Xóa 7 viên ngọc và gọi rồng thần
            for (short dball = 14; dball <= 20; dball++)
            {
                character.CharacterHandler.RemoveItemBagById(dball, 1, reason:"Gọi rồng");
            }
            // Gọi rồng thần
            MapManager.delayCallDragon = timeServer + 180000;
            MapManager.SetDragonAppeared(true);
            character.InfoChar.CountGoiRong++;
            character.InfoMore.VuaGoiRong = true;
            character.CharacterHandler.SendMessage(Service.SendBag(character));
            // character.Zone.ZoneHandler.SendMessage(Service.CallDragon(0, 0, character));
            character.CharacterHandler.SendMessage(Service.CallDragon(0, 0, character));
            // Thread.Sleep(3000);
            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(24, MenuNpc.Gi().TextRongThan, MenuNpc.Gi().MenuDieuUocRongThan, 3));
        }

        private static void UsePetItem(Model.Character.Character character, Model.Item.Item item)
        {
            if (character.Zone.Map.IsMapCustom())
            {
                character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format("Không thể sử dụng linh thú tại đây")));
                return;
            }

            var petImeiOption = item.Options.FirstOrDefault(option => option.Id == 73);

            if (petImeiOption == null)
            {
                character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format("Linh thú đã bị lỗi, vui lòng liên hệ Admin")));
                return;
            }

            var pet = character.Pet;
            
            if (pet != null) 
            {
                if (petImeiOption.Param == character.InfoChar.PetImei && item.Id == character.InfoChar.PetId)
                {
                    // Cất pet
                    character.Zone.ZoneHandler.RemovePet(pet);
                    character.InfoChar.PetId = -1;
                    character.InfoChar.PetImei = -1;
                    character.InfoMore.PetItemIndex = -1;
                    character.Pet = null;
                    character.CharacterHandler.SetUpInfo();
                }
                else 
                {
                    character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format("Bạn đang có linh thú bên ngoài")));
                }
                return;
            }

            pet = new Pet(item.Id, character);
            character.Pet = pet;
            character.Zone.ZoneHandler.AddPet(pet);

            character.InfoChar.PetId = item.Id;
            character.InfoChar.PetImei = petImeiOption.Param;
            character.InfoMore.PetItemIndex = item.IndexUI;
            character.CharacterHandler.SetUpInfo();
        }

        private static SkillTemplate UseSkill(Model.Character.Character character, Model.Item.Item item)
        {
            var itemTemplate = ItemCache.ItemTemplate(item.Id);
            if (itemTemplate.Gender != character.InfoChar.Gender && itemTemplate.Gender != 3)
            {
                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DONOT_USE_SKILL));
                return null;
            }

            var skillTemplate = Cache.Gi().SKILL_TEMPLATES.FirstOrDefault(skill => skill.Id == itemTemplate.Skill);
            if (skillTemplate == null) return null;
            {
                var levelSkillBook = itemTemplate.Level;
                var skillChar = character.Skills.FirstOrDefault(skill => skill.Id == skillTemplate.Id);
                if (skillChar != null)
                {
                    if (levelSkillBook <= skillChar.Point)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DUPLICATE_USE_SKILL));
                        return null;
                    }
                    if (levelSkillBook - skillChar.Point != 1)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANT_YET_USE_SKILL));
                        return null;
                    }
                    return skillTemplate;
                }

                if (character.BoughtSkill.Contains(item.Id))
                {
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DUPLICATE_USE_SKILL));
                    return null;
                }
                switch (itemTemplate.Skill)
                    {
                        case 21:
                        {
                            var skilCharCheck = character.Skills.FirstOrDefault(skill => skill.Id == 13);
                            if (skilCharCheck == null || skilCharCheck.Point < 7)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANT_YET_USE_SKILL));
                                return null;
                            }
                            break;
                        }
                        case 18:
                        {
                            var skilCharCheck = character.Skills.FirstOrDefault(skill => skill.Id == 12);
                            if (skilCharCheck == null || (skilCharCheck?.Point < 7))
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANT_YET_USE_SKILL));
                                return null;
                            }
                            break;
                        }
                        case 22:
                        {
                            var skilCharCheck = character.Skills.FirstOrDefault(skill => skill.Id == 9);
                            if (skilCharCheck == null || (skilCharCheck.Point < 7))
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANT_YET_USE_SKILL));
                                return null;
                            }
                            break;
                        }
                    }

                if (itemTemplate.Level == 1)
                {
                    return skillTemplate;
                }
                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANT_YET_USE_SKILL));
                return null; 
            }
        }

        public static void AddLearnSkill(Model.Character.Character character, Model.Item.Item item, SkillTemplate skillTemplate)
        {
            var itemTemplate = ItemCache.ItemTemplate(item.Id);
            var levelBook = itemTemplate.Level;
            var skillChar = character.Skills.FirstOrDefault(skill => skill.Id == skillTemplate.Id);
            if (skillChar != null)
            {
                if (levelBook <= skillChar.Point) return;
                if (levelBook - skillChar.Point != 1) return;
                var skillAdd =
                    skillTemplate.SkillDataTemplates.FirstOrDefault(option => option.Point == levelBook);
                skillChar.SkillId = skillAdd!.SkillId;
                skillChar.CoolDown = 0;
                skillChar.Point++;
                character.CharacterHandler.SendMessage(Service.UpdateSkill((short)skillAdd.SkillId));
                character.BoughtSkill.Add(item.Id);
            }
            else
            {
                var skillAdd =
                    skillTemplate.SkillDataTemplates.FirstOrDefault(option => option.Point == 1);
                if (skillAdd == null) return;
                character.Skills.Add(new SkillCharacter()
                {
                    Id = skillTemplate.Id,
                    SkillId = skillAdd.SkillId,
                    CoolDown = 0,
                    Point = 1,
                });
                character.CharacterHandler.SendMessage(Service.AddSkill((short) skillAdd.SkillId));
                character.BoughtSkill.Add(item.Id);
            }
        }

        public static void UseCard(Model.Character.Character character, Model.Item.Item item, bool isBox = false)
        {
            var radarTemplate = Cache.Gi().RADAR_TEMPLATE.FirstOrDefault(r => r.Id == item.Id);
            if(radarTemplate == null) return;
            // kiểm tra require ở đây
            if (radarTemplate.Require != -1)
            {
                var radarRequireTemplate = Cache.Gi().RADAR_TEMPLATE.FirstOrDefault(r => r.Id == radarTemplate.Require); 
                if(radarRequireTemplate == null) return;
                var cardRequire = character.InfoChar.Cards.GetValueOrDefault(radarTemplate.Require);  
                if (cardRequire == null || cardRequire.Level < radarTemplate.RequireLevel)
                {
                    character.CharacterHandler.SendMessage(Service.DialogMessage(string.Format(TextServer.gI().RADAR_REQUIRE, radarRequireTemplate.Name, radarTemplate.RequireLevel)));
                    return;
                }

            }
            var card = character.InfoChar.Cards.GetValueOrDefault(item.Id);
            if (card == null)
            {
                var newCard = new Card()
                {
                    Id = item.Id,
                    Amount = 1,
                    MaxAmount = radarTemplate.Max,
                    Level = -1,
                    Options = radarTemplate.Options.Copy()
                };
                if (character.InfoChar.Cards.TryAdd(newCard.Id, newCard))
                {
                    character.CharacterHandler.SendMessage(Service.Radar3(newCard.Id, newCard.Amount, newCard.MaxAmount));
                    character.CharacterHandler.SendMessage(Service.Radar2(newCard.Id, newCard.Level));
                    if (isBox)
                    {
                        character.CharacterHandler.RemoveItemBoxByIndex(item.IndexUI, 1);
                        character.CharacterHandler.SendMessage(Service.SendBox(character));
                    }
                    else
                    {
                        character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI, 1, reason:"Dùng thẻ sưu tầm");
                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                    }
                }
            }
            else
            {
                if (card.Level >= 2)
                {
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().LEVEL_CARD_MAX));
                    return;
                }
                card.Amount++;
                if (card.Amount >= card.MaxAmount)
                {
                    card.Amount = 0;
                    if (card.Level == -1)
                    {
                        card.Level = 1;
                    }
                    else 
                    {
                        card.Level++;
                    }
                    character.CharacterHandler.SetUpInfo();
                    character.CharacterHandler.SendMessage(Service.MeLoadPoint(character));
                    character.CharacterHandler.SendMessage(Service.Radar2(card.Id, card.Level));
                }
                character.CharacterHandler.SendMessage(Service.Radar3(card.Id, card.Amount, card.MaxAmount));
                if (isBox)
                {
                    character.CharacterHandler.RemoveItemBoxByIndex(item.IndexUI, 1);
                    character.CharacterHandler.SendMessage(Service.SendBox(character));
                }
                else
                {
                    character.CharacterHandler.RemoveItemBagByIndex(item.IndexUI, 1, reason:"Dùng thẻ sưu tầm");
                    character.CharacterHandler.SendMessage(Service.SendBag(character));
                }
            }
        }
    }
}
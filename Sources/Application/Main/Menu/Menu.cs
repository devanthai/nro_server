using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Handlers.Item;
using NRO_Server.Application.IO;
using NRO_Server.Application.Manager;
using NRO_Server.Application.Map;
using NRO_Server.Application.Threading;
using NRO_Server.DatabaseManager;
using NRO_Server.DatabaseManager.Player;
using NRO_Server.Main.Menu;
using NRO_Server.Model.Character;
using NRO_Server.Model.Clan;
using NRO_Server.Model.Info;
using NRO_Server.Model.Option;
using NRO_Server.Model.Template;
using NRO_Server.Model.SkillCharacter;
using Org.BouncyCastle.Math.Field;
using NRO_Server.Application.Interfaces.Character;

namespace NRO_Server.Application.Main.Menu
{
    public static class Menu
    {
        public static void OpenUiMenu(short npcId, Character character)
        {
            Server.Gi().Logger.Debug($"Menu NpcId Case 33: ------------------------------------ {npcId}");
            try
            {
                switch (npcId)
                {
                    //3 ông già
                    case 0:
                    case 1:
                    case 2:
                    {
                        switch (character.InfoChar.Task.Id)
                        {
                            case 0:
                            {
                                break;
                            }
                            case 1:
                            {
                                break;
                            }
                            case 2:
                            {
                                break;
                            }
                            default:
                            {
                                // character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, MenuNpc.Gi().TextBaOngGia[0]));
                                //
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextBaOngGia[0], MenuNpc.Gi().MenuBaOngGia[0], character.InfoChar.Gender));
                                character.TypeMenu = 0;
                                break;
                            }
                        }

                        break;
                    }
                    //Rương đồ
                    case 3:
                    {
                        character.CharacterHandler.SendMessage(Service.SendBox(character, 1));
                        break;
                    }
                    //Đậu thần
                    case 4:
                    {
                        var magicTree = MagicTreeManager.Get(character.Id);
                        if (magicTree == null) return;
                        var ngoc = magicTree.Diamond;
                        if (magicTree.IsUpdate)
                        {
                            character.CharacterHandler.SendMessage(Service.MagicTree1(new List<string>() {$"Nâng cấp\nnhanh\n{ngoc} ngọc", "Huỷ\nnâng cáp"}));
                        }
                        else
                        {
                            if (magicTree.Peas == magicTree.MaxPea)
                            {
                                character.CharacterHandler.SendMessage(Service.MagicTree1(new List<string>()
                                    {"Thu hoạch", $"Nâng cấp\n{ServerUtils.ConvertMilisecond(DataCache.UpgradeDauThanTime[magicTree.Level - 1])}\n{ServerUtils.GetMoney(DataCache.UpgradeDauThanGold[magicTree.Level - 1])}\nvàng"}));
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(Service.MagicTree1(new List<string>() {"Thu hoạch", $"Nâng cấp\n{ServerUtils.ConvertMilisecond(DataCache.UpgradeDauThanTime[magicTree.Level - 1])}\n300 Tr\nvàng", $"Kết hạt\nnhanh\n{ngoc} ngọc"}));
                            }
                        }
                        break;
                    }
                    //Bumma
                    case 7:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextBumma[0], MenuNpc.Gi().MenuShopDistrict[0], character.InfoChar.Gender));
                        character.TypeMenu = 0;
                        break;
                    }
                    //Dende
                    case 8:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextDende[0], MenuNpc.Gi().MenuShopDistrict[0], character.InfoChar.Gender));
                        character.TypeMenu = 0;
                        break;
                    }
                    //Appule
                    case 9:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextAppule[0], MenuNpc.Gi().MenuShopDistrict[0], character.InfoChar.Gender));
                        character.TypeMenu = 0;
                        break;
                    }
                    //Brief
                    case 10:
                    {
                        character.CharacterHandler.SendMessage(character.InfoChar.MapId == 84
                            ? Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextBrief[1],
                                new List<string>()
                                {
                                    character.InfoChar.Gender != 0
                                        ? character.InfoChar.Gender != 1 ? "Về Xayda" : "Về Namếc"
                                        : "Về\nTrái Đất"
                                }, character.InfoChar.Gender)
                            : Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextBrief[0], MenuNpc.Gi().MenuBrief[0],
                                character.InfoChar.Gender));
                        break;
                    }
                    //Cargo
                    case 11:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextCargo[0], MenuNpc.Gi().MenuCargo[0], character.InfoChar.Gender));
                        break;
                    }
                    //Cui
                    case 12:
                    {
                        switch (character.InfoChar.MapId)
                        {
                            case 19:
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextCui[1], MenuNpc.Gi().MenuCui[2], character.InfoChar.Gender));
                                break;
                            case 68:
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextCui[2], MenuNpc.Gi().MenuCui[3], character.InfoChar.Gender));
                                break;
                            default:
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextCui[0], MenuNpc.Gi().MenuCui[0], character.InfoChar.Gender));
                                break;
                        }

                        break;
                    }
                    //Quy lão
                    case 13:
                    {
                        if (character.InfoChar.LearnSkill != null)
                        {
                            var itemAdd = character.InfoChar.LearnSkill.ItemSkill;
                            var time = character.InfoChar.LearnSkill.Time - ServerUtils.CurrentTimeMillis();
                            var skillTemplate = Cache.Gi().SKILL_TEMPLATES.FirstOrDefault(skill => skill.Id == character.InfoChar.LearnSkill.ItemTemplateSkillId);

                            if (character.InfoChar.LearnSkill.Time <= ServerUtils.CurrentTimeMillis())
                            {
                                ItemHandler.AddLearnSkill(character, itemAdd, skillTemplate);
                                character.InfoChar.LearnSkill = null;
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[0], MenuNpc.Gi().MenuQuyLao[0], character.InfoChar.Gender));
                                character.TypeMenu = 0;
                            }
                            else
                            {
                                var itemTempalte = ItemCache.ItemTemplate(itemAdd.Id);
                                var ngoc = 5;
                                if (time / 600000 >= 2)
                                {
                                    ngoc += (int)time / 600000;
                                }

                                var menu = string.Format(TextServer.gI().ADDING_SKILL, skillTemplate.Name,
                                    itemTempalte.Level, ServerUtils.GetTime(time));
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, menu, new List<string>() {$"Học\nCấp tốc\n{ngoc} ngọc", "Huỷ","Bỏ qua"}, character.InfoChar.Gender));
                                character.TypeMenu = 3;
                            }
                        }
                        else
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[0], MenuNpc.Gi().MenuQuyLao[0], character.InfoChar.Gender));
                            character.TypeMenu = 0;
                        }
                        break;
                    }
                    //Trưởng lão Guru
                    case 14:
                    {
                        if (character.InfoChar.Gender != 1)
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Nơi đây chỉ dành cho những chiến binh Namếc, hãy về hành tinh của mình đi."));
                            return;
                        }
                        if (character.InfoChar.LearnSkill != null)
                        {
                            var itemAdd = character.InfoChar.LearnSkill.ItemSkill;
                            var time = character.InfoChar.LearnSkill.Time - ServerUtils.CurrentTimeMillis();
                            var skillTemplate = Cache.Gi().SKILL_TEMPLATES.FirstOrDefault(skill => skill.Id == character.InfoChar.LearnSkill.ItemTemplateSkillId);

                            if (character.InfoChar.LearnSkill.Time <= ServerUtils.CurrentTimeMillis())
                            {
                                ItemHandler.AddLearnSkill(character, itemAdd, skillTemplate);
                                character.InfoChar.LearnSkill = null;
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[1], MenuNpc.Gi().MenuQuyLao[1], character.InfoChar.Gender));
                                character.TypeMenu = 0;
                            }
                            else
                            {
                                var itemTempalte = ItemCache.ItemTemplate(itemAdd.Id);
                                var ngoc = 5;
                                if (time / 600000 >= 2)
                                {
                                    ngoc += (int)time / 600000;
                                }

                                var menu = string.Format(TextServer.gI().ADDING_SKILL, skillTemplate.Name,
                                    itemTempalte.Level, ServerUtils.GetTime(time));
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, menu, new List<string>() {$"Học\nCấp tốc\n{ngoc} ngọc", "Huỷ","Bỏ qua"}, character.InfoChar.Gender));
                                character.TypeMenu = 2;
                            }
                        }
                        else
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[1], MenuNpc.Gi().MenuQuyLao[1], character.InfoChar.Gender));
                            character.TypeMenu = 0;
                        }
                        break;
                    }
                    //Vua vegeta
                    case 15:
                    {
                        if (character.InfoChar.Gender != 2)
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Nơi đây chỉ dành cho những chiến binh Xayda, hãy về hành tinh của mình đi."));
                            return;
                        }
                        if (character.InfoChar.LearnSkill != null)
                        {
                            var itemAdd = character.InfoChar.LearnSkill.ItemSkill;
                            var time = character.InfoChar.LearnSkill.Time - ServerUtils.CurrentTimeMillis();
                            var skillTemplate = Cache.Gi().SKILL_TEMPLATES.FirstOrDefault(skill => skill.Id == character.InfoChar.LearnSkill.ItemTemplateSkillId);

                            if (character.InfoChar.LearnSkill.Time <= ServerUtils.CurrentTimeMillis())
                            {
                                ItemHandler.AddLearnSkill(character, itemAdd, skillTemplate);
                                character.InfoChar.LearnSkill = null;
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[1], MenuNpc.Gi().MenuQuyLao[1], character.InfoChar.Gender));
                                character.TypeMenu = 0;
                            }
                            else
                            {
                                var itemTempalte = ItemCache.ItemTemplate(itemAdd.Id);
                                var ngoc = 5;
                                if (time / 600000 >= 2)
                                {
                                    ngoc += (int)time / 600000;
                                }

                                var menu = string.Format(TextServer.gI().ADDING_SKILL, skillTemplate.Name,
                                    itemTempalte.Level, ServerUtils.GetTime(time));
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, menu, new List<string>() {$"Học\nCấp tốc\n{ngoc} ngọc", "Huỷ","Bỏ qua"}, character.InfoChar.Gender));
                                character.TypeMenu = 2;
                            }
                        }
                        else
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[1], MenuNpc.Gi().MenuQuyLao[1], character.InfoChar.Gender));
                            character.TypeMenu = 0;
                        }
                        break;
                    }
                    //Uron
                    case 16:
                    {
                        var idShop = 15 + character.InfoChar.Gender;
                        character.CharacterHandler.SendMessage(Service.Shop(character, 0, idShop));
                        character.ShopId = idShop;
                        character.TypeMenu = 0;
                        break;
                    }
                    //Thần mèo
                    // case 18:
                    // {
                    //     character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextThanMeo[0], MenuNpc.Gi().MenuThanMeo[0], character.InfoChar.Gender));
                    //     character.TypeMenu = 0;
                    //     break;
                    // }
                    //Thượng đế
                    case 19:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextThuongDe[1], MenuNpc.Gi().MenuThuongDe[1], character.InfoChar.Gender));
                        character.TypeMenu = 0;
                        break;
                    }
                    case 20:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextThanVuTru[0], MenuNpc.Gi().MenuThanVuTru[0], character.InfoChar.Gender));
                        character.TypeMenu = 0;
                        break;
                    }
                    //Bà hạt mít
                    case 21:
                    {
                        switch (character.InfoChar.MapId)
                        {
                            case 5:
                            {
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextBaHatMit[0], MenuNpc.Gi().MenuBaHatMit[3], character.InfoChar.Gender));
                                character.TypeMenu = 1;
                                break;
                            }
                            case 42:
                            case 43:
                            case 44:
                            case 84:
                            {
                                List<string> menuBaHatMit = new List<string>();
                                var bongTaiPorata2 = character.CharacterHandler.GetItemBagById(921);

                                menuBaHatMit = MenuNpc.Gi().MenuBaHatMit[(character.InfoChar.IsNhanBua ? 0 : 1)];

                                if (bongTaiPorata2 != null)
                                {
                                    menuBaHatMit[(character.InfoChar.IsNhanBua ? 5 : 4)] = "Mở chỉ số\nBông tai\nPorata cấp 2";
                                }

                                character.CharacterHandler.SendMessage(
                                    Service
                                        .OpenUiConfirm(npcId, MenuNpc.Gi().TextBaHatMit[0], menuBaHatMit, character.InfoChar.Gender));
                                character.TypeMenu = 0;
                                break;
                            }
                            case 46:
                            {
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextBaHatMit[0], MenuNpc.Gi().MenuBaHatMit[15], character.InfoChar.Gender));
                                character.TypeMenu = 14;
                                break;
                            }
                        }

                        break;
                    }
                    //Bumma TL
                    case 37:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextBumma[0], MenuNpc.Gi().MenuShopDistrict[1], character.InfoChar.Gender));
                        character.TypeMenu = 0;
                        break;
                    }
                    // Ca lích
                    case 38:
                    {
                        switch (character.InfoChar.MapId)
                        {
                            case 28:
                            {
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextCalich[0], MenuNpc.Gi().MenuCalich[0], character.InfoChar.Gender));
                                character.TypeMenu = 0;
                                break;
                            }
                            case 102:
                            {
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextCalich[0], MenuNpc.Gi().MenuCalich[1], character.InfoChar.Gender));
                                character.TypeMenu = 1;
                                break;
                            }
                        }
                        break;
                    }
                    //Santa
                    case 39:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextSanta[0], MenuNpc.Gi().MenuSanta[0], character.InfoChar.Gender));
                        character.TypeMenu = 0;
                        break;
                    }
                    // trung thu
                    case 41:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextTrungThu[0], MenuNpc.Gi().MenuTrungThu[0], character.InfoChar.Gender));
                        character.TypeMenu = 0;
                        break;
                    }
                    //Quốc vương
                    case 42:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuocVuong[0], MenuNpc.Gi().MenuQuocVuong[0], character.InfoChar.Gender));
                        character.TypeMenu = 0;
                        break;
                    }
                    // Giu ma
                    case 47:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextGiuMa[0], MenuNpc.Gi().MenuGiuMa[0], character.InfoChar.Gender));
                        character.TypeMenu = 0;
                        break;
                    }
                    // Quả trứng
                    case 50:
                    {
                        if (character.InfoChar.ThoiGianTrungMaBu <= 0) return;
                        var seconds = (character.InfoChar.ThoiGianTrungMaBu - ServerUtils.CurrentTimeMillis()) / 1000;
                        if (seconds > 0) //chưa đủ thời gian nở
                        {
                            MenuNpc.Gi().MenuQuaTrung[0][0] = "Chờ\n" + ServerUtils.GetTimeAgo((int)seconds);
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuaTrung[0], MenuNpc.Gi().MenuQuaTrung[0], character.InfoChar.Gender));
                            character.TypeMenu = 0;
                        }
                        else 
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuaTrung[0], MenuNpc.Gi().MenuQuaTrung[1], character.InfoChar.Gender));
                            character.TypeMenu = 1;
                        }
                        break;
                    }
                    // Bill
                    case 55:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextBill[0], MenuNpc.Gi().MenuBill[0], character.InfoChar.Gender));
                        character.TypeMenu = 0;
                        break;
                    }
                    // Nồi bánh
                    case 66:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextNoiBanh[0], MenuNpc.Gi().MenuNoiBanh[0], character.InfoChar.Gender));
                        character.TypeMenu = 0;
                        break;
                    }
                    // Mrpopo
                    case 67:
                    {
                        break;
                    }
                    default:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, TextServer.gI().UPDATING));
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error OpenUiMenu in Menu.cs: {e.Message} \n {e.StackTrace}", e);
            }
        }

        public static void MenuHandler(Message message, Character character)
        {
            try
            {
                var npcId = message.Reader.ReadByte();
                var menuId = message.Reader.ReadByte();
                var optionId = message.Reader.ReadByte();
                Server.Gi().Logger.Debug($"Menu Handler --------------------------- {npcId} - {menuId} - {optionId}");
                switch (npcId)
                {
                    //Đậu thần
                    case 4:
                    {
                        MenuDauThan(character, npcId, menuId, optionId);
                        break;
                    }
                    default:
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, TextServer.gI().UPDATING));
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Menu Handler in Menu.cs: {e.Message} \n {e.StackTrace}", e);
            }
            finally
            {
                message?.CleanUp();
            }
        }

        public static void UiConfirm(Message message, Character character)
        {
            try
            {
                var npcId = message.Reader.ReadShort();
                var select = message.Reader.ReadByte();
                switch (npcId)
                {
                    case 0:
                    case 1:
                    case 2:
                    {
                        // 3 ông già
                        ConfirmBaOngGia(character, npcId, select);
                        break;
                    }
                    case 5:
                    {
                        ConfirmMeo(character, npcId, select);
                        break;
                    }
                    case 7: {
                        ConfirmBumma(character, npcId, select);
                        break;
                    }
                    case 8: {
                        ConfirmDende(character, npcId, select);
                        break;
                    }
                    case 9: {
                        ConfirmAppule(character, npcId, select);
                        break;
                    }
                    case 10: {
                        ConfirmBrief(character, npcId, select);
                        break;
                    }
                    case 11: {
                        ConfirmCargo(character, npcId, select);
                        break;
                    }
                    case 12: {
                        ConfirmCui(character, npcId, select);
                        break;
                    }
                    case 13: {
                        ConfirmQuyLao(character, npcId, select);
                        break;
                    }
                    case 14: {
                        ConfirmTruongLaoGuru(character, npcId, select);
                        break;
                    }
                    case 15: {
                        ConfirmVuaVegeta(character, npcId, select);
                        break;
                    }
                    case 18: {
                        ConfirmThanMeo(character, npcId, select);
                        break;
                    }
                    case 19: {
                        ConfirmThuongDe(character, npcId, select);
                        break;
                    }
                    case 20: {
                        ConfirmThanVuTru(character, npcId, select);
                        break;
                    }
                    case 21: {
                        ConfirmBaHatMit(character, npcId, select);
                        break;
                    }
                    case 23: {
                        ConfirmGhiDanh(character, npcId, select);
                        break;
                    }
                    case 24: {
                        ConfirmRongThan(character, npcId, select);
                        break;
                    }
                    case 25: {
                        character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Sẽ mở vào ngày 10/11"));
                        break;
                    }
                    case 37: {
                        ConfirmBummaTL(character, npcId, select);
                        break;
                    }
                    case 38: {
                        ConfirmCalich(character, npcId, select);
                        break;
                    }
                    case 39: {
                        ConfirmSanta(character, npcId, select);
                        break;
                    }
                    // case 41: {
                    //     // ConfirmTrungThu(character, npcId, select);
                    //     break;
                    // }
                    case 42: {
                        ConfirmQuocVuong(character, npcId, select);
                        break;
                    }
                    case 47:
                    {
                        ConfirmGiuMa(character, npcId, select);
                        break;
                    }
                    case 50: {
                        if (character.InfoChar.ThoiGianTrungMaBu <= 0)
                        {
                            UserDB.BanUser(character.Player.Id);
                            ClientManager.Gi().KickSession(character.Player.Session);
                            ServerUtils.WriteLog("hacktrung", $"Tên tài khoản {character.Player.Username} (ID:{character.Player.Id}) hack trứng");

                            var temp = ClientManager.Gi().GetPlayer(character.Player.Id);
                            if (temp != null)
                            {
                                ClientManager.Gi().KickSession(temp.Session);
                            }
                            return;
                        }
                        ConfirmQuaTrung(character, npcId, select);
                        break;
                    }
                    case 55: {
                        ConfirmBill(character, npcId, select);
                        break;
                    }
                    // case 66: {
                    //     ConfirmNoiBanh(character, npcId, select);
                    //     break;
                    // }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Ui Confirm in Menu.cs: {e.Message} \n {e.StackTrace}", e);
            }
            finally
            {
                message?.CleanUp();
            }
        }

        #region Menu COFIRM

        private static void ConfirmBaOngGia(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                case 0:
                {
                    switch(select)
                    {
                        case 0://nạp
                        {
                            // character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, MenuNpc.Gi().TextNapThe[0]));
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextNapThe[0], MenuNpc.Gi().MenuNapThe[0], character.InfoChar.Gender));
                            character.TypeMenu = 2;
                            character.ShopId = DataCache.SHOP_ID_NAPTHE + npcId;
                            break;
                        }
                        // case 0://Nhận 100k ngọc
                        // {
                        //     var timeServer = ServerUtils.CurrentTimeMillis();
                        //     if (character.Delay.GetGem > timeServer)
                        //     {
                        //         var delay = (character.Delay.GetGem - timeServer) / 1000;
                        //         if (delay < 1)
                        //         {
                        //             delay = 1;
                        //         }

                        //         character.CharacterHandler.SendMessage(
                        //             Service.ServerMessage(string.Format(TextServer.gI().DELAY_SEC,
                        //                 delay)));
                        //         return;
                        //     }
                        //     character.PlusDiamond(2000000);
                        //     character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                        //     character.Delay.GetGem = timeServer + 300000;
                        //     character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Bạn đã nhận được 2tr ngọc"));
                        //     break;
                        // }
                        case 1://Gift code
                        {
                            var timeServer = ServerUtils.CurrentTimeMillis();
                            if (character.Delay.UseGiftCode > timeServer)
                            {
                                var delay = (character.Delay.UseGiftCode - timeServer) / 1000;
                                if (delay < 1)
                                {
                                    delay = 1;
                                }

                                character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().DELAY_SEC,
                                        delay)));
                                return;
                            }
                            
                            var inputGiftcode = new List<InputBox>();
                            var inputCode = new InputBox(){
                                Name = "Nhập mã quà tặng",
                                Type = 1,
                            };
                            inputGiftcode.Add(inputCode);
                            character.CharacterHandler.SendMessage(Service.ShowInput("Giftcode Ngọc Rồng SUPER HERO", inputGiftcode));
                            character.TypeInput = 1;
                            character.ShopId = npcId;
                            break;
                        }
                        case 2://nhận lại đệ tử
                        {
                            var timeServer = ServerUtils.CurrentTimeMillis();
                            if (character.Delay.GetGem > timeServer)
                            {
                                var delay = (character.Delay.GetGem - timeServer) / 1000;
                                if (delay < 1)
                                {
                                    delay = 1;
                                }

                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(string.Format(TextServer.gI().DELAY_SEC,
                                        delay)));
                                return;
                            }

                            if (character.InfoChar.IsHavePet == false && DiscipleDB.IsAlreadyExist(-character.Id)) //ko load được đệ
                            {
                                character.InfoChar.IsHavePet = true;
                                character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Đệ tử của bạn đã trở lại vui lòng thoát hẳn game và đăng nhập lại"));
                                return;
                            }

                            if (character.Disciple == null && !DiscipleDB.IsAlreadyExist(-character.Id))
                            {
                                var disciple = new Disciple();
                                disciple.CreateNewDisciple(character);
                                disciple.Player = character.Player;
                                disciple.CharacterHandler.SetUpInfo();
                                character.Disciple = disciple;
                                character.InfoChar.IsHavePet = true;
                                character.CharacterHandler.SendMessage(Service.Disciple(1, null));
                                DiscipleDB.Create(disciple);
                                character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Con đã nhận được đệ tử"));
                            }
                            else 
                            {
                                character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Con đã có đệ tử rồi mà"));
                            }
                            character.Delay.GetGem = timeServer + 300000;
                            break;
                        }
                        case 3://đổi mật khẩu
                        {
                            var timeServer = ServerUtils.CurrentTimeMillis();
                            if (character.Delay.UseGiftCode > timeServer)
                            {
                                var delay = (character.Delay.UseGiftCode - timeServer) / 1000;
                                if (delay < 1)
                                {
                                    delay = 1;
                                }

                                character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().DELAY_SEC,
                                        delay)));
                                return;
                            }
                            // nhaapj mk hien tai
                            // nhap mk moi
                            // Nhap sdt dang ky
                            // Hiển thị menu input nhập seri và số thẻ
                            var inputDoiMatKhau = new List<InputBox>();

                            var inputMKHT = new InputBox(){
                                Name = TextServer.gI().INPUT_CURRENT_PASS,
                                Type = 1,
                            };
                            inputDoiMatKhau.Add(inputMKHT);

                            var inputMKM = new InputBox(){
                                Name = TextServer.gI().INPUT_NEW_PASS,
                                Type = 1,
                            };
                            inputDoiMatKhau.Add(inputMKM);

                            // var inputSDT = new InputBox(){
                            //     Name = TextServer.gI().INPUT_REG_SDT,
                            //     Type = 0,
                            // };
                            // inputDoiMatKhau.Add(inputSDT);

                            character.CharacterHandler.SendMessage(Service.ShowInput(TextServer.gI().INPUT_CHANGE_PASS, inputDoiMatKhau));
                            character.TypeInput = 2;
                            character.ShopId = npcId;
                            break;
                        }
                        case 4: //bật tắt hiệu ứng
                        {
                            if (character.InfoChar.HieuUngDonDanh)
                            {
                                character.InfoChar.HieuUngDonDanh = false;
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Đã TẮT hiệu ứng đòn đánh"));
                            }
                            else 
                            {
                                character.InfoChar.HieuUngDonDanh = true;
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Đã BẬT hiệu ứng đòn đánh"));
                            }
                            break;
                        }
                        case 5://top nạp
                        {
                            var bangXepHangTopNap = Server.Gi().BangXepHang.GetListTopNap();
                            bangXepHangTopNap += $"\b{ServerUtils.Color("red")}Điểm tích nạp của bạn là: " + character.Player.DiemTichNap;
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, bangXepHangTopNap));
                            break;
                        }
                        case 6: 
                        {
                            var bangXepHangTopSM = Server.Gi().BangXepHang.GetList();
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, bangXepHangTopSM));
                            break;
                        }
                        // case 5://đỏi máy chủ
                        // {
                        //     var delayChangeServer = character.InfoChar.ThoiGianDoiMayChu;
                        //     var timeServer = ServerUtils.CurrentTimeMillis();
                        //     if (delayChangeServer > timeServer)
                        //     {
                        //         var time = (delayChangeServer - timeServer) / 1000;
                        //         character.CharacterHandler.SendMessage(
                        //             Service.ServerMessage(string.Format(TextServer.gI().DELAY_CHANGE_SV, time)));
                        //         return;
                        //     }
                        //     character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextBaOngGia[2], MenuNpc.Gi().MenuBaOngGia[1], character.InfoChar.Gender));
                        //     character.TypeMenu = 1;
                        //     break;
                        // }
                        
                    }
                    break;
                }
                case 1:
                {
                    var timeServer = ServerUtils.CurrentTimeMillis();
                    switch (select)
                    {
                        case 0:
                        {
                            if (DatabaseManager.Manager.gI().ServerPort == 14445)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn đang ở máy chủ này rồi"));
                                return;
                            }

                            character.InfoChar.ThoiGianDoiMayChu = timeServer + 1800000;
                            var userId = character.Player.Id;
                            character.Delay.DoiMayChu = true;
                            UserDB.UpdatePort(userId, 14445);
                            ClientManager.Gi().KickSession(character.Player.Session);
                            break;
                        }
                        case 1:
                        {
                            // if (DatabaseManager.Manager.gI().ServerPort == 14446)
                            // {
                            //     character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn đang ở máy chủ này rồi"));
                            //     return;
                            // }
                            // character.InfoChar.ThoiGianDoiMayChu = timeServer + 1800000;
                            // if (!character.InfoChar.IsPremium)
                            // {
                            //     character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_PREMIUM));
                            //     return;
                            // }
                            // var userId = character.Player.Id;
                            // character.Delay.DoiMayChu = true;
                            // UserDB.UpdatePort(userId, 14446);
                            // ClientManager.Gi().KickSession(character.Player.Session);
                            break;
                        }
                    }
                    break;
                }
                // Các case nạp thẻ
                case 2://Chọn danh sách loại thẻ nạp
                {
                    switch(select)
                    {
                        case 0:
                        {
                            character.NapTheTemp.LoaiThe = "VIETTEL";
                            break;
                        }
                        case 1:
                        {
                            character.NapTheTemp.LoaiThe = "VINAPHONE";
                            break;
                        }
                        case 2:
                        {
                            character.NapTheTemp.LoaiThe = "MOBIFONE";
                            break;
                        }
                        case 3:
                        {
                            character.NapTheTemp.LoaiThe = "ZING";
                            break;
                        }
                        default:
                        {
                            character.NapTheTemp.LoaiThe = "";
                            return;
                        }
                    }
                    // Chọn mệnh giá
                    character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextNapThe[1], MenuNpc.Gi().MenuNapThe[1], character.InfoChar.Gender));
                    character.TypeMenu = 3;
                    character.MenuPage = 0;//Page 0
                    break;
                }
                case 3://Chọn mệnh giá thẻ nạp
                {
                    switch(character.MenuPage)
                    {
                        case 0://page 0
                        {
                            switch(select)
                            {
                                case 0:
                                {
                                    //10k
                                    character.NapTheTemp.MenhGia = 10000;
                                    break;
                                }
                                case 1:
                                {
                                    //20k
                                    character.NapTheTemp.MenhGia = 20000;
                                    break;
                                }
                                case 2:
                                {
                                    //30k
                                    character.NapTheTemp.MenhGia = 30000;
                                    break;
                                }
                                case 3:
                                {
                                    //50k
                                    character.NapTheTemp.MenhGia = 50000;
                                    break;
                                }
                                case 4:
                                {
                                    //Mệnh giá khác
                                    character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextNapThe[1], MenuNpc.Gi().MenuNapThe[2], character.InfoChar.Gender));
                                    character.TypeMenu = 3;
                                    character.MenuPage = 1;//Page 1
                                    return;
                                }
                                default:
                                {
                                    return;
                                }
                            }
                            break;
                        }
                        case 1:
                        {
                            switch(select)
                            {
                                case 0:
                                {
                                    //100k
                                    character.NapTheTemp.MenhGia = 100000;
                                    break;
                                }
                                case 1:
                                {
                                    //200k
                                    character.NapTheTemp.MenhGia = 200000;
                                    break;
                                }
                                case 2:
                                {
                                    //300k
                                    character.NapTheTemp.MenhGia = 300000;
                                    break;
                                }
                                case 3:
                                {
                                    //500k
                                    character.NapTheTemp.MenhGia = 500000;
                                    break;
                                }
                                case 4:
                                {
                                    //1000k
                                    character.NapTheTemp.MenhGia = 1000000;
                                    break;
                                }
                                default:
                                {
                                    return;
                                }
                            }
                            break;
                        }
                    }
                    // Hiển thị menu input nhập seri và số thẻ
                    var inputNapThe = new List<InputBox>();
                    var inputSeri = new InputBox(){
                        Name = TextServer.gI().INPUT_SERI_THE,
                        Type = 0,
                    };
                    inputNapThe.Add(inputSeri);
                    var inputPin = new InputBox(){
                        Name = TextServer.gI().INPUT_PIN_THE,
                        Type = 0,
                    };
                    inputNapThe.Add(inputPin);
                    character.CharacterHandler.SendMessage(Service.ShowInput(TextServer.gI().NHAP_TT_THE, inputNapThe));
                    character.TypeInput = 0;
                    break;
                }
            }
        }

        private static void ConfirmMeo(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                //Thách đấu
                case 0:
                {
                    if (DataCache.IdMapCustom.Contains(character.InfoChar.MapCustomId))
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DOT_NOT_TEST_HERE));
                        return;
                    }
                    var zone = MapManager.Get(character.InfoChar.MapId)?.GetZoneById(character.InfoChar.ZoneId);
                    if (zone == null) return;
                    var testChar =  (Character)zone.ZoneHandler.GetCharacter(character.Test.CheckId);
                    if(testChar == null) character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                    else
                    {
                        switch (select)
                        {
                            //1,000 vàng
                            case 0:
                            {
                                if (character.InfoChar.Gold < 1000)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                                    character.Test.GoldTest = 0;
                                }
                                else
                                {
                                    var text = string.Format(TextServer.gI().SEND_TEST, character.Name, ServerUtils.GetPower(character.InfoChar.Potential), 1000);
                                    character.Test.GoldTest = testChar.Test.GoldTest = 1000;
                                    testChar
                                        .CharacterHandler
                                        .SendMessage(Service
                                            .PlayerVsPLayer(3, character.Id, 1000, text));
                                }
                                break;
                            }
                            //10,000 vàng
                            case 1:
                            {
                                if (character.InfoChar.Gold < 10000)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                                    character.Test.GoldTest = 0;
                                }
                                else
                                {
                                    var text = string.Format(TextServer.gI().SEND_TEST, character.Name, ServerUtils.GetPower(character.InfoChar.Potential), 10000);
                                    character.Test.GoldTest = testChar.Test.GoldTest = 10000;
                                    testChar
                                        .CharacterHandler
                                        .SendMessage(Service
                                            .PlayerVsPLayer(3, character.Id, 10000, text));
                                }
                                break;
                            }
                            //100,000 vàng
                            case 2:
                            {
                                if (character.InfoChar.Gold < 100000)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                                    character.Test.GoldTest = 0;
                                }
                                else
                                {
                                    var text = string.Format(TextServer.gI().SEND_TEST, character.Name, ServerUtils.GetPower(character.InfoChar.Potential), 100000);
                                    character.Test.GoldTest = testChar.Test.GoldTest = 100000;
                                    testChar
                                        .CharacterHandler
                                        .SendMessage(Service
                                            .PlayerVsPLayer(3, character.Id, 100000, text));
                                }
                                break;
                            }
                        }
                    }
                    break;
                }
                //Nâng cấp đậu
                case 1:
                {
                    var magicTree = MagicTreeManager.Get(character.Id);
                    if(magicTree == null || select == 1) return;
                    lock (magicTree)
                    {
                        var levelTree = magicTree.Level;
                        var gold = DataCache.UpgradeDauThanGold[levelTree - 1];
                        if (character.InfoChar.Gold < gold)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                            return;
                        }
                        character.MineGold(gold);
                        magicTree.IsUpdate = true;
                        magicTree.Seconds = DataCache.UpgradeDauThanTime[levelTree - 1] + ServerUtils.CurrentTimeMillis();
                        magicTree.MagicTreeHandler.HandleNgoc();
                        character.CharacterHandler.SendMessage(Service.MagicTree0(magicTree));
                        character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                    }
                    break;
                }
                //Huỷ nâng cấp đậu
                case 2:
                {
                    var magicTree = MagicTreeManager.Get(character.Id);
                    if(magicTree == null || select == 1) return;
                    lock (magicTree)
                    {
                        var levelTree = magicTree.Level;
                        var gold = DataCache.UpgradeDauThanGold[levelTree - 1];
                        character.PlusGold(gold/2);
                        magicTree.IsUpdate = false;
                        if (magicTree.Peas == magicTree.MaxPea)
                        {
                            magicTree.Seconds = 0;
                        }
                        else
                        {
                            magicTree.Seconds = 60000 * magicTree.Level + ServerUtils.CurrentTimeMillis();
                        }
                        magicTree.MagicTreeHandler.HandleNgoc();
                        character.CharacterHandler.SendMessage(Service.MagicTree0(magicTree));
                        character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                    }
                    break;
                }
                //Kết bạn
                case 3:
                {
                    if(select != 0 || character.FriendTemp == null) return;
                    character.Friends.Add(character.FriendTemp);
                    var @char = ClientManager.Gi().GetCharacter(character.FriendTemp.Id);
                    @char?.CharacterHandler.SendMessage(Service.WorldChat((Character)character, string.Format(TextServer.gI().ADD_FRIEND, character.Name, @char.Name), 1));
                    character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().ADD_FRIEND_2, character.FriendTemp.Name)));
                    character.FriendTemp = null;
                    break;
                }
                //Xoá kết bạn
                case 4:
                {
                    if(select != 0 || character.FriendTemp == null) return;
                    character.Friends.RemoveAll(friend => friend.Id == character.FriendTemp.Id);
                    character.CharacterHandler.SendMessage(Service.ListFriend2(character.FriendTemp.Id));
                    character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().FRIEND_DELETE, character.FriendTemp.Name)));
                    character.FriendTemp = null;
                    break;
                }
                //Dịch chuyển tới người chơi
                case 5:
                {
                    if(select != 0 || character.EnemyTemp == null) return;
                    var charCheck = ClientManager.Gi().GetCharacter(character.EnemyTemp.Id);
                    if (charCheck == null)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().USER_OFFLINE));
                    }
                    else
                    {
                        var mapId = character.InfoChar.MapId;
                        if (DataCache.IdMapCustom.Contains(mapId))
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().TELEPORT_ERROR));
                        }
                        else
                        {

                        }
                    }

                    character.EnemyTemp = null;
                    break;
                }
                //Rời bang
                case 6:
                {
                    if(select != 0) return;
                    var clan = ClanManager.Get(character.ClanId);
                    if (clan == null) return;
                    var me = clan.ClanHandler.GetMember(character.Id);
                    if (clan.ClanHandler.RemoveMember(me.Id))
                    {
                        var lastMess = clan.Messages.LastOrDefault();
                        var id = lastMess != null ? lastMess.Id + 1 : 0;
                        clan.ClanHandler.Chat(new ClanMessage()
                        {
                            Type = 0,
                            Id = id,
                            PlayerId = -1,
                            PlayerName = "Thông báo",
                            Role = 0,
                            Time = ServerUtils.CurrentTimeSecond() - 1000000000,
                            Text = string.Format(TextServer.gI().LEAVE_CLAN, me.Name),
                            Color = 1,
                            NewMessage = true,
                        });
                        character.ClanId = -1;
                        character.InfoChar.Bag = -1;
                        clan.ClanHandler.SendUpdateClan();
                        if(character.InfoChar.PhukienPart == -1)character.CharacterHandler.SendZoneMessage(Service.SendImageBag(character.Id, -1));
                        character.CharacterHandler.SendMessage(Service.GetImageBag(null));
                        character.CharacterHandler.SendMessage(Service.MyClanInfo());
                        character.CharacterHandler.SendZoneMessage(Service.UpdateClanId(character.Id, -1));
                        clan.ClanHandler.UpdateClanId();
                        CharacterDB.Update(character);
                        ClanDB.Update(clan);
                    }
                    break;
                }
                //Xoá thù địch
                case 7:
                {
                    if(select != 0 || character.EnemyTemp == null) return;
                    character.Enemies.RemoveAll(enemy => enemy.Id == character.EnemyTemp.Id);
                    character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().ENEMY_DELETE, character.EnemyTemp.Name)));
                    character.EnemyTemp = null;
                    break;
                }
                //Đồng ý kích hoạt mã
                case 8:
                {
                    if(select != 0 || character.InfoChar.LockInventory.PassTemp == -1) return;
                    if (character.InfoChar.Gold < 50000)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                        return;
                    }
                    character.MineGold(50000);
                    character.InfoChar.LockInventory.IsLock = true;
                    character.InfoChar.LockInventory.Pass = character.InfoChar.LockInventory.PassTemp;
                    character.InfoChar.LockInventory.PassTemp = -1;
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().ACTIVE_LOCK_INVENTORY));
                    character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                    break;
                }
                //Mở/Khoá rương
                case 9:
                {
                    if(select != 0 || character.InfoChar.LockInventory.Pass == -1) return;
                    character.InfoChar.LockInventory.IsLock = !character.InfoChar.LockInventory.IsLock;
                    character.CharacterHandler.SendMessage(character.InfoChar.LockInventory.IsLock
                        ? Service.ServerMessage(TextServer.gI().SUCCESS_LOCK_INVENTORY)
                        : Service.ServerMessage(TextServer.gI().UNACTIVE_LOCK_INVENTORY));
                    break;
                }
                // Nội tại
                case 10:
                {
                    switch(select)
                    {
                        case 0: //Xem tất cả nội tại
                        {
                            character.CharacterHandler.SendMessage(Service.SpeacialSkill(character, 1));
                            break;
                        }
                        case 1: //Mở nội tại thường
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5, string.Format(MenuNpc.Gi().TextNoiTai[1], DataCache.PRICE_UNLOCK_SPECIAL_SKILL),
                                    MenuNpc.Gi().MenuNoiTai[1], character.InfoChar.Gender));
                            character.TypeMenu = 11;
                            break;
                        }
                        case 2: //Mở nội tại VIP
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5, string.Format(MenuNpc.Gi().TextNoiTai[2], ServerUtils.GetMoney(DataCache.PRICE_UNLOCK_SPECIAL_SKILL_VIP)),
                                    MenuNpc.Gi().MenuNoiTai[2], character.InfoChar.Gender));
                            character.TypeMenu = 12;
                            break;
                        }

                    }
                    break;
                }
                case 11://mở nội tại thường
                {
                    var specialSkillTemplate = Cache.Gi().SPECIAL_SKILL_TEMPLATES.FirstOrDefault(s => s.Key == character.InfoChar.Gender).Value;
                    if (specialSkillTemplate == null) return;
                    if(character.AllDiamond() < DataCache.PRICE_UNLOCK_SPECIAL_SKILL) {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                        return;
                    }
                    character.MineDiamond(DataCache.PRICE_UNLOCK_SPECIAL_SKILL);

                    int RandomIndex = ServerUtils.RandomNumber(specialSkillTemplate.Count);
                    SpecialSkillTemplate SkillRandom = specialSkillTemplate[RandomIndex];

                    int ValueRandom = 0;
                    
                    if (SkillRandom.Vip == 1) {
                        ValueRandom = ServerUtils.RandomNumber(SkillRandom.Min, SkillRandom.Max/2);
                    }
                    else 
                    {
                        ValueRandom = ServerUtils.RandomNumber(SkillRandom.Min, SkillRandom.Max + 1);
                    }
                    
                    string InfoRandom = SkillRandom.InfoFormat.Replace("#", ValueRandom + "");

                    character.SpecialSkill.Id = SkillRandom.Id;
                    character.SpecialSkill.Info = InfoRandom;
                    character.SpecialSkill.Img = SkillRandom.Img;
                    character.SpecialSkill.SkillId = SkillRandom.SkillId;
                    character.SpecialSkill.Value = ValueRandom;
                    character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn đã mở nội tại " + InfoRandom));
                    character.CharacterHandler.SendMessage(Service.SpeacialSkill(character, 0));
                    character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                    break;
                }
                case 12://mở nội tại VIP
                {
                    var specialSkillTemplate = Cache.Gi().SPECIAL_SKILL_TEMPLATES.FirstOrDefault(s => s.Key == character.InfoChar.Gender).Value;
                    if (specialSkillTemplate == null) return;
                    if(character.InfoChar.Gold < DataCache.PRICE_UNLOCK_SPECIAL_SKILL_VIP) {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                        return;
                    }
                    character.MineGold(DataCache.PRICE_UNLOCK_SPECIAL_SKILL_VIP);

                    int RandomIndex = ServerUtils.RandomNumber(specialSkillTemplate.Count);
                    SpecialSkillTemplate SkillRandom = specialSkillTemplate[RandomIndex];

                    int ValueRandom = 0;

                    ValueRandom = ServerUtils.RandomNumber(SkillRandom.Min, SkillRandom.Max + 1);
                    
                    string InfoRandom = SkillRandom.InfoFormat.Replace("#", ValueRandom + "");

                    character.SpecialSkill.Id = SkillRandom.Id;
                    character.SpecialSkill.Info = InfoRandom;
                    character.SpecialSkill.Img = SkillRandom.Img;
                    character.SpecialSkill.SkillId = SkillRandom.SkillId;
                    character.SpecialSkill.Value = ValueRandom;
                    character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn đã mở nội tại " + InfoRandom));
                    character.CharacterHandler.SendMessage(Service.SpeacialSkill(character, 0));
                    character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                    break;
                }
            }
        }
        
        private static void ConfirmBumma(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                case 0:
                {
                    if (character.InfoChar.Gender != 0)
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, MenuNpc.Gi().TextBumma[1]));
                    }
                    else if (select == 0)
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextBumma[0], MenuNpc.Gi().MenuShopDistrict[1], character.InfoChar.Gender));
                        character.TypeMenu = 1;
                    }
                    break;
                }
                //Show shop
                case 1:
                {
                    if(select == 1) return;
                    var shopId = 12;
                    character.CharacterHandler.SendMessage(Service.Shop(character, 0, shopId));
                    character.ShopId = shopId;
                    character.TypeMenu = 0;
                    break;
                }
            }
        }

        private static void ConfirmBummaTL(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                case 0:
                {
                    if(select == 1) return;
                    var shopId = 22;
                    character.CharacterHandler.SendMessage(Service.Shop(character, 0, shopId));
                    character.ShopId = shopId;
                    character.TypeMenu = 0;
                    break;
                }
            }
        }
        
        private static void ConfirmDende(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                case 0:
                {
                    if (character.InfoChar.Gender != 1)
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, MenuNpc.Gi().TextDende[1]));
                    }
                    else if (select == 0)
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextDende[0], MenuNpc.Gi().MenuShopDistrict[1], character.InfoChar.Gender));
                        character.TypeMenu = 1;
                    }
                    break;
                }
                //Show shop
                case 1:
                {
                    if(select == 1) return;
                    var idShop = 13;
                    character.CharacterHandler.SendMessage(Service.Shop(character, 0, idShop));
                    character.ShopId = idShop;
                    character.TypeMenu = 0;
                    break;
                }
            }
        }
        
        private static void ConfirmAppule(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                case 0:
                {
                    if (character.InfoChar.Gender != 2)
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, MenuNpc.Gi().TextAppule[1]));
                    }
                    else if (select == 0)
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextAppule[0], MenuNpc.Gi().MenuShopDistrict[1], character.InfoChar.Gender));
                        character.TypeMenu = 1;
                    }
                    break;
                }
                //Show shop
                case 1:
                {
                    if(select == 1) return;
                    var idShop = 14;
                    character.CharacterHandler.SendMessage(Service.Shop(character, 0, idShop));
                    character.ShopId = idShop;
                    character.TypeMenu = 0;
                    break;
                }
            }
        }
        
        private static void ConfirmBrief(Character character, short npcId, int select)
        {
            var map = MapManager.Get(character.InfoChar.MapId);
            if (map == null) return;
            Threading.Map mapJoin;
            if (map.Id == 84)
            {
                mapJoin = MapManager.Get(character.InfoChar.Gender + 24);
            }
            else
            {
                switch (select)
                {
                    case 0:
                    {
                        mapJoin = MapManager.Get(26);
                        break;
                    }
                    case 1:
                    {
                        mapJoin = MapManager.Get(25);
                        break;
                    }
                    case 2:
                    {
                        mapJoin = MapManager.Get(84);
                        break;
                    }
                    default:
                    {
                        return;
                    }
                }
            }

            if (mapJoin == null) return;
            var zoneJoin = mapJoin.GetZoneNotMaxPlayer();
            if (zoneJoin != null)
            {
                character.CharacterHandler.SendZoneMessage(Service.SendTeleport(character.Id, character.InfoChar.Teleport));
                map.OutZone(character, mapJoin.Id);
                zoneJoin.ZoneHandler.JoinZone(character, false, true, character.InfoChar.Teleport);
            }
            else
            {
                character.CharacterHandler.SendMessage(Service.OpenUiSay(5, TextServer.gI().MAX_NUMCHARS, false, character.InfoChar.Gender));
            }
        }
        
        private static void ConfirmCargo(Character character, short npcId, int select)
        {
            var map = MapManager.Get(character.InfoChar.MapId);
            if (map == null) return;
            Threading.Map mapJoin;
            switch (select)
            {
                case 0:
                {
                    mapJoin = MapManager.Get(24);
                    break;
                }
                case 1:
                {
                    mapJoin = MapManager.Get(26);
                    break;
                }
                case 2:
                {
                    mapJoin = MapManager.Get(84);
                    break;
                }
                default:
                {
                    return;
                }
            }

            if (mapJoin == null) return;
            var zoneJoin = mapJoin.GetZoneNotMaxPlayer();
            if (zoneJoin != null)
            {
                character.CharacterHandler.SendZoneMessage(Service.SendTeleport(character.Id, character.InfoChar.Teleport));
                map.OutZone(character, mapJoin.Id);
                zoneJoin.ZoneHandler.JoinZone(character, false, true, character.InfoChar.Teleport);
            }
            else
            {
                character.CharacterHandler.SendMessage(Service.OpenUiSay(5, TextServer.gI().MAX_NUMCHARS, false, character.InfoChar.Gender));
            }
        }
        
        private static void ConfirmCui(Character character, short npcId, int select)
        {
            var map = MapManager.Get(character.InfoChar.MapId);
            if (map == null) return;
            Threading.Map mapJoin = null;
            switch (map.Id)
            {
                case 19 when @select == 0:
                {
                    if (character.InfoChar.Power < 40000000000) {
                        character.CharacterHandler.SendMessage(Service.OpenUiSay(5, "Bạn cần đạt 40 tỷ sức mạnh mới có thể qua hành tinh Cold", false, character.InfoChar.Gender));
                        return;
                    }
                    mapJoin = MapManager.Get(109);
                    break;
                }
                case 19:
                {
                    if (@select == 1)
                    {
                        mapJoin = MapManager.Get(68);
                    }
                    break;
                }
                case 68:
                {
                    if (@select == 0)
                    {
                        mapJoin = MapManager.Get(19);
                    }

                    break;
                }
                default:
                {
                    switch (@select)
                    {
                        case 0:
                        {
                            mapJoin = MapManager.Get(24);
                            break;
                        }
                        case 1:
                        {
                            mapJoin = MapManager.Get(25);
                            break;
                        }
                        case 2:
                        {
                            mapJoin = MapManager.Get(84);
                            break;
                        }
                        default:
                        {
                            return;
                        }
                    }

                    break;
                }
            }

            if (mapJoin == null) return;
            var zoneJoin = mapJoin.GetZoneNotMaxPlayer();
            if (zoneJoin != null)
            {
                character.CharacterHandler.SendZoneMessage(Service.SendTeleport(character.Id, character.InfoChar.Teleport));
                map.OutZone(character, mapJoin.Id);
                zoneJoin.ZoneHandler.JoinZone(character, false, true, character.InfoChar.Teleport);
            }
            else
            {
                character.CharacterHandler.SendMessage(Service.OpenUiSay(5, TextServer.gI().MAX_NUMCHARS, false, character.InfoChar.Gender));
            }
        }
        
        private static void ConfirmQuyLao(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                //Open menu 1
                case 0:
                {
                    switch (select)
                    {
                        //Nói chuyện
                        case 0: {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[1], MenuNpc.Gi().MenuQuyLao[1], character.InfoChar.Gender));
                            character.TypeMenu = 1;
                            break;
                        }
                        //Kho báo dưới biển
                        case 1:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, TextServer.gI().UPDATING));
                            break;
                        }
                    }
                    break;
                }
                //Open menu Nói chuyện
                case 1:
                {
                    switch (select)
                    {
                        case 0:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, TextServer.gI().UPDATING));
                            break;
                        }
                        case 1:
                        {
                            if (character.InfoChar.LearnSkill != null)
                            {
                                var itemAdd = character.InfoChar.LearnSkill.ItemSkill;
                                var time = character.InfoChar.LearnSkill.Time - ServerUtils.CurrentTimeMillis();
                                var skillTemplate = Cache.Gi().SKILL_TEMPLATES.FirstOrDefault(skill => skill.Id == character.InfoChar.LearnSkill.ItemTemplateSkillId);
                                var itemTempalte = ItemCache.ItemTemplate(itemAdd.Id);
                                var ngoc = 5;
                                if (time / 600000 >= 2)
                                {
                                    ngoc += (int)time / 600000;
                                }

                                var menu = string.Format(TextServer.gI().ADDING_SKILL, skillTemplate.Name,
                                    itemTempalte.Level, ServerUtils.GetTime(time));
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, menu, new List<string>() {$"Học\nCấp tốc\n{ngoc} ngọc", "Huỷ","Bỏ qua"}, character.InfoChar.Gender));
                                character.TypeMenu = 3;
                            }
                            else
                            {
                                var idShop = 7 + character.InfoChar.Gender;
                                character.CharacterHandler.SendMessage(Service.Shop(character, 1, idShop));
                                character.ShopId = idShop;
                                character.TypeMenu = 0;
                            }
                            break;
                        }
                    }
                    break;
                }
                //Học skill
                case 2:
                {
                    switch (select)
                    {
                        //Đồng ý
                        case 0:
                        {
                            if(character.InfoChar.LearnSkillTemp == null) return;
                            var itemAdd = character.InfoChar.LearnSkillTemp.ItemSkill;
                            var time = character.InfoChar.LearnSkillTemp.Time + ServerUtils.CurrentTimeMillis();
                            var idSkill = character.InfoChar.LearnSkillTemp.ItemTemplateSkillId;
                            character.InfoChar.Potential -= itemAdd.BuyPotential;
                            character.InfoChar.LearnSkill = new LearnSkill()
                            {
                                ItemSkill = itemAdd,
                                Time = time,
                                ItemTemplateSkillId = idSkill,
                                Potential = (int)itemAdd.BuyPotential
                            };
                            character.InfoChar.LearnSkillTemp = null;
                            character.CharacterHandler.SendMessage(Service.MeLoadPoint(character));
                            character.CharacterHandler.SendMessage(Service.ClosePanel());
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Con đã học thành công, hãy cố gắng chờ đợi nha"));
                            break;
                        }
                        //Từ chối
                        case 1:
                        {
                            character.InfoChar.LearnSkillTemp = null;
                            break;
                        }
                    }
                    break;
                }
                //Open menu with learn skill
                case 3:
                {
                    switch (select)
                    {
                        //Đồng ý học nhanh
                        case 0:
                        {
                            if(character.InfoChar.LearnSkill == null) return;
                            var itemAdd = character.InfoChar.LearnSkill.ItemSkill;
                            var time = character.InfoChar.LearnSkill.Time - ServerUtils.CurrentTimeMillis();
                            var skillTemplate = Cache.Gi().SKILL_TEMPLATES.FirstOrDefault(skill => skill.Id == character.InfoChar.LearnSkill.ItemTemplateSkillId);
                            if (skillTemplate == null) return;
                            var ngoc = 5;
                            if (time / 600000 >= 2)
                            {
                                ngoc += (int)time / 600000;
                            }
                            if(character.AllDiamond() < ngoc) {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                                return;
                            }
                            character.MineDiamond(ngoc);
                            character.InfoChar.LearnSkill = null;
                            character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                            ItemHandler.AddLearnSkill(character, itemAdd, skillTemplate);
                            break;
                        }
                        //Huỷ học skill
                        case 1:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[3], MenuNpc.Gi().MenuMeo[1], character.InfoChar.Gender));
                            character.TypeMenu = 4;
                            break;
                        }
                        //Open menu 1
                        case 2:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[0], MenuNpc.Gi().MenuQuyLao[0], character.InfoChar.Gender));
                            character.TypeMenu = 0;
                            break;
                        }
                    }
                    break;
                }
                //Huỷ học skill
                case 4:
                {
                    if(select != 0) return;
                    var plusPoint = character.InfoChar.LearnSkill.Potential / 2;
                    character.CharacterHandler.PlusPoint(0, plusPoint, false);
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANCEL_LEARN_SKILL));
                    character.InfoChar.LearnSkill = null;   
                    character.InfoChar.LearnSkillTemp = null;   
                    break;
                }
            }
        }

        private static void ConfirmTruongLaoGuru(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                //Open menu Nói chuyện
                case 0:
                {
                    switch (select)
                    {
                        case 0:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, TextServer.gI().UPDATING));
                            break;
                        }
                        case 1:
                        {
                            if (character.InfoChar.LearnSkill != null)
                            {
                                var itemAdd = character.InfoChar.LearnSkill.ItemSkill;
                                var time = character.InfoChar.LearnSkill.Time - ServerUtils.CurrentTimeMillis();
                                var skillTemplate = Cache.Gi().SKILL_TEMPLATES.FirstOrDefault(skill => skill.Id == character.InfoChar.LearnSkill.ItemTemplateSkillId);
                                var itemTempalte = ItemCache.ItemTemplate(itemAdd.Id);
                                var ngoc = 5;
                                if (time / 600000 >= 2)
                                {
                                    ngoc += (int)time / 600000;
                                }

                                var menu = string.Format(TextServer.gI().ADDING_SKILL, skillTemplate.Name,
                                    itemTempalte.Level, ServerUtils.GetTime(time));
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, menu, new List<string>() {$"Học\nCấp tốc\n{ngoc} ngọc", "Huỷ","Bỏ qua"}, character.InfoChar.Gender));
                                character.TypeMenu = 2;
                            }
                            else
                            {
                                var idShop = 10;
                                character.CharacterHandler.SendMessage(Service.Shop(character, 1, idShop));
                                character.ShopId = idShop;
                                character.TypeMenu = 1;
                            }
                            break;
                        }
                    }
                    break;
                }
                //Học skill
                case 1:
                {
                    switch (select)
                    {
                        //Đồng ý
                        case 0:
                        {
                            if(character.InfoChar.LearnSkillTemp == null) return;
                            var itemAdd = character.InfoChar.LearnSkillTemp.ItemSkill;
                            var time = character.InfoChar.LearnSkillTemp.Time + ServerUtils.CurrentTimeMillis();
                            var idSkill = character.InfoChar.LearnSkillTemp.ItemTemplateSkillId;
                            character.InfoChar.Potential -= itemAdd.BuyPotential;
                            character.InfoChar.LearnSkill = new LearnSkill()
                            {
                                ItemSkill = itemAdd,
                                Time = time,
                                ItemTemplateSkillId = idSkill,
                                Potential = (int)itemAdd.BuyPotential
                            };
                            character.InfoChar.LearnSkillTemp = null;
                            character.CharacterHandler.SendMessage(Service.MeLoadPoint(character));
                            character.CharacterHandler.SendMessage(Service.ClosePanel());
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Con đã học thành công, hãy cố gắng chờ đợi nha"));
                            break;
                        }
                        //Từ chối
                        case 1:
                        {
                            character.InfoChar.LearnSkillTemp = null;
                            break;
                        }
                    }
                    break;
                }
                //Open menu with learn skill
                case 2:
                {
                    switch (select)
                    {
                        //Đồng ý học nhanh
                        case 0:
                        {
                            if(character.InfoChar.LearnSkill == null) return;
                            var itemAdd = character.InfoChar.LearnSkill.ItemSkill;
                            var time = character.InfoChar.LearnSkill.Time - ServerUtils.CurrentTimeMillis();
                            var skillTemplate = Cache.Gi().SKILL_TEMPLATES.FirstOrDefault(skill => skill.Id == character.InfoChar.LearnSkill.ItemTemplateSkillId);
                            if (skillTemplate == null) return;
                            var ngoc = 5;
                            if (time / 600000 >= 2)
                            {
                                ngoc += (int)time / 600000;
                            }
                            if(character.AllDiamond() < ngoc) {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                                return;
                            }
                            character.MineDiamond(ngoc);
                            character.InfoChar.LearnSkill = null;
                            character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                            ItemHandler.AddLearnSkill(character, itemAdd, skillTemplate);
                            break;
                        }
                        //Huỷ học skill
                        case 1:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[3], MenuNpc.Gi().MenuMeo[1], character.InfoChar.Gender));
                            character.TypeMenu = 3;
                            break;
                        }
                        //Open menu 1
                        case 2:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[1], MenuNpc.Gi().MenuQuyLao[1], character.InfoChar.Gender));
                            character.TypeMenu = 0;
                            break;
                        }
                    }
                    break;
                }
                //Huỷ học skill
                case 3:
                {
                    if(select != 0) return;
                    var plusPoint = character.InfoChar.LearnSkill.Potential / 2;
                    character.CharacterHandler.PlusPoint(0, plusPoint, false);
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANCEL_LEARN_SKILL));
                    character.InfoChar.LearnSkill = null;   
                    character.InfoChar.LearnSkillTemp = null;   
                    break;
                }
            }
        }

        private static void ConfirmVuaVegeta(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                //Open menu Nói chuyện
                case 0:
                {
                    switch (select)
                    {
                        case 0:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, TextServer.gI().UPDATING));
                            break;
                        }
                        case 1:
                        {
                            if (character.InfoChar.LearnSkill != null)
                            {
                                var itemAdd = character.InfoChar.LearnSkill.ItemSkill;
                                var time = character.InfoChar.LearnSkill.Time - ServerUtils.CurrentTimeMillis();
                                var skillTemplate = Cache.Gi().SKILL_TEMPLATES.FirstOrDefault(skill => skill.Id == character.InfoChar.LearnSkill.ItemTemplateSkillId);
                                var itemTempalte = ItemCache.ItemTemplate(itemAdd.Id);
                                var ngoc = 5;
                                if (time / 600000 >= 2)
                                {
                                    ngoc += (int)time / 600000;
                                }

                                var menu = string.Format(TextServer.gI().ADDING_SKILL, skillTemplate.Name,
                                    itemTempalte.Level, ServerUtils.GetTime(time));
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, menu, new List<string>() {$"Học\nCấp tốc\n{ngoc} ngọc", "Huỷ","Bỏ qua"}, character.InfoChar.Gender));
                                character.TypeMenu = 2;
                            }
                            else
                            {
                                var idShop = 11;
                                character.CharacterHandler.SendMessage(Service.Shop(character, 1, idShop));
                                character.ShopId = idShop;
                                character.TypeMenu = 2;
                            }
                            break;
                        }
                    }
                    break;
                }
                //Học skill
                case 1:
                {
                    switch (select)
                    {
                        //Đồng ý
                        case 0:
                        {
                            if(character.InfoChar.LearnSkillTemp == null) return;
                            var itemAdd = character.InfoChar.LearnSkillTemp.ItemSkill;
                            var time = character.InfoChar.LearnSkillTemp.Time + ServerUtils.CurrentTimeMillis();
                            var idSkill = character.InfoChar.LearnSkillTemp.ItemTemplateSkillId;
                            character.InfoChar.Potential -= itemAdd.BuyPotential;
                            character.InfoChar.LearnSkill = new LearnSkill()
                            {
                                ItemSkill = itemAdd,
                                Time = time,
                                ItemTemplateSkillId = idSkill,
                                Potential = (int)itemAdd.BuyPotential
                            };
                            character.InfoChar.LearnSkillTemp = null;
                            character.CharacterHandler.SendMessage(Service.MeLoadPoint(character));
                            character.CharacterHandler.SendMessage(Service.ClosePanel());
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, "Con đã học thành công, hãy cố gắng chờ đợi nha"));
                            break;
                        }
                        //Từ chối
                        case 1:
                        {
                            character.InfoChar.LearnSkillTemp = null;
                            break;
                        }
                    }
                    break;
                }
                //Open menu with learn skill
                case 2:
                {
                    switch (select)
                    {
                        //Đồng ý học nhanh
                        case 0:
                        {
                            if(character.InfoChar.LearnSkill == null) return;
                            var itemAdd = character.InfoChar.LearnSkill.ItemSkill;
                            var time = character.InfoChar.LearnSkill.Time - ServerUtils.CurrentTimeMillis();
                            var skillTemplate = Cache.Gi().SKILL_TEMPLATES.FirstOrDefault(skill => skill.Id == character.InfoChar.LearnSkill.ItemTemplateSkillId);
                            if (skillTemplate == null) return;
                            var ngoc = 5;
                            if (time / 600000 >= 2)
                            {
                                ngoc += (int)time / 600000;
                            }
                            if(character.AllDiamond() < ngoc) {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                                return;
                            }
                            character.MineDiamond(ngoc);
                            character.InfoChar.LearnSkill = null;
                            character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                            ItemHandler.AddLearnSkill(character, itemAdd, skillTemplate);
                            break;
                        }
                        //Huỷ học skill
                        case 1:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[3], MenuNpc.Gi().MenuMeo[1], character.InfoChar.Gender));
                            character.TypeMenu = 3;
                            break;
                        }
                        //Open menu 1
                        case 2:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuyLao[1], MenuNpc.Gi().MenuQuyLao[1], character.InfoChar.Gender));
                            character.TypeMenu = 0;
                            break;
                        }
                    }
                    break;
                }
                //Huỷ học skill
                case 3:
                {
                    if(select != 0) return;
                    var plusPoint = character.InfoChar.LearnSkill.Potential / 2;
                    character.CharacterHandler.PlusPoint(0, plusPoint, false);
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CANCEL_LEARN_SKILL));
                    character.InfoChar.LearnSkill = null;   
                    character.InfoChar.LearnSkillTemp = null;   
                    break;
                }
            }
        }

        private static void ConfirmThanMeo(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                //Menu ban đầu
                case 0: //Đổi điểm sự kiện
                {
                    switch (select)
                    {
                        case 0:
                        {
                            var thanMeo = MenuNpc.Gi().TextThanMeo[1];
                            thanMeo += $"\b{ServerUtils.Color("green")}Điểm sự kiện của bạn là: " + character.DiemSuKien;
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, thanMeo, MenuNpc.Gi().MenuThanMeo[1], character.InfoChar.Gender));
                            character.TypeMenu = 1;
                            break;
                        }
                        case 1:
                        {
                            // chọn đổi điểm tích nạp
                            break;
                        }
                    }
                    break;
                }
                case 1:
                {
                    if (select != 0) return;
                    //chấp nhận đổi điểm sự kiện
                    var mocDiem = 0;
                    var DiemSuKien = character.DiemSuKien;
                    var bagNull = character.LengthBagNull();
                    // - 300 điểm : x10 đá bảo vệ , 5 viên cs trung thu , 10tv
                    // - 500 điểm :  x15 đá bảo vệ , 10 viên cs trung thu .10tv
                    // - 1000 điểm : x20 đá bảo vệ , 25 viên cs trung thu , item lồng đèn tc , 20tv
                    // - 3000 điểm : x30 đá bảo vệ , x99 viên cs trung thu , item lồng đèn tc ,30 tv
                    // - 5000 điểm :x35 đá bảo vệ , x99 viên cs trung thu , item lồng đèn tc , 35tv , ngẫu nhiên item mèo mun , phóng lợn vv….
                    // - 7000 điểm :x50 đá bảo vệ , x99 viên cs trung thu , item lồng đèn , 40 tv , ngẫu nhiên item mèo mun , phóng lợn , pet cua vv…. , random trang bị hủy diệt 10% trở lên
                    // - 10000 điểm :x99 đá bảo vệ , x99 viên cs trung thu , item lồng đèn,50tv ,ngẫu nhiên item mèo mun , phóng lợn , pet cua vv…. , random trang bị hủy diệt 10% trở lên , cải trang vip nhất hiện tại.
                    if (DiemSuKien < 300)
                    {
                        character.CharacterHandler.SendMessage(
                            Service.DialogMessage("Bạn không đủ điểm để đổi"));
                        return;
                    }

                    if (DiemSuKien >= 300 && DiemSuKien <= 499)
                    {
                        if  (bagNull < 3)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                            return;
                        }
                        var thoivang = ItemCache.GetItemDefault((short)457);
                        thoivang.Quantity = 10;
                        character.CharacterHandler.AddItemToBag(true, thoivang, "Đổi điểm sự kiện tt 300 điểm");

                        var dabaove = ItemCache.GetItemDefault((short)987);
                        dabaove.Quantity = 10;
                        character.CharacterHandler.AddItemToBag(true, dabaove, "Đổi điểm sự kiện tt 300 điểm");

                        var cstrungthu = ItemCache.GetItemDefault((short)737);
                        cstrungthu.Quantity = 5;
                        character.CharacterHandler.AddItemToBag(true, cstrungthu, "Đổi điểm sự kiện tt 300 điểm");
                        character.CharacterHandler.SendMessage(Service.DialogMessage("Bạn đã đổi điểm sự kiện thành công, 10 tv, 10 dbv, 5 cs trung thu"));
                        mocDiem = 1;
                    }
                    else if (DiemSuKien >= 500 && DiemSuKien <= 999)
                    {
                        if  (bagNull < 3)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                            return;
                        }
                        var thoivang = ItemCache.GetItemDefault((short)457);
                        thoivang.Quantity = 10;
                        character.CharacterHandler.AddItemToBag(true, thoivang, "Đổi điểm sự kiện tt 500 điểm");

                        var dabaove = ItemCache.GetItemDefault((short)987);
                        dabaove.Quantity = 15;
                        character.CharacterHandler.AddItemToBag(true, dabaove, "Đổi điểm sự kiện tt 500 điểm");

                        var cstrungthu = ItemCache.GetItemDefault((short)737);
                        cstrungthu.Quantity = 10;
                        character.CharacterHandler.AddItemToBag(true, cstrungthu, "Đổi điểm sự kiện tt 500 điểm");
                        character.CharacterHandler.SendMessage(Service.DialogMessage("Bạn đã đổi điểm sự kiện thành công, 10 tv, 15 dbv, 10 cs trung thu"));
                        mocDiem = 2;
                    } 
                    else if (DiemSuKien >= 1000 && DiemSuKien <= 2999)
                    {
                        if  (bagNull < 4)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                            return;
                        }
                        var thoivang = ItemCache.GetItemDefault((short)457);
                        thoivang.Quantity = 20;
                        character.CharacterHandler.AddItemToBag(true, thoivang, "Đổi điểm sự kiện tt 1000 điểm");

                        var dabaove = ItemCache.GetItemDefault((short)987);
                        dabaove.Quantity = 20;
                        character.CharacterHandler.AddItemToBag(true, dabaove, "Đổi điểm sự kiện tt 1000 điểm");

                        var cstrungthu = ItemCache.GetItemDefault((short)737);
                        cstrungthu.Quantity = 25;
                        character.CharacterHandler.AddItemToBag(true, cstrungthu, "Đổi điểm sự kiện tt 1000 điểm");
                        mocDiem = 3;
                    } 
                    else if (DiemSuKien >= 3000 && DiemSuKien <= 4999)
                    {
                        if  (bagNull < 4)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                            return;
                        }
                        var thoivang = ItemCache.GetItemDefault((short)457);
                        thoivang.Quantity = 30;
                        character.CharacterHandler.AddItemToBag(true, thoivang, "Đổi điểm sự kiện tt 3000 điểm");

                        var dabaove = ItemCache.GetItemDefault((short)987);
                        dabaove.Quantity = 30;
                        character.CharacterHandler.AddItemToBag(true, dabaove, "Đổi điểm sự kiện tt 3000 điểm");

                        var cstrungthu = ItemCache.GetItemDefault((short)737);
                        cstrungthu.Quantity = 99;
                        character.CharacterHandler.AddItemToBag(true, cstrungthu, "Đổi điểm sự kiện tt 3000 điểm");
                        mocDiem = 4;
                    } 
                    else if (DiemSuKien >= 5000 && DiemSuKien <= 6999)
                    {
                        if  (bagNull < 5)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                            return;
                        }
                        var thoivang = ItemCache.GetItemDefault((short)457);
                        thoivang.Quantity = 35;
                        character.CharacterHandler.AddItemToBag(true, thoivang, "Đổi điểm sự kiện tt 5000 điểm");

                        var dabaove = ItemCache.GetItemDefault((short)987);
                        dabaove.Quantity = 35;
                        character.CharacterHandler.AddItemToBag(true, dabaove, "Đổi điểm sự kiện tt 5000 điểm");

                        var cstrungthu = ItemCache.GetItemDefault((short)737);
                        cstrungthu.Quantity = 99;
                        character.CharacterHandler.AddItemToBag(true, cstrungthu, "Đổi điểm sự kiện tt 5000 điểm");
                        mocDiem = 5;

                        var listitem = new List<short>() {993,995,996,997,998,999,1000,1001}; // item ngẫu nhiên
                        var itemrand = listitem[ServerUtils.RandomNumber(listitem.Count)];
                        var itemAdd = ItemCache.GetItemDefault(itemrand);
                        character.CharacterHandler.AddItemToBag(true, itemAdd, "Đổi điểm sự kiện tt 5000 điểm");
                    } 
                    else if (DiemSuKien >= 7000 && DiemSuKien <= 9999)
                    {
                        if  (bagNull < 6)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                            return;
                        }
                        var thoivang = ItemCache.GetItemDefault((short)457);
                        thoivang.Quantity = 40;
                        character.CharacterHandler.AddItemToBag(true, thoivang, "Đổi điểm sự kiện tt 7000 điểm");

                        var dabaove = ItemCache.GetItemDefault((short)987);
                        dabaove.Quantity = 50;
                        character.CharacterHandler.AddItemToBag(true, dabaove, "Đổi điểm sự kiện tt 7000 điểm");

                        var cstrungthu = ItemCache.GetItemDefault((short)737);
                        cstrungthu.Quantity = 99;
                        character.CharacterHandler.AddItemToBag(true, cstrungthu, "Đổi điểm sự kiện tt 7000 điểm");
                        mocDiem = 6;

                        var trunglinhthu = ItemCache.GetItemDefault((short)1049);
                        trunglinhthu.Quantity = 1;
                        character.CharacterHandler.AddItemToBag(true, trunglinhthu, "Đổi điểm sự kiện tt 7000 điểm");

                        var listitem = new List<short>() {993,995,996,997,998,999,1000,1001}; // item ngẫu nhiên
                        var itemrand = listitem[ServerUtils.RandomNumber(listitem.Count)];
                        var itemAdd = ItemCache.GetItemDefault(itemrand);
                        character.CharacterHandler.AddItemToBag(true, itemAdd, "Đổi điểm sự kiện tt 7000 điểm");

                        //trang bi huy diet
                        var chiso = ServerUtils.RandomNumber(10, 13);
                        switch (character.InfoChar.Gender)
                        {
                            case 0:
                            {
                                var listHD = new List<short>() {650,651,657,658,656}; // item ngẫu nhiên
                                var itemHDRand = listHD[ServerUtils.RandomNumber(listHD.Count)];
                                var itemHDAdd = ItemCache.GetItemDefault(itemHDRand);
                                itemHDAdd.Options.Where(option => DataCache.IdOptionGoc.Contains(option.Id)).ToList().ForEach(
                                    option => {
                                        option.Param += option.Param*chiso/100;
                                    });
                                character.CharacterHandler.AddItemToBag(true, itemHDAdd, "Đổi điểm sự kiện tt 7000 điểm");
                                break;
                            }
                            case 1:
                            {
                                var listHD = new List<short>() {652,653,659,660,656}; // item ngẫu nhiên
                                var itemHDRand = listHD[ServerUtils.RandomNumber(listHD.Count)];
                                var itemHDAdd = ItemCache.GetItemDefault(itemHDRand);
                                itemHDAdd.Options.Where(option => DataCache.IdOptionGoc.Contains(option.Id)).ToList().ForEach(
                                    option => {
                                        option.Param += option.Param*chiso/100;
                                    });
                                character.CharacterHandler.AddItemToBag(true, itemHDAdd, "Đổi điểm sự kiện tt 7000 điểm");
                                break;
                            }
                            case 2:
                            {
                                var listHD = new List<short>() {654,655,661,662,656}; // item ngẫu nhiên
                                var itemHDRand = listHD[ServerUtils.RandomNumber(listHD.Count)];
                                var itemHDAdd = ItemCache.GetItemDefault(itemHDRand);
                                itemHDAdd.Options.Where(option => DataCache.IdOptionGoc.Contains(option.Id)).ToList().ForEach(
                                    option => {
                                        option.Param += option.Param*chiso/100;
                                    });
                                character.CharacterHandler.AddItemToBag(true, itemHDAdd, "Đổi điểm sự kiện tt 7000 điểm");
                                break;
                            }
                        }
                    } 
                    else if (DiemSuKien >= 10000)
                    {
                        if  (bagNull < 6)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                            return;
                        }
                        var thoivang = ItemCache.GetItemDefault((short)457);
                        thoivang.Quantity = 50;
                        character.CharacterHandler.AddItemToBag(true, thoivang, "Đổi điểm sự kiện tt 10000 điểm");

                        var dabaove = ItemCache.GetItemDefault((short)987);
                        dabaove.Quantity = 99;
                        character.CharacterHandler.AddItemToBag(true, dabaove, "Đổi điểm sự kiện tt 10000 điểm");

                        var cstrungthu = ItemCache.GetItemDefault((short)737);
                        cstrungthu.Quantity = 99;
                        character.CharacterHandler.AddItemToBag(true, cstrungthu, "Đổi điểm sự kiện tt 10000 điểm");
                        mocDiem = 7;

                        var trunglinhthu = ItemCache.GetItemDefault((short)1049);
                        trunglinhthu.Quantity = 1;
                        character.CharacterHandler.AddItemToBag(true, trunglinhthu, "Đổi điểm sự kiện tt 10000 điểm");

                        var listitem = new List<short>() {993,995,996,997,998,999,1000,1001}; // item ngẫu nhiên
                        var itemrand = listitem[ServerUtils.RandomNumber(listitem.Count)];
                        var itemAdd = ItemCache.GetItemDefault(itemrand);
                        character.CharacterHandler.AddItemToBag(true, itemAdd, "Đổi điểm sự kiện tt 10000 điểm");

                        //trang bi huy diet
                        var chiso = ServerUtils.RandomNumber(10, 13);
                        switch (character.InfoChar.Gender)
                        {
                            case 0:
                            {
                                var listHD = new List<short>() {650,651,657,658,656}; // item ngẫu nhiên
                                var itemHDRand = listHD[ServerUtils.RandomNumber(listHD.Count)];
                                var itemHDAdd = ItemCache.GetItemDefault(itemHDRand);
                                itemHDAdd.Options.Where(option => DataCache.IdOptionGoc.Contains(option.Id)).ToList().ForEach(
                                    option => {
                                        option.Param += option.Param*chiso/100;
                                    });
                                character.CharacterHandler.AddItemToBag(true, itemHDAdd, "Đổi điểm sự kiện tt 10000 điểm");
                                break;
                            }
                            case 1:
                            {
                                var listHD = new List<short>() {652,653,659,660,656}; // item ngẫu nhiên
                                var itemHDRand = listHD[ServerUtils.RandomNumber(listHD.Count)];
                                var itemHDAdd = ItemCache.GetItemDefault(itemHDRand);
                                itemHDAdd.Options.Where(option => DataCache.IdOptionGoc.Contains(option.Id)).ToList().ForEach(
                                    option => {
                                        option.Param += option.Param*chiso/100;
                                    });
                                character.CharacterHandler.AddItemToBag(true, itemHDAdd, "Đổi điểm sự kiện tt 10000 điểm");
                                break;
                            }
                            case 2:
                            {
                                var listHD = new List<short>() {654,655,661,662,656}; // item ngẫu nhiên
                                var itemHDRand = listHD[ServerUtils.RandomNumber(listHD.Count)];
                                var itemHDAdd = ItemCache.GetItemDefault(itemHDRand);
                                itemHDAdd.Options.Where(option => DataCache.IdOptionGoc.Contains(option.Id)).ToList().ForEach(
                                    option => {
                                        option.Param += option.Param*chiso/100;
                                    });
                                character.CharacterHandler.AddItemToBag(true, itemHDAdd, "Đổi điểm sự kiện tt 10000 điểm");
                                break;
                            }
                        }
                    }
                    if (mocDiem >= 3)
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextThanMeo[2], MenuNpc.Gi().MenuThanMeo[2], character.InfoChar.Gender));
                        character.TypeMenu = 2;
                    }
                    character.CharacterHandler.SendMessage(Service.SendBag(character));
                    character.DiemSuKien = -10000;
                    break;
                }
                case 2://tự chọn lồng đèn
                {
                    switch (select)
                    {
                        case 0:
                        {
                            var ldKeoQuan = ItemCache.GetItemDefault((short)469);
                            ldKeoQuan.Quantity = 1;
                            character.CharacterHandler.AddItemToBag(true, ldKeoQuan, "Đổi điểm sự kiện tt tren 1k");
                            break;
                        }
                        case 1:
                        {
                            var ldOngSao = ItemCache.GetItemDefault((short)467);
                            ldOngSao.Quantity = 1;
                            character.CharacterHandler.AddItemToBag(true, ldOngSao, "Đổi điểm sự kiện tt tren 1k");
                            break;
                        }
                        case 2:
                        {
                            var ldCaChep = ItemCache.GetItemDefault((short)468);
                            ldCaChep.Quantity = 1;
                            character.CharacterHandler.AddItemToBag(true, ldCaChep, "Đổi điểm sự kiện tt tren 1k");
                            break;
                        }
                        case 3:
                        {
                            var ldConGa = ItemCache.GetItemDefault((short)802);
                            ldConGa.Quantity = 1;
                            character.CharacterHandler.AddItemToBag(true, ldConGa, "Đổi điểm sự kiện tt tren 1k");
                            break;
                        }
                        case 4:
                        {
                            var ldHoiAn = ItemCache.GetItemDefault((short)471);
                            ldHoiAn.Quantity = 1;
                            character.CharacterHandler.AddItemToBag(true, ldHoiAn, "Đổi điểm sự kiện tt tren 1k");
                            break;
                        }
                        default:
                        {
                            var ldKeoQuan = ItemCache.GetItemDefault((short)469);
                            ldKeoQuan.Quantity = 1;
                            character.CharacterHandler.AddItemToBag(true, ldKeoQuan, "Đổi điểm sự kiện tt tren 1k");
                            break;
                        }
                    }
                    character.CharacterHandler.SendMessage(Service.SendBag(character));
                    break;
                }
            }
        }

        private static void ConfirmThuongDe(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                //Menu ban đầu
                case 0:
                {
                    switch (select)
                    {
                        //Quay ngọc may mắn
                        case 0:
                        {
                            var menu = MenuNpc.Gi().MenuThuongDe[2].ToList();
                            if (character.LuckyBox.Count > 0)
                            {
                                menu.Insert(2, $"Rương phụ \n{character.LuckyBox.Count}\nmón"); 
                                menu.Insert(3, $"Xóa tất cả\nRương phụ"); 
                            }
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextThuongDe[2], menu, character.InfoChar.Gender));
                            character.TypeMenu = 1;
                            break;
                        }
                        case 1://Đến Kaio
                        {
                            var karin = new Karin();
                            karin.GetMapById(48)
                                .JoinZone(character, 0, true, true, character.InfoChar.Teleport);
                            break;
                        }
                    }
                    break;
                }
                //Quay ngọc may mắn
                case 1:
                {
                    switch (select)
                    {
                        case 0:
                        {
                            if (character.LuckyBox.Count >= DataCache.LIMIT_SLOT_RUONG_PHU_THUONG_DE)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().FULL_LUCKY_BOX));
                                break;
                            }
                            character.CharacterHandler.SendMessage(Service.LuckRoll0());
                            character.ShopId = 0;
                            break;
                        }
                        case 1:
                        {
                            // character.CharacterHandler.SendMessage(Service.LuckRoll0());
                            // character.ShopId = 1;
                            break;
                        }
                        case 2:
                        {
                            var luckRoll = character.LuckyBox;
                            if (character.LuckyBox.Count > 0)
                            {
                                character.CharacterHandler.SendMessage(Service.SubBox(luckRoll));
                                character.ShopId = 1111;
                            } 
                            break;
                        }
                        case 3:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextThuongDe[3], MenuNpc.Gi().MenuThuongDe[3], character.InfoChar.Gender));
                            character.TypeMenu = 2;
                            break;
                        }
                    }

                    break;
                }
                case 2:
                {
                    if(select != 0) return;
                    character.LuckyBox.Clear();
                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DELETED_ALL_LUCKY_BOX));
                    break;
                }
            }
        }
        
        private static void ConfirmThanVuTru(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                //Menu ban đầu
                case 0:
                {
                    if (select != 0) return;
                    var karin = new Karin();
                    karin.GetMapById(45)
                        .JoinZone(character, 0, true, true, character.InfoChar.Teleport);
                    break;
                }
            }
        }
        private static void ConfirmBaHatMit(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                //Menu vách núi
                case 0:
                {
                    if (!character.InfoChar.IsNhanBua) select += 1;
                    switch (select)
                    {
                        //Nhận bùa miễn phí
                        case 0:
                        {
                            var idAmulet = (short)DataCache.IdAmulet[ServerUtils.RandomNumber(DataCache.IdAmulet.Count)];
                            var timePlus = DataCache._1HOUR;
                            if (character.InfoChar.ItemAmulet.ContainsKey(idAmulet))
                            {
                                character.InfoChar.ItemAmulet[idAmulet] += timePlus;
                            }
                            else
                            {
                                character.InfoChar.ItemAmulet.TryAdd(idAmulet, timePlus + ServerUtils.CurrentTimeMillis());
                            }
                            character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().ADD_AMULET, ItemCache.ItemTemplate(idAmulet).Name)));
                            character.InfoChar.IsNhanBua = false;
                            // Setup Bùa
                            break;
                        }
                        //Cửa hàng bùa
                        case 1:
                        {
                            character.CharacterHandler.SendMessage(
                                Service
                                    .OpenUiConfirm(npcId, MenuNpc.Gi().TextBaHatMit[0], MenuNpc.Gi().MenuBaHatMit[2], character.InfoChar.Gender));
                            character.TypeMenu = 2;
                            break;
                        }
                        //Nâng cấp vật phẩm
                        case 2:
                        {
                            character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[4]));
                            character.ShopId = 0;
                            break;
                        }
                        //Làm phép nhập đá
                        case 3:
                        {
                            character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[5]));
                            character.ShopId = 1;
                            break;
                        }
                        //Nhập ngọc rồng
                        case 4:
                        {
                            character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[6]));
                            character.ShopId = 2;
                            break;
                        }
                        //Nâng cấp bông tai porata
                        case 5:
                        {
                            var bongTaiPorata2 = character.CharacterHandler.GetItemBagById(921);
                            if (bongTaiPorata2 == null)
                            {
                                character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[13]));
                                character.ShopId = 7;
                            }
                            else 
                            {
                                character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[14]));
                                character.ShopId = 8;
                            }
                            break;
                        }
                    }
                    break;
                }
                //Đảo kame
                case 1:
                {
                    switch (select)
                    {
                        //Ép sao trang bị
                        case 0:
                        {
                            character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[11]));
                            character.ShopId = 3;
                            break;
                        }
                        //MENU - Pha lê hoá trang bị
                        case 1:
                        {
                            character.CharacterHandler.SendMessage(
                                Service
                                    .OpenUiConfirm(npcId, MenuNpc.Gi().TextBaHatMit[2], MenuNpc.Gi().MenuBaHatMit[9], character.InfoChar.Gender));
                            character.TypeMenu = 3;
                            break;
                        }
                        //MENU - Chuyển hoá trang bị
                        case 2:
                        {
                            character.CharacterHandler.SendMessage(
                                Service
                                    .OpenUiConfirm(npcId, MenuNpc.Gi().TextBaHatMit[3], MenuNpc.Gi().MenuBaHatMit[10], character.InfoChar.Gender));
                            character.TypeMenu = 4;
                            break;
                        }
                    }
                    break;
                }
                //Cửa hàng bùa
                case 2:
                {
                    if (@select is < 0 or > 2) select = 0;
                    var idShop = select;
                    character.CharacterHandler.SendMessage(Service.Shop(character, 0, idShop));
                    character.ShopId = idShop;
                    character.TypeMenu = 0;
                    break;
                }
                //Menu Pha lê hoá
                case 3:
                {
                    if(select != 0) return;
                    character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[8]));
                    character.ShopId = 4;
                    break;
                }
                //MENU - Chuyển hoá trang bị
                case 4:
                {
                    switch (select)
                    {
                        case 0:
                        {
                            character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[12]));
                            character.ShopId = 5;
                            break;
                        }
                        case 1:
                        {
                            character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[12]));
                            character.ShopId = 6;
                            break;
                        }
                    }
                    break;
                }
                //Nâng cấp trang bị
                case 5:
                {
                    var listArray = character.CombinneIndex;
                    var dungDaBaoVe = listArray[5];
                    var daBaoVeItemIndex = listArray[6];
                    var daBaoVe = false;
                    if (select == 1 && dungDaBaoVe == 1 && daBaoVeItemIndex != -1)
                    {
                        daBaoVe = true;
                        Console.WriteLine("Co su dung da bao ve");
                    }
                    else if (select != 0)
                    {
                        return;
                    }
                   
                    var trangBi = character.CharacterHandler.GetItemBagByIndex(listArray[0]);
                    if (trangBi == null) return;
                    var da = character.CharacterHandler.GetItemBagByIndex(listArray[1]);
                    var soDaCanNangCap = listArray[2];
                    var gold = listArray[3];
                    var percentSuccess = listArray[4];
                    if (character.InfoChar.Gold < gold)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                        return;
                    }
                    if (da.Quantity < soDaCanNangCap)
                    {
                        character.CharacterHandler.SendMessage(
                            Service.DialogMessage(TextServer.gI().NEED_ENOUGH_STONE));
                        return;
                    }

                    var optionCheck = trangBi.Options.FirstOrDefault(option => option.Id == 72);
                    if (percentSuccess/DataCache.DIV_FAKE_PERCENT_UPGRADE >= 1)
                    {
                        percentSuccess /= DataCache.DIV_FAKE_PERCENT_UPGRADE;
                    }
                    var percentRandom = ServerUtils.RandomNumber(100) < percentSuccess;
                    if (percentRandom)
                    {
                        if (optionCheck == null)
                        {
                            trangBi.Options.Add(new OptionItem()
                            {
                                Id = 72,
                                Param = 1
                            });
                        }
                        else
                        {
                            optionCheck.Param += 1;
                        }
                        trangBi.Options.Where(option => DataCache.IdOptionGoc.Contains(option.Id)).ToList().ForEach(
                            option => {
                                option.Param += option.Param/10;
                            });
                            
                        character.CharacterHandler.SendMessage(Service.SendCombinne2());
                    }
                    else
                    {
                        if (optionCheck != null)
                        {
                            // – cấp 0 lên cấp 1 xịt hay lên ko ảnh hưởng gì hết. Xác suất 80%
                            // – cấp 1 lên cấp 2 xịt hay lên ko ảnh hưởng. Xác suất 50%
                            // – cấp 2 lên cấp 3 xịt bị rớt xuống cấp 1 và giảm 1% chỉ số. Xác suất 20%
                            // – cấp 3 lên 4 xịt k giảm cấp và chỉ số. Xác suất 10%
                            // – cấp 4 lên 5 xịt rớt xuống 3 giảm 1% chỉ số. Xác suất 5%
                            // – cấp 5 lên 6 xịt ko sao. Xác suất 2%
                            // – cấp 6 lên 7 xịt xuống 5 và giảm 1% chỉ số. Xác suất 1%

                            if (optionCheck.Param > 0 && optionCheck.Param % 2 == 0 && !daBaoVe)
                            {
                                optionCheck.Param -= 1;
                                trangBi.Options.Where(option => DataCache.IdOptionGoc.Contains(option.Id)).ToList().ForEach(
                                    option =>
                                    {
                                        option.Param -= option.Param/10;
                                    });
                            }
                            
                        }
                        character.CharacterHandler.SendMessage(Service.SendCombinne3());
                    }
                    character.MineGold(gold);
                    if (daBaoVe)
                    {
                        character.CharacterHandler.RemoveItemBagByIndex(daBaoVeItemIndex, 1, false, reason:"Dùng đá bảo vệ");
                        Console.WriteLine("Xoa da bao ve");
                    }

                    character.CharacterHandler.RemoveItemBagByIndex(da.IndexUI, soDaCanNangCap, reason:"Dùng đá nâng cấp");
                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                    character.CharacterHandler.SendMessage(Service.SendBag(character));

                    var checkDa = character.CharacterHandler.GetItemBagByIndex(listArray[1]);
                    var listIndexUi = new List<int>();
                    if (checkDa != null && checkDa.Id == da.Id)
                    {
                        listIndexUi.Add(trangBi.IndexUI);
                        listIndexUi.Add(da.IndexUI);
                    }
                    else
                    {
                        listIndexUi.Add(trangBi.IndexUI);
                    }
                    character.CharacterHandler.SendMessage(Service.SendCombinne1(listIndexUi));
                    character.CombinneIndex.Clear();
                    character.CombinneIndex = null;
                    
                    break;
                }
                //Nhập đá
                case 6:
                {
                    if(select != 0) return;
                    var bagNull = character.LengthBagNull();
                    var listArray = character.CombinneIndex;
                    var item1 = character.CharacterHandler.GetItemBagByIndex(listArray[0]);
                    var item2 = character.CharacterHandler.GetItemBagByIndex(listArray[1]);
                    var idNew = (short) (220 + ServerUtils.RandomNumber(5));
                    var itemNew = ItemCache.GetItemDefault(idNew);

                    var itemBagNotMax = character.CharacterHandler.ItemBagNotMaxQuantity(itemNew.Id);
                    if (itemBagNotMax == null && bagNull < 1)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                        return;
                    }
                    switch (item1.Id)
                    {
                        case 225:
                        {
                            character.CharacterHandler.RemoveItemBagByIndex(item1.IndexUI, 10, false, reason:"Nhập đá");
                            character.CharacterHandler.RemoveItemBagByIndex(item2.IndexUI, 1, false, reason:"Nhập đá");
                            break;
                        }
                        default:
                        {
                            character.CharacterHandler.RemoveItemBagByIndex(item1.IndexUI, 1, false, reason:"Nhập đá");
                            character.CharacterHandler.RemoveItemBagByIndex(item2.IndexUI, 10, false, reason:"Nhập đá");
                            break;
                        }
                    }
                    character.MineGold(2000);
                    character.CharacterHandler.AddItemToBag(true, itemNew, "Nhập đá");
                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                    character.CharacterHandler.SendMessage(Service.SendBag(character));

                    var listIndexUi = new List<int>();
                    var itemReturn = character.CharacterHandler.GetItemBagByIndex(item1.IndexUI);
                    if (itemReturn != null && itemReturn.Id == item1.Id)
                    {
                        listIndexUi.Add(item1.IndexUI);
                    }
                    itemReturn = character.CharacterHandler.GetItemBagByIndex(item2.IndexUI);
                    if (itemReturn != null && itemReturn.Id == item2.Id)
                    {
                        listIndexUi.Add(item2.IndexUI);
                    }
                    
                    character.CharacterHandler.SendMessage(Service.SendCombinne1(listIndexUi));
                    character.CharacterHandler.SendMessage(Service.SendCombinne4(ItemCache.ItemTemplate(itemNew.Id).IconId));
                    character.CombinneIndex.Clear();
                    character.CombinneIndex = null;
                    break;
                }
                //Nhập ngọc rông
                case 7:
                {
                    if(select != 0) return;
                    var bagNull = character.LengthBagNull();
                    var listArray = character.CombinneIndex;
                    if (listArray == null) return;
                    var ngocRong = character.CharacterHandler.GetItemBagByIndex(listArray[0]);
                    var idNew = (short) (ngocRong.Id - 1);
                    var itemNew = ItemCache.GetItemDefault(idNew);

                    var itemBagNotMax = character.CharacterHandler.ItemBagNotMaxQuantity(itemNew.Id);
                    if (itemBagNotMax == null && bagNull < 1)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                        return;
                    }
                    character.MineGold(2000);
                    character.CharacterHandler.RemoveItemBagByIndex(ngocRong.IndexUI, 7, reason:"Nhập ngọc");
                    character.CharacterHandler.AddItemToBag(true, itemNew, "Nhập ngọc");
                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                    character.CharacterHandler.SendMessage(Service.SendBag(character));

                    character.CharacterHandler.SendMessage(Service.SendCombinne5(ItemCache.ItemTemplate(itemNew.Id).IconId));

                    var listIndexUi = new List<int>();
                    var itemReturn = character.CharacterHandler.GetItemBagByIndex(ngocRong.IndexUI);
                    if (itemReturn != null && itemReturn.Id == ngocRong.Id)
                    {
                        listIndexUi.Add(ngocRong.IndexUI);
                    }

                    character.CharacterHandler.SendMessage(Service.SendCombinne1(listIndexUi));
                    character.CombinneIndex.Clear();
                    character.CombinneIndex = null;
                    break;
                } 
                //Ép sao trang bị
                case 8:
                {
                    if(select != 0) return;
                    if (10 > character.AllDiamond())
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                        return;
                    }
                    var bagNull = character.LengthBagNull();
                    var listArray = character.CombinneIndex;
                    if (listArray == null) return;
                    var trangBi = character.CharacterHandler.GetItemBagByIndex(listArray[0]);
                    var ngocRong = character.CharacterHandler.GetItemBagByIndex(listArray[1]);
                    var optionId = listArray[2];
                    var optionParam = listArray[3];
                    if(trangBi == null || ngocRong == null) return;

                    var optionCheck = trangBi.Options.FirstOrDefault(opt => opt.Id == 102);
                    var optionUp = trangBi.Options.FirstOrDefault(opt => opt.Id == optionId);
                    if (optionCheck == null)
                    {
                        trangBi.Options.Add(new OptionItem()
                        {
                            Id = 102,
                            Param = 1
                        });
                    }
                    else
                    {
                        optionCheck.Param++;
                    }

                    if (optionUp == null)
                    {
                        trangBi.Options.Add(new OptionItem()
                        {
                            Id = optionId,
                            Param = optionParam
                        });
                    }
                    else
                    {
                        optionUp.Param += optionParam;
                    }
                    character.MineDiamond(10);
                    character.CharacterHandler.SendMessage(Service.SendCombinne2());
                    character.CharacterHandler.RemoveItemBagByIndex(ngocRong.IndexUI, 1, reason:"Ép ngọc rồng");
                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                    character.CharacterHandler.SendMessage(Service.SendBag(character));
                    var itemReturn = character.ItemBag.FirstOrDefault(item =>
                        item.Id == trangBi.Id && item.Options.Count == trangBi.Options.Count && item.IndexUI != trangBi.IndexUI) ?? trangBi;
                    character.CharacterHandler.SendMessage(Service.SendCombinne1(new List<int>(){itemReturn.IndexUI}));
                    character.CombinneIndex.Clear();
                    character.CombinneIndex = null;
                    break;
                }
                //Pha lê hoá trang bị
                case 9:
                {
                    if(select != 0) return;
                    var listArray = character.CombinneIndex;
                    if (listArray == null) return;
                    var itemBag = character.CharacterHandler.GetItemBagByIndex(listArray[0]);
                    var lvOption = listArray[1];
                    if(itemBag == null) return;
                    var percentPhaLe = DataCache.PercentPhaLe[lvOption];
                    var goldPhaLe = percentPhaLe[0] * 1000000;
                    var diamondPhaLe = percentPhaLe[2];
                    if (character.InfoChar.Gold < goldPhaLe)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                        return;
                    }
                    if (character.AllDiamond() < diamondPhaLe)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                        return;
                    }

                    var percentSuccess = ServerUtils.RandomNumber(1, 100) <= (percentPhaLe[1]/DataCache.DIV_FAKE_PERCENT_PL);
                    if (percentSuccess)
                    {
                        var optionPlus = itemBag.Options.FirstOrDefault(option => option.Id == 107);
                        if (optionPlus != null)
                        {
                            optionPlus.Param++;
                        }
                        else
                        {
                            itemBag.Options.Add(new OptionItem()
                            {
                                Id = 107,
                                Param = 1
                            });
                        }
                        character.CharacterHandler.SendMessage(Service.SendCombinne2());
                    }
                    else
                    {
                        character.CharacterHandler.SendMessage(Service.SendCombinne3());
                    }
                    character.MineGold(goldPhaLe);
                    character.MineDiamond(diamondPhaLe);
                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                    character.CharacterHandler.SendMessage(Service.SendBag(character));
                    character.CharacterHandler.SendMessage(Service.SendCombinne1(new List<int>(){itemBag.IndexUI}));
                    character.CombinneIndex.Clear();
                    character.CombinneIndex = null;
                    break;
                }
                //Chuyển hoá trang bị VÀNG / 10
                //Chuyển hoá trang bị NGỌC / 11
                case 10:
                case 11:
                {
                    if(select != 0) return;
                    var listArray = character.CombinneIndex;
                    var itemLuongLong = character.CharacterHandler.GetItemBagByIndex(listArray[0]); //old
                    var itemThan = character.CharacterHandler.GetItemBagByIndex(listArray[1]); //new đồ thần
                    var levelUp = listArray[2];
                    var checkMoney = listArray[3];
                    if(itemLuongLong == null || itemThan == null) return;
                    switch (character.TypeMenu)
                    {
                        case 10:
                        {
                            if (character.InfoChar.Gold < checkMoney)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.DialogMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                                return;
                            }
                            else
                            {
                                character.MineGold(checkMoney);
                            }
                            break;
                        }
                        case 11:
                        {
                            if (character.AllDiamond() < checkMoney)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.DialogMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                                return;
                            }
                            else
                            {
                                character.MineDiamond(checkMoney);
                            }
                            break;
                        }
                    }

                    var checkLevel = itemLuongLong.Options.FirstOrDefault(opt => opt.Id == 72)?.Param;
                    
                    var listOptionLlGoc = itemLuongLong.Options.Where(opt => DataCache.IdOptionGoc.Contains(opt.Id)).ToList();
                    itemThan.Options.ForEach(opt =>
                    {
                        var paramNew = 0;
                        var optCheck = listOptionLlGoc.FirstOrDefault(o => o.Id == opt.Id);
                        if (optCheck == null) return;
                        if (checkLevel == levelUp)
                        {
                            paramNew += optCheck.Param;
                        }
                        else
                        {
                            paramNew += optCheck.Param - optCheck.Param / 10;
                        }
                        opt.Param += paramNew;
                    });
                    var listCheckPlus = itemLuongLong.Options.Where(opt => itemThan.Options.FirstOrDefault(o => o.Id == opt.Id) == null).ToList();
                    itemThan.Options.AddRange(listCheckPlus);

                    character.CharacterHandler.RemoveItemBag(itemLuongLong.IndexUI, reason:"Chuyển hóa");
                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                    character.CharacterHandler.SendMessage(Service.SendBag(character));
                    character.CharacterHandler.SendMessage(Service.SendCombinne4(ItemCache.ItemTemplate(itemThan.Id).IconId));
                    
                    var itemReturn = character.ItemBag.FirstOrDefault(item =>
                        item.Id == itemThan.Id && item.Options.Count == itemThan.Options.Count &&
                        item.IndexUI != itemThan.IndexUI) ?? itemThan;
                    character.CharacterHandler.SendMessage(Service.SendCombinne1(new List<int>(){itemReturn.IndexUI}));
                    character.CombinneIndex.Clear();
                    character.CombinneIndex = null;
                    break;
                }
                //Nâng cấp porata
                case 12:
                {
                    if(select != 0) return;
                    
                    var listArray = character.CombinneIndex;
                    var bongTaiPorata = character.CharacterHandler.GetItemBagByIndex(listArray[0]);
                    var manhVoBongTai = character.CharacterHandler.GetItemBagByIndex(listArray[1]);
                    var soNgocCanNangCap = listArray[2];
                    var soVangCanNangCap = listArray[3];
                    var percentSuccess = listArray[4];
                    var isThanhCong = false;

                    if (character.InfoChar.Gold < soVangCanNangCap)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                        return;
                    }
                    if (character.AllDiamond() < soNgocCanNangCap)
                    {
                        character.CharacterHandler.SendMessage(Service.DialogMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                        return;
                    }

                    var optionCheck = manhVoBongTai.Options.FirstOrDefault(option => option.Id == 31);
                    var percentRandom = ServerUtils.RandomNumber(100) < percentSuccess;
                    if (percentRandom)
                    {
                        // Thành công thì xóa số lượng 9999 item, xóa item bông tai và thêm item bông tai 2
                        if (optionCheck != null)
                        {
                            optionCheck.Param -= 9999;
                            if (optionCheck.Param <= 0)
                            {
                                character.CharacterHandler.RemoveItemBagByIndex(manhVoBongTai.IndexUI, 1, false, reason:"NC Porata");
                            }
                        }
                        character.CharacterHandler.RemoveItemBagByIndex(bongTaiPorata.IndexUI, 1, false, reason:"NC Porata");
                        var itemAdd = ItemCache.GetItemDefault(921);
                        itemAdd.Quantity = 1;
                        character.CharacterHandler.AddItemToBag(false, itemAdd, "Nâng cấp porata");
                        character.CharacterHandler.SendMessage(Service.SendCombinne2());
                        isThanhCong = true;
                    }
                    else
                    {
                        if (optionCheck != null)
                        {
                            optionCheck.Param -= 99;
                        }
                        character.CharacterHandler.SendMessage(Service.SendCombinne3());
                    }

                    character.MineGold(soVangCanNangCap);
                    character.MineDiamond(soNgocCanNangCap);
                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                    character.CharacterHandler.SendMessage(Service.SendBag(character));

                    var checkManhVoBongTai = character.CharacterHandler.GetItemBagByIndex(listArray[1]);
                    var listIndexUi = new List<int>();
                    if (!isThanhCong)
                    {
                        if (checkManhVoBongTai != null && checkManhVoBongTai.Id == manhVoBongTai.Id)
                        {
                            listIndexUi.Add(bongTaiPorata.IndexUI);
                            listIndexUi.Add(manhVoBongTai.IndexUI);
                        }
                        else
                        {
                            listIndexUi.Add(bongTaiPorata.IndexUI);
                        }
                    }
                    character.CharacterHandler.SendMessage(Service.SendCombinne1(listIndexUi));
                    character.CombinneIndex.Clear();
                    character.CombinneIndex = null;
                    break;
                }
                // Mở option porata
                case 13:
                {
                    if(select != 0) return;
                    
                    var listArray = character.CombinneIndex;
                    var bongTaiPorata2 = character.CharacterHandler.GetItemBagByIndex(listArray[0]);
                    var manhHonBongTai = character.CharacterHandler.GetItemBagByIndex(listArray[1]);
                    var daXanhLam = character.CharacterHandler.GetItemBagByIndex(listArray[2]);
                    var soNgocCanNangCap = listArray[3];
                    var soVangCanNangCap = listArray[4];
                    var percentSuccess = listArray[5];

                    if (character.InfoChar.Gold < soVangCanNangCap)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                        return;
                    }
                    if (character.AllDiamond() < soNgocCanNangCap)
                    {
                        character.CharacterHandler.SendMessage(Service.DialogMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                        return;
                    }
                    // remove item đá xanh lam, và 99 cái mảnh hồn
                    character.CharacterHandler.RemoveItemBagByIndex(manhHonBongTai.IndexUI, 99, false, reason:"CS Porata");
                    character.CharacterHandler.RemoveItemBagByIndex(daXanhLam.IndexUI, 1, false, reason:"CS Porata");
                    // Remove tiền và ngọc
                    character.MineGold(soVangCanNangCap);
                    character.MineDiamond(soNgocCanNangCap);

                    var optionCheck = bongTaiPorata2.Options.FirstOrDefault(option => option.Id != 72);

                    var percentRandom = ServerUtils.RandomNumber(100) < percentSuccess;
                    if (percentRandom)
                    {
                        var optionRandom = DataCache.OptionPorata2[ServerUtils.RandomNumber(DataCache.OptionPorata2.Count)]; 
                        // Thành công thì lấy random option trong list
                        if (optionCheck != null)
                        {
                            optionCheck.Id = optionRandom[0];
                            optionCheck.Param = ServerUtils.RandomNumber(optionRandom[1], optionRandom[2]);
                        }
                        else 
                        {
                            bongTaiPorata2.Options.Add(new OptionItem()
                            {
                                Id = optionRandom[0],
                                Param = ServerUtils.RandomNumber(optionRandom[1], optionRandom[2])
                            });
                        }
                        character.CharacterHandler.SendMessage(Service.SendCombinne2());
                    }
                    else
                    {
                        character.CharacterHandler.SendMessage(Service.SendCombinne3());
                    }

                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                    character.CharacterHandler.SendMessage(Service.SendBag(character));

                    var checkBongTaiPorata2 = character.CharacterHandler.GetItemBagByIndex(listArray[0]);
                    var listIndexUi = new List<int>();
                    if (checkBongTaiPorata2 != null && checkBongTaiPorata2.Id == bongTaiPorata2.Id)
                    {
                        listIndexUi.Add(bongTaiPorata2.IndexUI);
                    }

                    character.CharacterHandler.SendMessage(Service.SendCombinne1(listIndexUi));
                    character.CombinneIndex.Clear();
                    character.CombinneIndex = null;
                    break;
                }
                case 14:
                {
                    //menu linh thú
                    switch (select)
                    {
                        case 0://nở trứng linh thú
                        {
                            character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[16]));
                            character.ShopId = 9;
                            break;
                        }
                        case 1://nâng cấp linh thú
                        {
                            character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[17]));
                            character.ShopId = 10;
                            break;
                        }
                        case 2://nâng cấp linh thú
                        {
                            character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[18]));
                            character.ShopId = 11;
                            break;
                        }
                        case 3://nâng cấp linh thú
                        {
                            character.CharacterHandler.SendMessage(Service.SendCombinne0(MenuNpc.Gi().MenuBaHatMit[19]));
                            character.ShopId = 12;
                            break;
                        }
                    }
                    break;
                }
                case 15://nở trứng
                {
                    var listArray = character.CombinneIndex;

                    var trungLinhThu = character.CharacterHandler.GetItemBagByIndex(listArray[0]);
                    var honLinhThu = character.CharacterHandler.GetItemBagByIndex(listArray[1]);
                    short trungLinhThuIcon = ItemCache.ItemTemplate(trungLinhThu.Id).IconId;

                    character.CharacterHandler.RemoveItemBagByIndex(trungLinhThu.IndexUI, 1, false, reason:"Nở trứng");
                    character.CharacterHandler.RemoveItemBagByIndex(honLinhThu.IndexUI, 99, false, reason:"Nở trứng");

                    if (listArray.Count == 3)
                    {
                        var thoiVang = character.CharacterHandler.GetItemBagByIndex(listArray[2]);
                        character.CharacterHandler.RemoveItemBagByIndex(thoiVang.IndexUI, 5, false, reason:"Nở trứng nhanh");
                    }

                    var linhThuNgauNhien = DataCache.ListPetD[ServerUtils.RandomNumber(DataCache.ListPetD.Count)];
                    var itemLinhThu = ItemCache.GetItemDefault(linhThuNgauNhien);
                    
                    var maSoLinhThu = ServerUtils.RandomNumber(100,100000);
                    var optionHiden = itemLinhThu.Options.FirstOrDefault(option => option.Id == 73);
                            
                    if (optionHiden != null) 
                    {
                        optionHiden.Param = maSoLinhThu;
                    }
                    else 
                    {
                        itemLinhThu.Options.Add(new OptionItem()
                        {
                            Id = 73,
                            Param = maSoLinhThu,
                        });
                    }
                    
                    character.CharacterHandler.SendMessage(Service.NpcChat(npcId, TextServer.gI().RANDOM_LINH_THU));
                    character.CharacterHandler.SendMessage(Service.SendCombinne6(trungLinhThuIcon, ItemCache.ItemTemplate(itemLinhThu.Id).IconId));

                    character.CharacterHandler.AddItemToBag(false, itemLinhThu, "Nở trứng");
                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                    character.CharacterHandler.SendMessage(Service.SendBag(character));

                    character.CharacterHandler.SendMessage(Service.SendCombinne1(new List<int>()));
                    character.CombinneIndex.Clear();
                    character.CombinneIndex = null;
                    break;
                }
            }
        }
        
        private static void ConfirmGhiDanh(Character character, short npcId, int select)
        {
            throw new NotImplementedException();
        }

        private static void ConfirmRongThan(Character character, short npcId, int select)
        {
            // character.CharacterHandler.SendMessage(Service.ServerMessage("select: " + select));
            if (!character.InfoMore.VuaGoiRong) {
                UserDB.BanUser(character.Player.Id);
                ClientManager.Gi().KickSession(character.Player.Session);
                ServerUtils.WriteLog("hackrong", $"Tên tài khoản {character.Player.Username} (ID:{character.Player.Id}) hack rong");

                var temp = ClientManager.Gi().GetPlayer(character.Player.Id);
                if (temp != null)
                {
                    ClientManager.Gi().KickSession(temp.Session);
                }
                return;
            }
            character.InfoMore.VuaGoiRong = false;
            switch (select)
            {
                case 0: //2 ty vang
                {
                    character.PlusGold(2000000000);
                    character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                    break;
                }
                case 1://+1 gang tay tren nguoi
                {
                    var trangBi = character.ItemBody[2];

                    if (trangBi == null) 
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage("Trên người của bạn không có găng tay"));
                        break;
                    }

                    var optionCheck = trangBi.Options.FirstOrDefault(option => option.Id == 72);
                    if (optionCheck == null)
                    {
                        trangBi.Options.Add(new OptionItem()
                        {
                            Id = 72,
                            Param = 1
                        });
                        trangBi.Options.Where(option => DataCache.IdOptionGoc.Contains(option.Id)).ToList().ForEach(
                                        option => option.Param += option.Param/10);
                        character.CharacterHandler.SendMessage(Service.SendBody(character));
                    }
                    else
                    {
                        if (optionCheck.Param < DataCache.MAX_LIMIT_UPGRADE - 1)
                        {
                            optionCheck.Param += 1;
                            trangBi.Options.Where(option => DataCache.IdOptionGoc.Contains(option.Id)).ToList().ForEach(
                                        option => option.Param += option.Param/10);
                            character.CharacterHandler.SendMessage(Service.SendBody(character));
                        }
                    }
                    break;
                }
                case 2://Doi ky nang de tu
                {
                    var disciple = character.Disciple;
                    var disciplePower = disciple.InfoChar.Power;
                    if (disciplePower >= 150000000 && disciple.Skills.Count >= 2)
                    {
                        var randomSkill = DataCache.IdSkillDisciple2[ServerUtils.RandomNumber(DataCache.IdSkillDisciple2.Count)];
                        disciple.Skills[1] = new SkillCharacter()
                        {
                            Id = randomSkill,
                            SkillId = Disciple.GetSkillId(randomSkill),
                            Point = 1,
                        };
                    }

                    if (disciplePower >= 1500000000 && disciple.Skills.Count >= 3)
                    {
                        var randomSkill = DataCache.IdSkillDisciple3[ServerUtils.RandomNumber(DataCache.IdSkillDisciple3.Count)];
                        disciple.Skills[2] = new SkillCharacter()
                        {
                            Id = randomSkill,
                            SkillId = Disciple.GetSkillId(randomSkill),
                            Point = 1,
                        };
                    }
                    break;
                }
                case 3://dep trai nhat vu tru
                {
                    var itemId = (character.InfoChar.Gender + 227);
                    var itemAdd = ItemCache.GetItemDefault((short)itemId);
                    itemAdd.Quantity = 1;
                    character.CharacterHandler.AddItemToBag(true, itemAdd, "Ước NR");
                    character.CharacterHandler.SendMessage(Service.SendBag(character));
                    break;
                }
            }
            // character.Zone.ZoneHandler.SendMessage(Service.CallDragon(1, 0, character));
            character.CharacterHandler.SendMessage(Service.CallDragon(1, 0, character));
            MapManager.SetDragonAppeared(false);
        }
        
        private static void ConfirmCalich(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                case 0: 
                {
                    switch(select)
                    {
                        case 0://Nói chuyện
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, MenuNpc.Gi().TextCalich[1]));
                            break;
                        }
                        case 1:
                        {
                            character.InfoMore.TransportMapId = 102;
                            character.CharacterHandler.SendMessage(Service.Transport(20));
                            // đến tương lai
                            break;
                        }
                    }
                    break;
                }
                case 1:
                {
                    if (select != 0) return;
                    character.InfoMore.TransportMapId = 24;
                    character.CharacterHandler.SendMessage(Service.Transport(20));
                    break;
                }
            }
        }

        private static void ConfirmSanta(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                case 0: 
                {
                    switch(select)
                    {
                        case 0:
                        {
                            var inputGiftcode = new List<InputBox>();
                            var inputCode = new InputBox(){
                                Name = "Nhập mã quà tặng",
                                Type = 1,
                            };
                            inputGiftcode.Add(inputCode);
                            character.CharacterHandler.SendMessage(Service.ShowInput("Giftcode Ngọc Rồng SUPER HERO", inputGiftcode));
                            character.TypeInput = 1;
                            break;
                        }
                        case 1:
                        {
                            var idShop = 18 + character.InfoChar.Gender;
                            character.CharacterHandler.SendMessage(Service.Shop(character, 0, idShop));
                            character.ShopId = idShop;
                            character.TypeMenu = 0;
                            break;
                        }
                        case 2:
                        {
                            var idShop = 3 + character.InfoChar.Gender;
                            character.CharacterHandler.SendMessage(Service.Shop(character, 0, idShop));
                            character.ShopId = idShop;
                            character.TypeMenu = 0;
                            break;
                        }
                        case 3:
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, string.Format(MenuNpc.Gi().TextSanta[2], (UserDB.GetVND(character.Player)/1000)), MenuNpc.Gi().MenuSanta[2], character.InfoChar.Gender));
                            character.TypeMenu = 2;
                            break;
                        }
                        case 4:
                        {
                            if (character.InfoChar.LimitGold >= DataCache.MAX_LIMIT_GOLD)
                            {
                                character.CharacterHandler.SendMessage(Service.NpcChat(npcId, TextServer.gI().MAX_LIMIT_GOLD));
                                return;
                            }
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, string.Format(MenuNpc.Gi().TextSanta[1], ServerUtils.GetMoneys(character.InfoChar.LimitGold)), MenuNpc.Gi().MenuSanta[1], character.InfoChar.Gender));
                            character.TypeMenu = 1;
                            break;
                        }
                        default:
                        {
                            character.CharacterHandler.SendMessage(Service.NpcChat(npcId, TextServer.gI().UPDATING));
                            break;
                        }
                    }
                    break;
                }
                case 1: //Nâng giới hạn vàng
                {
                    if (character.InfoChar.Gold < 200000000)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                        return;
                    }
                    character.MineGold(200000000);
                    character.PlusLimitGold(200000000);
                    character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                    character.CharacterHandler.SendMessage(Service.NpcChat(npcId, string.Format(TextServer.gI().UPGRADE_LIMIT_GOLD, ServerUtils.GetMoneys(character.InfoChar.LimitGold))));
                    break;
                }
                case 2:
                {
                    switch(select)
                    {
                        case 0:
                        {
                            int vnd = UserDB.GetVND(character.Player);

                            var soLuongThoiVang = vnd/1000;
                            var soLuongVNDSuDung = soLuongThoiVang*1000;
                            var soLuongBagCanDung = 1;
                            
                            if (!character.InfoChar.IsPremium)
                            {
                                int tongVND = UserDB.GetTongVND(character.Player);
                                if (tongVND >= 20000)
                                {
                                    character.InfoChar.IsPremium = true;
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().UPGRADE_TO_PREMIUM));
                                }
                            }

                            if (vnd > 0 && vnd >= soLuongVNDSuDung) 
                            {
                                // var list = new List<InputBox>();
                                // var inputAmount = new InputBox(){
                                //     Name = TextServer.gI().INPUT_VND_TO_GOLD,
                                //     Type = 0,
                                // };
                                // list.Add(inputAmount);
                                // character.CharacterHandler.SendMessage(Service.ShowInput(string.Format(TextServer.gI().LABEL_VND_TO_GOLD, ServerUtils.GetMoneys(vnd)), list));
                                // character.TypeInput = 1999;
                                var bagNull = character.LengthBagNull();
                                var itemNew = ItemCache.GetItemDefault(457);
                                itemNew.Quantity = soLuongThoiVang;
                                if (bagNull < soLuongBagCanDung)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                                    return;
                                }

                                if (UserDB.MineVND(character.Player, soLuongVNDSuDung))
                                {
                                    if(character.CharacterHandler.AddItemToBag(true, itemNew, "Đổi vàng")) {
                                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                                        character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().GET_GOLD_BAR, soLuongThoiVang)));
                                    }
                                }

                            }
                            else 
                            {
                                character.CharacterHandler.SendMessage(Service.NpcChat(npcId, TextServer.gI().NAP_VANG));
                            }
                            break;
                        }
                        case 1:
                        {
                            if (!character.InfoChar.IsPremium)
                            {
                                int tongVND = UserDB.GetTongVND(character.Player);
                                if (tongVND >= 20000)
                                {
                                    character.InfoChar.IsPremium = true;
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().UPGRADE_TO_PREMIUM));
                                }
                                else 
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NAP_VANG));
                                }
                            }
                            else 
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn hiện đang là thành viên chính thức"));
                            }
                            break;
                        }
                    }

                    break;
                }
            }
        }

        private static void ConfirmTrungThu(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                //Menu chính
                case 0:
                {
                    switch (select)
                    {
                        case 0://shop
                        {
                            character.CharacterHandler.SendMessage(Service.Shop(character, 0, 23));
                            character.ShopId = 23;
                            break;
                        }
                        case 1:
                        {
                            var bangXepHangTrungThu = Server.Gi().BangXepHang.GetList();
                            bangXepHangTrungThu += $"\b{ServerUtils.Color("red")}Điểm sự kiện của bạn là: " + character.DiemSuKien;
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, bangXepHangTrungThu));
                            break;
                        }
                        case 2:
                        {
                            var bangXepHangTopNap = Server.Gi().BangXepHang.GetListTopNap();
                            bangXepHangTopNap += $"\b{ServerUtils.Color("red")}Điểm tích nạp của bạn là: " + character.Player.DiemTichNap;
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(npcId, bangXepHangTopNap));
                            break;
                        }
                    }
                    break;
                }
            }
        }
        
        private static void ConfirmQuocVuong(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                //Menu chính
                case 0:
                {
                    switch (select)
                    {
                        case 0:
                        {
                            try
                            {
                                var limit = character.InfoChar.LitmitPower;
                                var LM = Cache.Gi().LIMIT_POWERS[limit];
                                var ngoc = 100*(limit+1); 
                                var text = string.Format(TextServer.gI().UPGRADE_LEVEL_ME, ServerUtils.GetPower(LM.Power), ngoc);
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, text, MenuNpc.Gi().MenuQuocVuong[1], character.InfoChar.Gender));
                                character.TypeMenu = 1;
                            }
                            catch (Exception)
                            {
                                character.CharacterHandler.SendMessage(Service.NpcChat(npcId, "Con đã đạt giới hạn tối đa"));
                            }
                            break;
                        }
                        case 1:
                        {
                            break;
                        }
                    }
                    break;
                }
                case 1:
                {
                    switch (select)
                    {
                        case 0:
                        {
                            break;
                        }
                        case 1:
                        {
                            var limit = character.InfoChar.LitmitPower;
                            if (limit >= DataCache.MAX_LIMIT_POWER_LEVEL - 1)
                            {
                                character.CharacterHandler.SendMessage(Service.NpcChat(npcId, "Con đã đạt giới hạn tối đa"));
                                return;
                            }

                            var ngoc = 100*(limit+1);
                            if (ngoc > character.AllDiamond())
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                                return;
                            }

                            character.InfoChar.IsPower = true;
                            character.InfoChar.LitmitPower += 1;
                            character.MineDiamond(ngoc);
                            character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                            character.CharacterHandler.SendMessage(Service.NpcChat(npcId, "Chúc mừng con đạt tới sức mạnh mới"));
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private static void ConfirmGiuMa(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                //Menu chính
                case 0:
                {
                    if (select != 0) return;
                    character.InfoMore.TransportMapId = 156;
                    character.CharacterHandler.SendMessage(Service.Transport(3, 1));
                    break;
                }   
            }
        }

        private static void ConfirmQuaTrung(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                case 0://Chua đủ thời gian
                {
                    switch(select)
                    {
                        case 0://Chờ, bỏ qua
                        {
                            break;
                        }
                        case 1://Dùng tiền để nở trứng
                        {
                            var disciple = character.Disciple;
                            if (disciple != null)
                            {
                                var itemDiscipleBody = disciple.ItemBody.FirstOrDefault(item => item != null);

                                if (itemDiscipleBody != null)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().PLEASE_EMPTY_DISCIPLE_BODY));
                                    return;
                                }
                            }
                            // Kiểm tra trạng thái hợp thể
                            if (character.InfoChar.Fusion.IsFusion)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().PLEASE_NOT_FUSION));
                                return;
                            }

                            if (character.InfoChar.Gold < DataCache.GIA_NO_TRUNG_MA_BU)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                                return;
                            }

                            // Kiểm tra sức mạnh đệ tử 20 tỷ
                            if (character.Disciple != null && character.Disciple.InfoChar.Power < 160000000)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DISCIPLE_NOT_ENOUGH_POWER_TO_OPEN_EGG));
                                return;
                            }

                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuaTrung[1], MenuNpc.Gi().MenuQuaTrung[2], character.InfoChar.Gender));
                            character.TypeMenu = 2;
                            break;
                        }
                    }
                    break;
                }
                case 1: //Menu đủ thời gian
                {
                    switch(select)
                    {
                        case 0: //Nở trứng
                        {
                            var disciple = character.Disciple;
                            if (disciple != null)
                            {
                                var itemDiscipleBody = disciple.ItemBody.FirstOrDefault(item => item != null);

                                if (itemDiscipleBody != null)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().PLEASE_EMPTY_DISCIPLE_BODY));
                                    return;
                                }
                            }
                            // Kiểm tra trạng thái hợp thể
                            if (character.InfoChar.Fusion.IsFusion)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().PLEASE_NOT_FUSION));
                                return;
                            }

                            if (character.Disciple != null && character.Disciple.InfoChar.Power < 160000000)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DISCIPLE_NOT_ENOUGH_POWER_TO_OPEN_EGG));
                                return;
                            }

                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextQuaTrung[1], MenuNpc.Gi().MenuQuaTrung[2], character.InfoChar.Gender));
                            character.TypeMenu = 3;
                            break;
                        }
                    }
                    break;
                }
                case 2:
                {
                    character.MineGold(DataCache.GIA_NO_TRUNG_MA_BU);
                    CreateDiscipleMabu(character, (sbyte)select);
                    break;
                }
                case 3:
                {
                    CreateDiscipleMabu(character, (sbyte)select);
                    break;
                }
            }
        }
        
        private static void ConfirmBill(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                case 0:
                {
                    if (select != 0) return;
                    var fullThucAn = character.ItemBag.FirstOrDefault(item => DataCache.ListThucAn.Contains(item.Id) && item.Quantity >= 99);

                    if (character.InfoSet.IsFullSetThanLinh && fullThucAn != null)
                    {
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextBill[2], MenuNpc.Gi().MenuBill[2], character.InfoChar.Gender));
                        character.TypeMenu = 2;
                    }
                    else 
                    {   
                        character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId, MenuNpc.Gi().TextBill[1], MenuNpc.Gi().MenuBill[1], character.InfoChar.Gender));
                        character.TypeMenu = 1;
                    }
                    break;
                }   
                case 2:
                {
                    if (select != 0) return;
                    character.CharacterHandler.SendMessage(Service.Shop(character, 3, 21));
                    character.ShopId = 21;
                    character.TypeMenu = 3;
                    break;
                }
            }
        }
        private static void ConfirmNoiBanh(Character character, short npcId, int select)
        {
            switch (character.TypeMenu)
            {
                case 0:
                {
                    switch (select)
                    {
                        case 0:
                        { // nấu bằng ngọc type menu 1
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId,
                                MenuNpc.Gi().TextNoiBanh[1], MenuNpc.Gi().MenuNoiBanh[1], character.InfoChar.Gender));
                            character.TypeMenu = 1;
                            break;
                        }
                        case 1:
                        { // nấu bằng vàng type menu 2
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(npcId,
                                MenuNpc.Gi().TextNoiBanh[2], MenuNpc.Gi().MenuNoiBanh[1], character.InfoChar.Gender));
                            character.TypeMenu = 2;
                            break;
                        }
                    }

                    break;
                    }   
                case 1: // xử lý client đã chọn nấu bằng ngọc
                {
                    switch (select)
                    {
                        case 0:
                        { // kiểm tra trước tránh null chết sv
                            if (character.CharacterHandler.GetItemBagById(886) == null || character.CharacterHandler.GetItemBagById(886) == null || character.CharacterHandler.GetItemBagById(887) == null || character.CharacterHandler.GetItemBagById(888) == null || character.CharacterHandler.GetItemBagById(889) == null)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage("Bạn không có đủ nguyên liệu"));
                                break;
                            }

                            var randtile = ServerUtils.RandomNumber(1, 10);
                            if (character.CharacterHandler.GetItemBagById(886).Quantity < 10 ||
                                character.CharacterHandler.GetItemBagById(887).Quantity < 10 ||
                                character.CharacterHandler.GetItemBagById(888).Quantity < 10 ||
                                character.CharacterHandler.GetItemBagById(889).Quantity < 10)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage("Bạn không có đủ nguyên liệu"));
                            } else if (character.InfoChar.Diamond < 100)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage("Bạn không có đủ ngọc"));
                            } else if (character.LengthBagNull() < 1)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage("Hành trang cần ít nhất 1 ô trống"));
                            } else if (randtile < 5)
                            {
                                character.CharacterHandler.SendMessage(Service.NpcChat(npcId,
                                    "Ohhh nooo, bánh đã bị hỏng rồi... chúc bạn may mắn lần sau ^^"));
                                character.CharacterHandler.RemoveItemBagById(886, 10, reason:"Nấu bánh");
                                character.CharacterHandler.RemoveItemBagById(887, 10, reason:"Nấu bánh");
                                character.CharacterHandler.RemoveItemBagById(888, 10, reason:"Nấu bánh");
                                character.CharacterHandler.RemoveItemBagById(889, 10, reason:"Nấu bánh");
                                character.MineDiamond(100);
                                character.CharacterHandler.SendMessage(Service.SendBag(character));
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(Service.NpcChat(npcId,
                                    "Chúc mừng bạn đã làm bánh thành công"));
                                character.CharacterHandler.RemoveItemBagById(886, 10, reason:"Nấu bánh");
                                character.CharacterHandler.RemoveItemBagById(887, 10, reason:"Nấu bánh");
                                character.CharacterHandler.RemoveItemBagById(888, 10, reason:"Nấu bánh");
                                character.CharacterHandler.RemoveItemBagById(889, 10, reason:"Nấu bánh");
                                character.MineDiamond(100);
                                var itemAdd = ItemCache.GetItemDefault(891);
                                character.CharacterHandler.AddItemToBag(true, itemAdd, "Làm bánh");
                                character.CharacterHandler.SendMessage(Service.SendBag(character));
                                character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                            }
                            break;
                        }
                        case 1:
                        { 
                            //hủy
                            break;
                        }
                    }

                    break;
                }
                case 2: //xử lý client đã chọn nấu bằng vàng
                {
                    switch (select)
                    {
                        case 0:
                        { // kiểm tra trước tránh null chết sv
                            if (character.CharacterHandler.GetItemBagById(886) == null || character.CharacterHandler.GetItemBagById(886) == null || character.CharacterHandler.GetItemBagById(887) == null || character.CharacterHandler.GetItemBagById(888) == null || character.CharacterHandler.GetItemBagById(889) == null)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage("Bạn không có đủ nguyên liệu"));
                                break;
                            }
                            
                            if (character.CharacterHandler.GetItemBagById(886).Quantity < 10 ||
                                character.CharacterHandler.GetItemBagById(887).Quantity < 10 ||
                                character.CharacterHandler.GetItemBagById(888).Quantity < 10 ||
                                character.CharacterHandler.GetItemBagById(889).Quantity < 10)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage("Bạn không có đủ nguyên liệu"));
                            } else if (character.InfoChar.Gold < 25000000)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage("Bạn không có đủ vàng"));
                            } else if (character.LengthBagNull() < 1)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage("Hành trang cần ít nhất 1 ô trống"));
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(Service.NpcChat(npcId,
                                    "Chúc mừng bạn đã làm bánh thành công"));
                                character.CharacterHandler.RemoveItemBagById(886, 10, reason:"Nấu bánh");
                                character.CharacterHandler.RemoveItemBagById(887, 10, reason:"Nấu bánh");
                                character.CharacterHandler.RemoveItemBagById(888, 10, reason:"Nấu bánh");
                                character.CharacterHandler.RemoveItemBagById(889, 10, reason:"Nấu bánh");
                                character.MineGold(2500000);
                                var itemAdd = ItemCache.GetItemDefault(891);
                                character.CharacterHandler.AddItemToBag(true, itemAdd, "Làm bánh");
                                character.CharacterHandler.SendMessage(Service.SendBag(character));
                                character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                            }
                            break;
                        }
                        case 1:
                        { 
                            //hủy
                            break;
                        }
                    }

                    break;
                }
                
            }
        }
        #endregion
        #region Menu NOT COFIRM

        private static void MenuDauThan(Character character, int npcId, int menuId, int optionId)
        {
            var magicTree = MagicTreeManager.Get(character.Id);
            if(magicTree == null) return;
            lock (magicTree)
            {
                switch (menuId)
                {
                    //Thu hoạch // Dùng ngọc nâng cấp
                    case 0:
                    {
                        if (magicTree.IsUpdate)
                        {
                            var ngoc = magicTree.Diamond;
                            if (character.AllDiamond() < ngoc)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                                return;
                            }
                            character.MineDiamond(ngoc);
                            
                            magicTree.IsUpdate = false;
                            magicTree.Level++;
                            switch (magicTree.Level)
                            {
                                case < 8:
                                    magicTree.NpcId++;
                                    break;
                                case >= 10:
                                    magicTree.Level = 10;
                                    break;
                            }

                            magicTree.MaxPea += 2; 
                            magicTree.Peas = magicTree.MaxPea;
                            magicTree.Seconds = 0;
                            magicTree.Diamond = 0;
                            // MagicTreeDB.Update(magicTree);

                            magicTree.MagicTreeHandler.HandleNgoc();
                            character.CharacterHandler.SendMessage(Service.MagicTree0(magicTree));
                            character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                            MagicTreeDB.Update(magicTree);
                        }
                        else
                        {
                            if(magicTree.Peas == 0) return;
                            var quantityPea = magicTree.Peas;
                            var emptyBag = 10 - character.GetTotalDauThanBag();
                            var emptyBox = 20 - character.GetTotalDauThanBox();
                            var totalEmpty = emptyBag + emptyBox;
                            if (emptyBag <= 0 && emptyBox <= 0)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().MAX_PEAS));
                                return;
                            }
                            if(quantityPea > 0 && emptyBag > 0) {
                                if(quantityPea < emptyBag) {
                                    emptyBag = quantityPea;
                                    quantityPea = 0;
                                } else {
                                    quantityPea -= emptyBag;
                                }
                                var item = ItemCache.GetItemDefault((short)DataCache.IdDauThan[magicTree.Level - 1], emptyBag);
                                if(character.CharacterHandler.AddItemToBag(true, item, "Thu hoạch đậu")) {
                                    character.CharacterHandler.SendMessage(Service.SendBag(character));
                                }
                            }
                            if(quantityPea > 0 && emptyBox > 0) {
                                if(quantityPea < emptyBox) {
                                    emptyBox = quantityPea;
                                    quantityPea = 0;
                                } else {
                                    quantityPea -= emptyBox;
                                }
                                var item = ItemCache.GetItemDefault((short)DataCache.IdDauThan[magicTree.Level - 1], emptyBox);
                                if(character.CharacterHandler.AddItemToBox(true, item)) {
                                    character.CharacterHandler.SendMessage(Service.SendBox(character));
                                }
                            }

                            if (totalEmpty > 0)
                            {
                                character.CharacterHandler.SendMessage(Service.MagicTree0(magicTree));
                            }
                            magicTree.Peas = quantityPea;
                            magicTree.Seconds = 60000 * magicTree.Level + ServerUtils.CurrentTimeMillis();
                            magicTree.IsUpdate = false;
                            magicTree.MagicTreeHandler.HandleNgoc();
                            character.CharacterHandler.SendMessage(Service.MagicTree2(quantityPea, magicTree.Level));
                        }
                        break;
                    }
                    //Nâng cấp
                    case 1:
                    {
                        if (magicTree.Level == 10)
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiSay(5, "Đậu thần đã đạt đến cấp độ tối đa", false, character.InfoChar.Gender));
                            return;
                        }
                        if (magicTree.IsUpdate)
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5, MenuNpc.Gi().TextMeo[1], MenuNpc.Gi().MenuMeo[0], character.InfoChar.Gender));
                            character.TypeMenu = 2;
                        }
                        else
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5, MenuNpc.Gi().TextMeo[0], MenuNpc.Gi().MenuMeo[0], character.InfoChar.Gender));
                            character.TypeMenu = 1;
                        }
                        break;
                    }
                    //Kết hạt nhanh
                    case 2:
                    {
                        if(magicTree.IsUpdate || magicTree.Peas == magicTree.MaxPea) return;
                        var ngoc = magicTree.Diamond;
                        if (character.AllDiamond() < ngoc)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                            return;
                        }
                        character.MineDiamond(ngoc);
                        magicTree.Peas = magicTree.MaxPea;
                        magicTree.Seconds = 0;
                        magicTree.IsUpdate = false;
                        magicTree.MagicTreeHandler.HandleNgoc();
                        character.CharacterHandler.SendMessage(Service.MagicTree0(magicTree));
                        character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                        break;
                    }
                }
            }
            
        }
        

        #endregion
    
        #region Function
        private static void CreateDiscipleMabu(Character character, sbyte gender)
        {
            // Nếu có đệ thì đổi đệ
            var oldDisciple = character.Disciple;
            if (oldDisciple != null || DiscipleDB.IsAlreadyExist(-character.Id))
            {
                oldDisciple = new Disciple();
                oldDisciple.CreateNewMaBuDisciple(character, gender);
                oldDisciple.Player = character.Player;
                oldDisciple.CharacterHandler.SetUpInfo();
                character.Disciple = oldDisciple;
                DiscipleDB.Update(oldDisciple);
            }
            // không có thì tạo mới
            else
            {
                var disciple = new Disciple();
                disciple.CreateNewMaBuDisciple(character, gender);
                disciple.Player = character.Player;
                disciple.CharacterHandler.SetUpInfo();
                character.Disciple = disciple;
                character.InfoChar.IsHavePet = true;
                character.CharacterHandler.SendMessage(Service.Disciple(1, null));
                DiscipleDB.Create(disciple);
            }
            
            // var oldDisciple = character.Disciple;
            // if (oldDisciple != null)
            // {
            //     DiscipleDB.Delete(oldDisciple.Id);
            //     character.CharacterHandler.SendMessage(Service.Disciple(0, null)); 
            //     character.InfoChar.IsHavePet = false;
            //     character.Disciple = null;
            // }
            character.CharacterHandler.SendMessage(Service.NoTrungMaBu());
            character.InfoChar.ThoiGianTrungMaBu = 0;

            // Thread.Sleep(3000);
            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().GET_NEW_MABU_DISCIPLE));
            
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Handlers.Character;
using NRO_Server.Application.Handlers.Client;
using NRO_Server.Application.Handlers.Item;
using NRO_Server.Application.Handlers.Skill;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Client;
using NRO_Server.Application.Interfaces.Map;
using NRO_Server.Application.IO;
using NRO_Server.Application.Manager;
using NRO_Server.Application.Map;
using NRO_Server.Application.Threading;
using NRO_Server.DatabaseManager;
using NRO_Server.DatabaseManager.Player;
using NRO_Server.Main.Menu;
using NRO_Server.Model;
using NRO_Server.Model.Character;
using NRO_Server.Model.Clan;
using NRO_Server.Model.Info;
using NRO_Server.Model.Map;
using NRO_Server.Model.SkillCharacter;
using NRO_Server.Model.Monster;
using NRO_Server.Model.Item;
using NRO_Server.Model.Template;
using NRO_Server.Model.Option;

namespace NRO_Server.Application.Main
{
    public class Controller : IMessageHandler
    {
        private readonly ISession_ME _session;

        public Controller(ISession_ME client)
        {
            _session = client;
        }

        public void OnConnectionFail(ISession_ME client, bool isMain)
        {
            //throw new System.NotImplementedException();
        }

        public void OnConnectOK(ISession_ME client, bool isMain)
        {
            //throw new System.NotImplementedException();
        }

        public void OnDisconnected(ISession_ME client, bool isMain)
        {
            //throw new System.NotImplementedException();
        }

        public async Task OnMessage(Message message)
        {
            try
            {
                if (message == null) return;
                var command = message.Command;
                Server.Gi().Logger.Debug($"Client: {_session.Id} - Command >>>>> {command}");
                Server.Gi().Logger.Debug($"Zom Level: {_session.ZoomLevel}");

                var characterLog = _session?.Player?.Character;
                if (characterLog != null)
                {

                    if (DataCache.LogTheoDoi.Contains(characterLog.Id))
                    {
                        ServerUtils.WriteTraceLog(characterLog.Id + "_" + characterLog.Name, "Command: " + command);
                    }
                }
                switch (command)
                {
                    // Transport
                    case -105:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null) return;
                            try
                            {
                                var mapId = character.InfoMore.TransportMapId;

                                if (mapId == -1)
                                {
                                    JoinHome(true, true, character.InfoChar.Teleport);
                                    return;
                                }

                                var @char = character;
                                var mapOld = MapManager.Get(@char.InfoChar.MapId);
                                if (DataCache.IdMapCustom.Contains(@char.InfoChar.MapId))
                                {
                                    mapOld = MapManager.GetMapCustom(@char.InfoChar.MapCustomId)
                                        .GetMapById(@char.InfoChar.MapId);
                                }

                                Threading.Map mapNext;
                                if (DataCache.IdMapCustom.Contains(mapId))
                                {
                                    _session.SendMessage(
                                        Service.SendTeleport(character.Id, character.InfoChar.Teleport));
                                    mapOld.OutZone(character, mapId);
                                    @char.MapIdOld = mapOld.Id;
                                    @char.SetOldMap();
                                    switch (mapId)
                                    {
                                        case 21:
                                        case 22:
                                        case 23:
                                            {
                                                JoinHome(true, true, character.InfoChar.Teleport);
                                                return;
                                            }
                                        case 47:
                                            {
                                                JoinKarin(47, true, true, character.InfoChar.Teleport);
                                                return;
                                            }
                                        case 45:
                                            {
                                                JoinKarin(45, true, true, character.InfoChar.Teleport);
                                                return;
                                            }
                                        case 48:
                                            {
                                                JoinKarin(48, true, true, character.InfoChar.Teleport);
                                                return;
                                            }
                                        case 111:
                                            {
                                                JoinKarin(111, true, true, character.InfoChar.Teleport);
                                                return;
                                            }
                                    }
                                }
                                else
                                {
                                    mapNext = MapManager.Get(mapId);
                                    var zoneNext = mapNext.GetZoneNotMaxPlayer();
                                    if (zoneNext == null)
                                    {
                                        _session.SendMessage(Service.OpenUiSay(5, TextServer.gI().MAX_NUMCHARS,
                                            false,
                                            character.InfoChar.Gender));
                                        JoinHome(true, true, character.InfoChar.Teleport);
                                    }
                                    else
                                    {
                                        _session.SendMessage(Service.SendTeleport(character.Id,
                                            character.InfoChar.Teleport));
                                        mapOld.OutZone(character, mapNext.Id);
                                        @char.MapIdOld = mapOld.Id;
                                        @char.SetOldMap();
                                        zoneNext.ZoneHandler.JoinZone((Character)character, true, true,
                                            character.InfoChar.Teleport);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            break;
                        }
                    // Special Skill
                    case 112:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null) return;
                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5, MenuNpc.Gi().TextNoiTai[0],
                                        MenuNpc.Gi().MenuNoiTai[0], character.InfoChar.Gender));
                            character.TypeMenu = 10;
                            break;
                        }
                    //Luck roll
                    case -127:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null) return;
                            var type = message.Reader.ReadByte();
                            var soluong = 0;
                            if (message.Reader.Available() > 0)
                            {
                                soluong = message.Reader.ReadByte();
                            }

                            Server.Gi().Logger
                                .Debug(
                                    $"Client: {_session.Id} - Luck roll -------------------- type: {type} - soluong: {soluong}");
                            switch (type)
                            {
                                case 0:
                                    {
                                        if (soluong == 0)
                                        {
                                            if (character.LuckyBox.Count >= DataCache.LIMIT_SLOT_RUONG_PHU_THUONG_DE)
                                            {
                                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().FULL_LUCKY_BOX));
                                                break;
                                            }
                                            character.CharacterHandler.SendMessage(Service.LuckRoll0());
                                            character.ShopId = 0;
                                        }
                                        break;
                                    }
                                case 1:
                                    {
                                        if (soluong == 0)
                                        {
                                            character.CharacterHandler.SendMessage(Service.LuckRoll0());
                                            break;
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        var charReal = (Character)character;

                                        // Kiểm tra số lượng vé quay trước
                                        var itemVeQuay = character.ItemBag.FirstOrDefault(i => i.Id is 821);
                                        var soLuongVeQuay = itemVeQuay != null ? itemVeQuay.Quantity : 0;
                                        var soLuongNgocRong = soluong;
                                        // Nếu số lượng vé dùng cao hơn số lượng vé trong túi 
                                        if (soLuongNgocRong > soLuongVeQuay)
                                        {
                                            // Thì số lượng vé sử dụng bằng tổng số lượng vé quay
                                            // soLuongNgocRong ngọc quay trừ đi số lượng vé quay
                                            soLuongNgocRong -= soLuongVeQuay;
                                        }
                                        // nếu số lượng vé cần dùng ít hơn hoặc bằng số lượng vé trong túi
                                        else
                                        {
                                            soLuongVeQuay = soLuongNgocRong;
                                            soLuongNgocRong = 0;
                                        }

                                        if (soLuongNgocRong * DataCache.CRACK_BALL_PRICE > charReal.InfoChar.Gold)
                                        {
                                            return;
                                        }

                                        if (itemVeQuay != null && soLuongVeQuay > 0)
                                        {
                                            charReal.CharacterHandler.RemoveItemBagByIndex(itemVeQuay.IndexUI, soLuongVeQuay, reason: "qvmm");
                                            charReal.CharacterHandler.SendMessage(Service.SendBag(character));
                                        }

                                        charReal.MineGold(soLuongNgocRong * DataCache.CRACK_BALL_PRICE);
                                        charReal.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                                        var list = new List<short>();
                                        var timeServer = ServerUtils.CurrentTimeSecond();
                                        for (var i = 0; i < soluong; i++)
                                        {
                                            var randomRate = ServerUtils.RandomNumber(0.0, 100.0);
                                            var listRandomItem = new List<short>();
                                            int expireDay = 0;
                                            int expireTime = 0;

                                            if (randomRate <= 0.1) //vinh vien hiem
                                            {
                                                listRandomItem = DataCache.LuckBoxRare;
                                            }
                                            else if (randomRate <= 1) //hiem co thoi han
                                            {
                                                listRandomItem = DataCache.LuckBoxRare;
                                                //thời hạn 15-30 ngày
                                                expireDay = ServerUtils.RandomNumber(15, 30);

                                            }
                                            else if (randomRate <= 4)
                                            {
                                                listRandomItem = DataCache.LuckBoxRare;
                                                //thời hạn 5 ngày - 10 ngày
                                                expireDay = ServerUtils.RandomNumber(5, 10);
                                            }
                                            else
                                            {
                                                randomRate = ServerUtils.RandomNumber(1, 100);
                                                if (randomRate <= 40)
                                                {
                                                    listRandomItem = DataCache.LuckBoxEpic;
                                                }
                                                else
                                                {
                                                    listRandomItem = DataCache.LuckBoxCommon;
                                                }
                                            }

                                            if (expireDay > 0)
                                            {
                                                expireTime = timeServer + (expireDay * 86400);
                                            }

                                            int itemRandomIndex = ServerUtils.RandomNumber(listRandomItem.Count);
                                            using (var itemNew = ItemCache.GetItemDefault(listRandomItem[itemRandomIndex]))
                                            {
                                                var template = ItemCache.ItemTemplate(itemNew.Id);
                                                var indexList = charReal.LuckyBox.Count;
                                                itemNew.Reason = "LUCKY ITEM";
                                                itemNew.IndexUI = indexList;

                                                if (expireDay > 0)
                                                {
                                                    var optionHiden = itemNew.Options.FirstOrDefault(option => option.Id == 73);
                                                    itemNew.Options.Add(new OptionItem()
                                                    {
                                                        Id = 93,
                                                        Param = expireDay,
                                                    });

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

                                                if (template.Type == 9) // vàng
                                                {
                                                    itemNew.Options.Add(new OptionItem()
                                                    {
                                                        Id = 171,
                                                        Param = 20,
                                                    });
                                                }

                                                list.Add(template.IconId);
                                                charReal.LuckyBox.Add(itemNew);
                                            }
                                            character.Delay.NeedToSaveLucky = true;
                                            character.Delay.InvAction = timeServer + 1000;
                                            if ((character.InfoChar.ThoiGianDoiMayChu - timeServer) < 180000)
                                            {
                                                character.InfoChar.ThoiGianDoiMayChu = timeServer + 300000;
                                            }
                                            // character.Delay.SaveData += 1000;
                                            // var itemNew =
                                            //     ItemCache.GetItemDefault(
                                            //         (short) ServerUtils.RandomNumber(Cache.Gi().ITEM_TEMPLATES.Count));
                                            // var template = ItemCache.ItemTemplate(itemNew.Id);
                                            // var indexList = charReal.LuckyBox.Count;
                                            // itemNew.Reason = "LUCKY ITEM";
                                            // itemNew.IndexUI = indexList;
                                            // list.Add(template.IconId);
                                            // charReal.LuckyBox.Add(itemNew);
                                        }

                                        character.CharacterHandler.SendMessage(Service.LuckRoll1(list));
                                        break;
                                    }
                            }

                            break;
                        }
                    //Input Client
                    case -125:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            InputClient.HanleInputClient((Character)character, message);
                            break;
                        }
                    case -111:
                        {
                            break;
                        }
                    //Change on skill
                    case -113:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var count = 0;
                            while (message.Reader.Available() > 0)
                            {
                                try
                                {
                                    character.InfoChar.OSkill[count++] = message.Reader.ReadByte();
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }

                            //update skill khi set onskill
                            character.CharacterHandler.SendMessage(Service.ChangeOnSkill(character.InfoChar.OSkill));
                            break;
                        }
                    //Status Đệ Tử
                    case -108:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character?.Disciple == null || !character.InfoChar.IsHavePet) return;

                            if (character.IsDontMove())
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().DO_NOT_ACTION_DISCIPLE));
                                return;
                            }

                            var status = message.Reader.ReadByte();
                            var disciple = character.Disciple;
                            lock (disciple)
                            {
                                Server.Gi().Logger
                                    .Debug(
                                        $"Client: {_session.Id} ----------------------- status pet: {character.Disciple.Status}");
                                switch (status)
                                {
                                    case 0:
                                        {
                                            if (character.InfoChar.Fusion.IsFusion || disciple.Status >= 4)
                                            {
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().DO_NOT_ACTION_DISCIPLE));
                                                return;
                                            }

                                            if (disciple.Status == 3)
                                            {
                                                async void Action()
                                                {
                                                    await Task.Delay(2000);
                                                    character.Zone.ZoneHandler.AddDisciple(disciple);
                                                    character.CharacterHandler.SendMessage(Service.PublicChat(disciple.Id,
                                                    "Bái kiến sư phụ"));
                                                }

                                                var task = new Task(Action);
                                                task.Start();
                                            }
                                            else
                                            {
                                                character.CharacterHandler.SendMessage(Service.PublicChat(disciple.Id,
                                                    "Ok, con đi theo sư phụ"));

                                            }
                                            disciple.Status = 0;
                                            break;
                                        }
                                    case 1:
                                        {
                                            if (character.InfoChar.Fusion.IsFusion || disciple.Status >= 4)
                                            {
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().DO_NOT_ACTION_DISCIPLE));
                                                return;
                                            }


                                            if (disciple.Status == 3)
                                            {
                                                character.Zone.ZoneHandler.AddDisciple(disciple);
                                            }


                                            character.CharacterHandler.SendMessage(Service.PublicChat(disciple.Id,
                                        "Ok, con sẽ bảo vệ sư phụ"));
                                            disciple.Status = 1;
                                            break;
                                        }
                                    case 2:
                                        {
                                            if (character.InfoChar.Fusion.IsFusion || disciple.Status >= 4)
                                            {
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().DO_NOT_ACTION_DISCIPLE));
                                                return;
                                            }

                                            if (disciple.Status == 3)
                                            {
                                                character.Zone.ZoneHandler.AddDisciple(disciple);
                                            }

                                            character.CharacterHandler.SendMessage(Service.PublicChat(disciple.Id,
                                                "Ok, sư phụ cứ để con lo cho"));
                                            disciple.Status = 2;
                                            break;
                                        }
                                    case 3:
                                        {
                                            if (character.InfoChar.Fusion.IsFusion || disciple.Status == 4)
                                            {
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().DO_NOT_ACTION_DISCIPLE));
                                                return;
                                            }

                                            character.CharacterHandler.SendMessage(Service.PublicChat(disciple.Id,
                                                "Bibi sư phụ..."));

                                            async void Action()
                                            {
                                                await Task.Delay(2000);
                                                try
                                                {
                                                    if (disciple.Zone != null && disciple.Zone.ZoneHandler != null)
                                                    {
                                                        disciple.Zone.ZoneHandler.RemoveDisciple(disciple);
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    Server.Gi().Logger.Error($"Error disciple.Zone.ZoneHandler.RemoveDisciple in Controller.cs: {e.Message} \n {e.StackTrace}", e);
                                                }
                                            }

                                            disciple.Status = 3;
                                            var task = new Task(Action);
                                            task.Start();
                                            break;
                                        }
                                    //Hợp thể 10 phút
                                    case 4:
                                        {
                                            if (character.Zone.Map.IsMapCustom())
                                            {
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().DONT_NOT_ACTION_DISCIPLE_HERE));
                                                return;
                                            }

                                            var timeServer = ServerUtils.CurrentTimeMillis();
                                            if (character.InfoChar.Fusion.IsFusion)
                                            {
                                                disciple.CharacterHandler.SetUpPosition(isRandom: true);
                                                character.Zone.ZoneHandler.AddDisciple(disciple);
                                                character.CharacterHandler.SendZoneMessage(Service.Fusion(character.Id, 1));
                                                lock (character.InfoChar.Fusion)
                                                {
                                                    character.InfoChar.Fusion.IsFusion = false;
                                                    character.InfoChar.Fusion.IsPorata = false;
                                                    character.InfoChar.Fusion.IsPorata2 = false;
                                                    character.InfoChar.Fusion.TimeStart = timeServer;
                                                    character.InfoChar.Fusion.DelayFusion = timeServer + 600000;
                                                    character.InfoChar.Fusion.TimeUse = 0;
                                                }

                                                disciple.Status = 0;
                                            }
                                            else
                                            {
                                                if (disciple.InfoChar.IsDie || disciple.Status >= 3)
                                                {
                                                    character.CharacterHandler.SendMessage(
                                                        Service.ServerMessage(TextServer.gI().CAN_NOT_USE_FUSION));
                                                    return;
                                                }

                                                if (character.InfoChar.Fusion.DelayFusion > timeServer)
                                                {
                                                    var delay = (character.InfoChar.Fusion.DelayFusion - timeServer) / 60000;
                                                    if (delay < 1)
                                                    {
                                                        delay = 1;
                                                    }

                                                    character.CharacterHandler.SendMessage(
                                                        Service.ServerMessage(string.Format(TextServer.gI().DELAY_FUSION_10M,
                                                            delay)));
                                                    return;
                                                }

                                                if (disciple.InfoSkill.HuytSao.IsHuytSao)
                                                {
                                                    SkillHandler.RemoveHuytSao(disciple);
                                                }

                                                if (disciple.InfoSkill.Monkey.MonkeyId == 1)
                                                {
                                                    SkillHandler.HandleMonkey(disciple, false);
                                                }

                                                disciple.Zone.ZoneHandler.RemoveDisciple(disciple);
                                                character.CharacterHandler.SendZoneMessage(Service.Fusion(character.Id, 4));
                                                lock (character.InfoChar.Fusion)
                                                {
                                                    character.InfoChar.Fusion.IsFusion = true;
                                                    character.InfoChar.Fusion.IsPorata = false;
                                                    character.InfoChar.Fusion.IsPorata2 = false;
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
                                    //Hợp thể vĩnh viễn
                                    case 5:
                                        {
                                            // if (character.InfoChar.Gender != 1) return;
                                            // if (character.Zone.Map.IsMapCustom())
                                            // {
                                            //     character.CharacterHandler.SendMessage(
                                            //         Service.ServerMessage(TextServer.gI().DONT_NOT_ACTION_DISCIPLE_HERE));
                                            //     return;
                                            // }

                                            // if (disciple.InfoChar.IsDie || disciple.Status >= 3)
                                            // {
                                            //     character.CharacterHandler.SendMessage(
                                            //         Service.ServerMessage(TextServer.gI().CAN_NOT_USE_FUSION));
                                            //     return;
                                            // }

                                            // if (DiscipleDB.Delete(disciple.Id))
                                            // {
                                            //     disciple.Status = 0;
                                            //     //Cleare disciple
                                            //     disciple.Zone.ZoneHandler.RemoveDisciple(disciple);
                                            //     disciple.CharacterHandler.Clear();

                                            //     character.CharacterHandler.SendZoneMessage(Service.Fusion(character.Id, 6));
                                            //     character.CharacterHandler.SendMessage(
                                            //         Service.ServerMessage(TextServer.gI().FUSION_88));

                                            //     character.InfoChar.Potential += disciple.InfoChar.Power;
                                            //     character.CharacterHandler.SendMessage(Service.MeLoadPoint(character));

                                            //     character.InfoChar.IsHavePet = false;
                                            //     character.Disciple = null;
                                            //     character.CharacterHandler.SendMessage(Service.Disciple(0, null));
                                            // }
                                            // else
                                            // {
                                            //     character.CharacterHandler.SendMessage(
                                            //         Service.ServerMessage(TextServer.gI().ERROR_SERVER));
                                            // }
                                            character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().ERROR_SERVER));

                                            break;
                                        }
                                }
                            }

                            break;
                        }
                    //Đệ tử
                    case -107:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null) return;
                            if (character.Disciple != null)
                            {
                                character.CharacterHandler.SendMessage(Service.Disciple(2, character.Disciple));
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(Service.OpenUiSay(5, TextServer.gI().NOT_FOUND_DISCIPLE, false, character.InfoChar.Gender));
                            }
                            break;
                        }
                    //Set LOCK
                    case -104:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null) return;
                            try
                            {
                                var pass = message.Reader.ReadInt();
                                if (pass.ToString().Length != 6) return;
                                if (character.InfoChar.LockInventory.Pass == -1)
                                {
                                    character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5,
                                        string.Format(MenuNpc.Gi().TextMeo[8], pass), MenuNpc.Gi().MenuMeo[1],
                                        character.InfoChar.Gender));
                                    character.TypeMenu = 8;
                                    character.InfoChar.LockInventory.PassTemp = pass;
                                }
                                else
                                {
                                    if (pass != character.InfoChar.LockInventory.Pass)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.ServerMessage(TextServer.gI().INVALID_LOCK_INVENTORY));
                                        return;
                                    }

                                    var text = MenuNpc.Gi().TextMeo[9];
                                    if (!character.InfoChar.LockInventory.IsLock)
                                    {
                                        text = MenuNpc.Gi().TextMeo[10];
                                    }

                                    character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5, text,
                                        MenuNpc.Gi().MenuMeo[1], character.InfoChar.Gender));
                                    character.TypeMenu = 9;
                                }
                            }
                            catch (Exception)
                            {
                                return;
                            }

                            break;
                        }
                    //CHANGE_FLAG
                    case -103:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null) return;
                            {
                                if (DataCache.IdMapCustom.Contains(character.InfoChar.MapId))
                                {
                                    character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(TextServer.gI().NOT_CHANGE_FLAG));
                                    return;
                                }

                                var action = message.Reader.ReadByte();
                                switch (action)
                                {
                                    case 0:
                                        {
                                            character.CharacterHandler.SendMessage(Service.ChangeFlag0());
                                            break;
                                        }
                                    case 1:
                                        {
                                            var type = message.Reader.ReadByte();
                                            var @char = (Character)character;
                                            var delayChange = @char.Delay.ChangeFlag;
                                            var timeServe = ServerUtils.CurrentTimeMillis();
                                            if (type != 0 && delayChange > timeServe)
                                            {
                                                var timeDelay = (delayChange - timeServe) / 1000;
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(string.Format(TextServer.gI().DELAY_CHANGEFLAG,
                                                        timeDelay)));
                                                return;
                                            }

                                            if (type is 9 or 10)
                                            {
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().NOT_FLAG));
                                                return;
                                            }

                                            character.Flag = type;
                                            character.CharacterHandler.SendZoneMessage(Service.ChangeFlag1(character.Id, type));

                                            if (character.Disciple != null && character.InfoChar.IsHavePet)
                                            {
                                                character.Disciple.Flag = type;
                                                if (!character.InfoChar.Fusion.IsFusion)
                                                {
                                                    character.CharacterHandler.SendZoneMessage(
                                                        Service.ChangeFlag1(character.Disciple.Id, type));
                                                }
                                            }

                                            if (type != 0)
                                            {
                                                @char.Delay.ChangeFlag = 60000 + timeServe;
                                            }

                                            break;
                                        }
                                    case 2:
                                        {
                                            var type = message.Reader.ReadByte();
                                            character.CharacterHandler.SendMessage(Service.ChangeFlag2(type));
                                            break;
                                        }
                                }
                            }
                            break;
                        }
                    //Guest Player
                    case -101:
                        {
                            _session.SendMessage(Service.DialogMessage(TextServer.gI().LOCK_LOGIN));
                            break;
                        }
                    //List enemy 
                    case -99:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var @charReal = (Character)character;
                            if (@charReal.Enemies == null)
                            {
                                @charReal.Enemies = new List<InfoFriend>();
                            }

                            var action = message.Reader.ReadByte();
                            switch (action)
                            {
                                //Danh sách
                                case 0:
                                    {
                                        character.CharacterHandler.SendMessage(Service.ListEmeny(@charReal.Enemies));
                                        break;
                                    }
                                //Chấp nhập Kết bạn YES/NO
                                case 1:
                                    {
                                        var charId = message.Reader.ReadInt();
                                        if (charId == character.Id) return;
                                        var @char = (Character)ClientManager.Gi().GetCharacter(charId);
                                        if (@char != null)
                                        {
                                            var enemy = @charReal.Enemies.FirstOrDefault(enemy => enemy.Id == charId);
                                            if (enemy == null)
                                            {
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                                                return;
                                            }

                                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5,
                                                string.Format(MenuNpc.Gi().TextMeo[4], @char.Name), MenuNpc.Gi().MenuMeo[1],
                                                character.InfoChar.Gender));
                                            @charReal.TypeMenu = 5;
                                            @charReal.EnemyTemp = enemy;
                                        }
                                        else
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().USER_OFFLINE));
                                        }

                                        break;
                                    }
                                //Xoá thù địch
                                case 2:
                                    {
                                        var charId = message.Reader.ReadInt();
                                        var info = @charReal.Enemies.FirstOrDefault(enemy => enemy.Id == charId);
                                        if (info != null)
                                        {
                                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5,
                                                string.Format(MenuNpc.Gi().TextMeo[7], info.Name), MenuNpc.Gi().MenuMeo[1],
                                                character.InfoChar.Gender));
                                            @charReal.TypeMenu = 7;
                                            @charReal.EnemyTemp = info;
                                        }
                                        else
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().FRIEND_NOT_FOUND));
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    //MAP_TRANSPOT
                    case -91:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            {
                                var itemtranspot193 = character.ItemBag.FirstOrDefault(i => i.Id is 193);
                                var itemtranspot194 = character.ItemBag.FirstOrDefault(i => i.Id is 194);
                                if (itemtranspot193 == null && itemtranspot194 == null) return;
                                try
                                {
                                    var @char = (Character)character;
                                    var mapTranspot = @char.MapTranspots[message.Reader.ReadByte()];
                                    if (mapTranspot == null) return;
                                    var mapOld = MapManager.Get(@char.InfoChar.MapId);
                                    if (DataCache.IdMapCustom.Contains(@char.InfoChar.MapId))
                                    {
                                        mapOld = MapManager.GetMapCustom(@char.InfoChar.MapCustomId)
                                            .GetMapById(@char.InfoChar.MapId);
                                    }

                                    Threading.Map mapNext;
                                    if (DataCache.IdMapCustom.Contains(mapTranspot.Id))
                                    {
                                        character.CharacterHandler.SendZoneMessage(
                                            Service.SendTeleport(character.Id, character.InfoChar.Teleport));
                                        mapOld.OutZone(character, mapTranspot.Id);
                                        @char.MapIdOld = mapOld.Id;
                                        @char.SetOldMap();
                                        if (itemtranspot194 == null || @char.IsItem193)
                                        {
                                            character.CharacterHandler.RemoveItemBagById(193, 1, reason: "Dùng cs");
                                            _session.SendMessage(Service.SendBag(character));
                                        }

                                        switch (mapTranspot.Id)
                                        {
                                            case 21:
                                            case 22:
                                            case 23:
                                                {
                                                    JoinHome(true, true, character.InfoChar.Teleport);
                                                    return;
                                                }
                                            case 47:
                                                {
                                                    JoinKarin(47, true, true, character.InfoChar.Teleport);
                                                    return;
                                                }
                                            case 45:
                                                {
                                                    JoinKarin(45, true, true, character.InfoChar.Teleport);
                                                    return;
                                                }
                                            case 48:
                                                {
                                                    JoinKarin(48, true, true, character.InfoChar.Teleport);
                                                    return;
                                                }
                                            case 111:
                                                {
                                                    JoinKarin(111, true, true, character.InfoChar.Teleport);
                                                    return;
                                                }
                                        }
                                    }
                                    else
                                    {
                                        mapNext = MapManager.Get(mapTranspot.Id);
                                        if (mapNext != null && DataCache.IdMapCold.Contains(mapNext.Id) && character.InfoChar.Power < 40000000000)
                                        {
                                            character.CharacterHandler.SendMessage(Service.OpenUiSay(5, "Bạn cần đạt 40 tỷ sức mạnh mới có thể qua hành tinh Cold", false, character.InfoChar.Gender));
                                            return;
                                        }
                                        var zoneNext = mapNext.GetZoneNotMaxPlayer();
                                        if (zoneNext == null)
                                        {
                                            _session.SendMessage(Service.OpenUiSay(5, TextServer.gI().MAX_NUMCHARS, false,
                                                character.InfoChar.Gender));
                                        }
                                        else
                                        {
                                            character.CharacterHandler.SendZoneMessage(
                                                Service.SendTeleport(character.Id, character.InfoChar.Teleport));
                                            mapOld.OutZone(character, mapNext.Id);
                                            @char.MapIdOld = mapOld.Id;
                                            @char.SetOldMap();
                                            zoneNext.ZoneHandler.JoinZone((Character)character, true, true,
                                                character.InfoChar.Teleport);
                                            if (itemtranspot194 == null || @char.IsItem193)
                                            {
                                                character.CharacterHandler.RemoveItemBagById(193, 1, reason: "Dùng cs");
                                                _session.SendMessage(Service.SendBag(character));
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    return;
                                }
                            }
                            break;
                        }
                    //GIAO DỊCH
                    case -86:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null || character.InfoChar.IsDie || character.InfoChar.TypePk != 0)
                                return;
                            if (!character.CheckLockInventory()) return;

                            var timeServer = ServerUtils.CurrentTimeMillis();

                            if ((Server.Gi().StartServerTime + 300000) > timeServer)
                            {
                                var delay = ((Server.Gi().StartServerTime + 300000) - timeServer) / 1000;
                                if (delay < 1)
                                {
                                    delay = 1;
                                }

                                character.CharacterHandler.SendMessage(Service.DialogMessage(string.Format(TextServer.gI().DELAY_RESTART_SEC,
                                        delay)));
                                return;
                            }

                            if (Maintenance.Gi().IsStart)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Máy chủ đang tiến hành bảo trì, không thể giao dịch ngay lúc này"));
                                return;
                            }

                            if (character.Delay.InvAction > timeServer)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn thao tác quá nhanh, chậm lại nhé"));
                                return;
                            }

                            if (!character.InfoChar.IsPremium)
                            {
                                character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(TextServer.gI().NOT_PREMIUM));
                                return;
                                // if (character.InfoChar.Power < 20000000000)
                                // {
                                //     character.CharacterHandler.SendMessage(
                                //             Service.ServerMessage(TextServer.gI().LIMIT_TRADE_POWER));
                                //     return;
                                // }

                                // if (character.InfoChar.SoLanGiaoDich > DataCache.LIMIT_NOT_PREMIUM_TRADE_DAY)
                                // {
                                //     character.CharacterHandler.SendMessage(
                                //         Service.ServerMessage(TextServer.gI().NOT_PREMIUM));
                                //     return;
                                // }

                                // if (character.InfoChar.ThoiGianGiaoDich > timeServer)
                                // {
                                //     var delay = (character.InfoChar.ThoiGianGiaoDich - timeServer) / 1000;
                                //     if (delay < 1)
                                //     {
                                //         delay = 1;
                                //     }

                                //     _session.SendMessage(Service.DialogMessage(string.Format(TextServer.gI().DELAY_SEC,
                                //             delay)));
                                //     return;
                                // }
                            }

                            ItemHandler.TradeItem(message, (Character)character);
                            break;
                        }
                    //COMBINNE
                    case -81:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            await Combinne(message, (Character)character);
                            break;
                        }
                    //ADD FRIEND
                    case -80:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var action = message.Reader.ReadByte();
                            var @charReal = (Character)character;
                            switch (action)
                            {
                                //Danh sách
                                case 0:
                                    {
                                        character.CharacterHandler.SendMessage(Service.ListFriend0(@charReal.Friends));
                                        break;
                                    }
                                //Chấp nhập Kết bạn YES/NO
                                case 1:
                                    {
                                        var charId = message.Reader.ReadInt();
                                        if (charId == character.Id) return;
                                        var @char = (Character)ClientManager.Gi().GetCharacter(charId);
                                        if (@char != null)
                                        {
                                            if (@charReal.Friends.FirstOrDefault(friend => friend.Id == charId) != null)
                                            {
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().FRIEND_DUPLICATE));
                                                return;
                                            }

                                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5,
                                                string.Format(MenuNpc.Gi().TextMeo[2], @char.Name), MenuNpc.Gi().MenuMeo[1],
                                                character.InfoChar.Gender));
                                            @charReal.TypeMenu = 3;
                                            @charReal.FriendTemp = @char.Me;
                                        }
                                        else
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().USER_OFFLINE));
                                        }

                                        break;
                                    }
                                //Xoá bạn bè
                                case 2:
                                    {
                                        var charId = message.Reader.ReadInt();
                                        var info = @charReal.Friends.FirstOrDefault(friend => friend.Id == charId);
                                        if (info != null)
                                        {
                                            character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5,
                                                string.Format(MenuNpc.Gi().TextMeo[3], info.Name), MenuNpc.Gi().MenuMeo[1],
                                                character.InfoChar.Gender));
                                            @charReal.TypeMenu = 4;
                                            @charReal.FriendTemp = info;
                                        }
                                        else
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().FRIEND_NOT_FOUND));
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    //Get PLayer menu
                    case -79:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var charId = message.Reader.ReadInt();
                            var map = DataCache.IdMapCustom.Contains(character.InfoChar.MapId)
                                ? MapManager.GetMapCustom(character.InfoChar.MapCustomId)
                                    .GetMapById(character.InfoChar.MapId)
                                : MapManager.Get(character.InfoChar.MapId);

                            if (map != null)
                            {
                                var zone = map.GetZoneById(character.InfoChar.ZoneId);
                                if (zone != null)
                                {
                                    var @charCheck = zone.ZoneHandler.GetCharacter(charId);
                                    if (@charCheck != null)
                                    {
                                        var levels =
                                            Cache.Gi().LEVELS.Where(x => x.Gender == @charCheck.InfoChar.Gender)
                                                .Select(x => x.Name).ToList()[@charCheck.InfoChar.Level - 1];
                                        character.CharacterHandler.SendMessage(Service.MenuPlayer(charId,
                                            @charCheck.InfoChar.Power, levels));
                                    }
                                    else
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                                    }
                                }
                                else
                                {
                                    character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                                }
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                            }

                            break;
                        }
                    //Get Image Resource
                    case -74:
                        {
                            if (!DatabaseManager.Manager.gI().IsDownloadServer)
                            {
                                _session.SendMessage(Service.DialogMessage("Vui lòng tải đúng phiên bản mới trên web NROSUPERHERO.COM để vào game"));
                                return;
                            }

                            //Check DDOS
                            IpTime check = null;
                            var block = false;
                            var ipv4 = _session.IpV4;
                            if (FireWall.IpTimes.ContainsKey(ipv4))
                            {
                                check = FireWall.IpTimes[ipv4];
                                Server.Gi().Logger
                                    .Info($"Ip: {ipv4} ----------- Call -74. Checking..... Count: {check.Count}");
                            }
                            else
                            {
                                check = new IpTime()
                                {
                                    Ip = ipv4,
                                    Time = ServerUtils.CurrentTimeMillis(),
                                    Count = 1,
                                };
                                FireWall.IpTimes.TryAdd(ipv4, check);
                            }


                            var action = message.Reader.ReadByte();
                            Server.Gi().Logger
                                .Debug(
                                    $"Client: {_session.Id} - Ip: {_session.IpV4} ----------- Call -74 with action: {action}");
                            switch (action)
                            {
                                case 1:
                                    {
                                        _session.SendMessage(Service.SendImageResource1(_session.ZoomLevel));
                                        break;
                                    }
                                case 2:
                                    {
                                        block = true;
                                        await Service.SendImageResource2Async(_session);
                                        Service.SendImageResource3(_session);
                                        break;
                                    }
                            }

                            if (block && check.Time - ServerUtils.CurrentTimeMillis() < 1000 && check.Count > 10)
                            {
                                check.Count++;
                                _session.CloseMessage();
                                Server.Gi().Logger.Info($"Ip: {ipv4} ----------- Call -74 qua nhieu, block IP.....");
                                FireWall.BanIp(ipv4);
                            }

                            break;
                        }
                    //CHAT PRIVATE
                    case -72:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var charReal = (Character)character;
                            var charId = message.Reader.ReadInt();
                            if (charId == character.Id) return;
                            var text = ServerUtils.FilterWords(message.Reader.ReadUTF());
                            var charTemp = ClientManager.Gi().GetCharacter(charId);
                            var info = charReal.Friends.FirstOrDefault(friend => friend.Id == charId);
                            if (charTemp != null)
                            {
                                if (info != null)
                                {
                                    character.CharacterHandler.SendMessage(Service.ListFriend3(info.Id, true));
                                }

                                charTemp.CharacterHandler.SendMessage(Service.WorldChat((Character)character, text, 1));
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().USER_OFFLINE));
                                if (info != null)
                                {
                                    character.CharacterHandler.SendMessage(Service.ListFriend3(info.Id, false));
                                }
                            }

                            break;
                        }
                    //CHAT THẾ GIỚI
                    case -71:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            {
                                var @char = (Character)character;
                                var delayChat = @char.InfoChar.ThoiGianChatTheGioi;
                                var timeServer = ServerUtils.CurrentTimeMillis();
                                if (delayChat > timeServer)
                                {
                                    var time = (delayChat - timeServer) / 1000;
                                    character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(string.Format(TextServer.gI().DELAY_CHAT_TG, time)));
                                    return;
                                }

                                if (@char.AllDiamond() < 5)
                                {
                                    character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                                    return;
                                }

                                if (!@char.InfoChar.IsPremium)
                                {
                                    // @char.InfoChar.ThoiGianChatTheGioi = timeServer + 300000;
                                    character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_PREMIUM));
                                    return;
                                }
                                else
                                {
                                    @char.InfoChar.ThoiGianChatTheGioi = timeServer + 60000;
                                }

                                var noiDung = message.Reader.ReadUTF();

                                if (noiDung.Length > 128)
                                {
                                    character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(TextServer.gI().MESSAGE_TO_LONG));
                                    return;
                                }
                                else if (noiDung.Length > 50)
                                {
                                    @char.InfoChar.ThoiGianChatTheGioi = timeServer + 300000;
                                }

                                @char.MineDiamond(5);
                                character.CharacterHandler.SendMessage(Service.BuyItem(@char));
                                ClientManager.Gi().SendMessageCharacter(Service.WorldChat(@char,
                                    ServerUtils.FilterWords(noiDung), 0));
                            }
                            break;
                        }
                    //Send Icon Resources
                    case -67:
                        {
                            _session.SendMessage(Service.SendIcon(_session.ZoomLevel, message.Reader.ReadInt()));
                            break;
                        }
                    //Send Effect Resources
                    case -66:
                        {
                            _session.SendMessage(Service.SendEffect(_session.ZoomLevel, message.Reader.ReadShort()));
                            break;
                        }
                    //GET CLAN IMAGE
                    case -63:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var imgId = message.Reader.ReadByte();
                            if (imgId == -1) return;
                            var clanImage = Cache.Gi().CLAN_IMAGES.FirstOrDefault(clan => clan.Id == imgId);
                            if (clanImage != null)
                            {
                                character.CharacterHandler.SendMessage(Service.GetImageBag(clanImage));
                            }

                            break;
                        }
                    //Send Clan Image
                    case -62:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var imgId = message.Reader.ReadByte();
                            var clanImage = Cache.Gi().CLAN_IMAGES.FirstOrDefault(clan => clan.Id == imgId);
                            if (clanImage != null)
                            {
                                character.CharacterHandler.SendMessage(Service.SendClanImage(clanImage));
                            }

                            break;
                        }
                    //PLAYER_ATTACK_PLAYER
                    case -60:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null || character.IsDontMove()) return;
                            SkillHandler.AttackPlayer(character, message);
                            break;
                        }
                    //thách đấu
                    case -59:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null || character.InfoChar.IsDie) return;
                            character.CharacterHandler.SendMessage(Service.DialogMessage("Chức năng hiện tại đang bảo trì"));
                            // if (DataCache.IdMapCustom.Contains(character.InfoChar.MapId))
                            // {
                            //     character.CharacterHandler.SendMessage(
                            //         Service.ServerMessage(TextServer.gI().DOT_NOT_TEST_HERE));
                            //     return;
                            // }

                            // var real = (Character) character;

                            // if (!real.InfoChar.IsPremium)
                            // {
                            //     character.CharacterHandler.SendMessage(
                            //         Service.ServerMessage(TextServer.gI().NOT_PREMIUM));
                            //     return;
                            // }

                            // var action = message.Reader.ReadByte();
                            // var type = message.Reader.ReadByte();
                            // var idChar = message.Reader.ReadInt();
                            // Server.Gi().Logger
                            //     .Debug(
                            //         $"Invite to TEST --------------- ------- action: {action} - type: {type} - idChar: {idChar}");
                            // switch (action)
                            // {
                            //     case 0:
                            //     {
                            //         switch (type)
                            //         {
                            //             //Select Menu
                            //             case 3:
                            //             {
                            //                 if (real.Test.IsTest)
                            //                 {
                            //                     character.CharacterHandler.SendMessage(
                            //                         Service.ServerMessage(TextServer.gI().NOT_TEST_ME));
                            //                     return;
                            //                 }

                            //                 var player = (Character) character.Zone?.ZoneHandler?.GetCharacter(idChar);
                            //                 if (player == null || player.Test == null)
                            //                 {
                            //                     character.CharacterHandler.SendMessage(
                            //                         Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                            //                     return;
                            //                 }

                            //                 if (player.Test.IsTest)
                            //                 {
                            //                     character.CharacterHandler.SendMessage(
                            //                         Service.ServerMessage(TextServer.gI().NOT_TEST));
                            //                     return;
                            //                 }

                            //                 if (!player.InfoChar.IsPremium)
                            //                 {
                            //                     character.CharacterHandler.SendMessage(
                            //                         Service.ServerMessage(TextServer.gI().PLAYER_NOT_PREMIUM));
                            //                     return;
                            //                 }

                            //                 real.Test.CheckId = player.Id;
                            //                 character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5,
                            //                     string.Format(MenuNpc.Gi().TextMeo[6], player.Name,
                            //                         ServerUtils.GetPower(player.InfoChar.Power)), MenuNpc.Gi().MenuMeo[2],
                            //                     character.InfoChar.Gender));
                            //                 real.TypeMenu = 0;
                            //                 break;
                            //             }
                            //         }

                            //         break;
                            //     }
                            //     case 1:
                            //     {
                            //         switch (type)
                            //         {
                            //             //Accept test
                            //             case 3:
                            //             {
                            //                 if (real.Test.IsTest)
                            //                 {
                            //                     character.CharacterHandler.SendMessage(
                            //                         Service.ServerMessage(TextServer.gI().NOT_TEST_ME));
                            //                     return;
                            //                 }

                            //                 var player = (Character) character.Zone?.ZoneHandler?.GetCharacter(idChar);
                            //                 if (player == null)
                            //                 {
                            //                     character.CharacterHandler.SendMessage(
                            //                         Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                            //                     return;
                            //                 }

                            //                 if (player.Test.IsTest)
                            //                 {
                            //                     character.CharacterHandler.SendMessage(
                            //                         Service.ServerMessage(TextServer.gI().NOT_TEST));
                            //                     return;
                            //                 }

                            //                 if (real.InfoChar.Gold < real.Test.GoldTest)
                            //                 {
                            //                     character.CharacterHandler.SendMessage(
                            //                         Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                            //                     real.Test.IsTest = false;
                            //                     real.Test.TestCharacterId = -1;
                            //                     real.Test.CheckId = -1;
                            //                     real.Test.GoldTest = 0;
                            //                     return;
                            //                 }

                            //                 real.MineGold(real.Test.GoldTest);
                            //                 real.Test.IsTest = true;
                            //                 real.Test.TestCharacterId = player.Id;
                            //                 real.InfoChar.TypePk = 3;

                            //                 player.MineGold(player.Test.GoldTest);
                            //                 player.Test.IsTest = true;
                            //                 player.Test.TestCharacterId = real.Id;
                            //                 player.InfoChar.TypePk = 3;

                            //                 real.CharacterHandler.SendMessage(Service.MeLoadInfo(real));
                            //                 player.CharacterHandler.SendMessage(Service.MeLoadInfo(player));

                            //                 real.CharacterHandler.SendZoneMessage(Service.ChangeTypePk(real.Id, 3));
                            //                 player.CharacterHandler.SendZoneMessage(Service.ChangeTypePk(player.Id, 3));
                            //                 break;
                            //             }
                            //         }

                            //         break;
                            //     }
                            // }

                            break;
                        }
                    //Invite to clan
                    case -57:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;

                            if (DatabaseManager.Manager.gI().IsVIPServer)
                            {
                                character.CharacterHandler.SendMessage(Service.DialogMessage(TextServer.gI().DOT_IT_ON_NORMAL));
                                return;
                            }

                            var @charReal = (Character)character;
                            var action = message.Reader.ReadByte();
                            Server.Gi().Logger.Debug($"Invite to Clan --------------- ------- action: {action}");
                            var map = DataCache.IdMapCustom.Contains(character.InfoChar.MapId)
                                ? MapManager.GetMapCustom(character.InfoChar.MapCustomId)
                                    .GetMapById(character.InfoChar.MapId)
                                : MapManager.Get(character.InfoChar.MapId);
                            var zone = map?.GetZoneById(character.InfoChar.ZoneId);
                            if (zone == null) return;

                            switch (action)
                            {
                                case 0:
                                    {
                                        if (@charReal.ClanId == -1) return;
                                        var clan = ClanManager.Get(@charReal.ClanId);
                                        if (clan == null) return;
                                        if (clan.CurrMember >= clan.MaxMember)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().MAX_MEMBER));
                                            break;
                                        }

                                        var playerId = message.Reader.ReadInt();
                                        var getCharZone = (Character)zone.ZoneHandler.GetCharacter(playerId);
                                        if (getCharZone == null)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().NOT_FOUND_CHAR_IN_MAP));
                                            return;
                                        }

                                        if (clan.ClanHandler.GetMember(playerId) != null || getCharZone.ClanId != -1)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().IN_CLAN_2));
                                            return;
                                        }

                                        var code = int.Parse($"{ServerUtils.RandomNumber(1000, 9999)}{character.Id}");
                                        charReal.CodeInviteClan = code;
                                        var invite = string.Format(TextServer.gI().INVITE_CLAN, character.Name, charReal.Name);
                                        getCharZone.CharacterHandler.SendMessage(Service.InviteClan(invite, clan.Id, code));
                                        break;
                                    }
                                case 1:
                                    {
                                        if (charReal.ClanId != -1)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().IN_CLAN));
                                            return;
                                        }

                                        var clanId = message.Reader.ReadInt();
                                        var code = message.Reader.ReadInt();
                                        code = int.Parse(code.ToString().Substring(4));

                                        var clan = ClanManager.Get(clanId);
                                        if (clan == null)
                                        {
                                            return;
                                        }

                                        if (clan.ClanHandler.GetMember(character.Id) != null)
                                        {
                                            return;
                                        }

                                        if (clan.ClanHandler.AddMember(charReal, 2))
                                        {
                                            charReal.ClanId = clanId;
                                            charReal.InfoChar.Bag = (sbyte)clan.ImgId;
                                            var img = Cache.Gi().CLAN_IMAGES.FirstOrDefault(i => i.Id == clan.ImgId);
                                            charReal.CharacterHandler.SendMessage(Service.GetImageBag(img));
                                            if (charReal.InfoChar.PhukienPart == -1) charReal.CharacterHandler.SendZoneMessage(
                                                Service.SendImageBag(charReal.Id, clan.ImgId));
                                            charReal.CharacterHandler.SendMessage(
                                                Service.ServerMessage(string.Format(TextServer.gI().ACCEPT_INVITE_CLAN,
                                                    clan.Name)));
                                            charReal.CharacterHandler.SendMessage(Service.MyClanInfo(charReal));
                                            charReal.CharacterHandler.SendZoneMessage(
                                                Service.UpdateClanId(charReal.Id, clan.Id));
                                            clan.ClanHandler.UpdateClanId();
                                            CharacterDB.Update(charReal);
                                            ClanDB.Update(clan);
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    //Change Leader
                    case -56:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            if (DatabaseManager.Manager.gI().IsVIPServer)
                            {
                                character.CharacterHandler.SendMessage(Service.DialogMessage(TextServer.gI().DOT_IT_ON_NORMAL));
                                return;
                            }
                            var @charReal = (Character)character;
                            var clan = ClanManager.Get(charReal.ClanId);
                            if (clan == null) return;
                            var playerId = message.Reader.ReadInt();
                            var role = message.Reader.ReadByte();
                            Server.Gi().Logger
                                .Debug($"Clan Change Leader --------------- ------- playerId: {playerId} - role: {role}");
                            if (playerId == character.Id) return;
                            var me = clan.ClanHandler.GetMember(character.Id);
                            var member = clan.ClanHandler.GetMember(playerId);
                            if (me == null || member == null) return;
                            var lastMess = clan.Messages.LastOrDefault();
                            var id = lastMess != null ? lastMess.Id + 1 : 0;
                            switch (role)
                            {
                                //Loại
                                case -1:
                                    {
                                        if (me.Role == 2) return;
                                        if (clan.ClanHandler.RemoveMember(member.Id))
                                        {
                                            clan.ClanHandler.Chat(new ClanMessage()
                                            {
                                                Type = 0,
                                                Id = id,
                                                PlayerId = -1,
                                                PlayerName = "Thông báo",
                                                Role = me.Role,
                                                Time = ServerUtils.CurrentTimeSecond() - 1000000000,
                                                Text = string.Format(TextServer.gI().REMOVE_MEMBER, member.Name),
                                                Color = 1,
                                                NewMessage = true,
                                            });
                                            var charRemove = (Character)ClientManager.Gi().GetCharacter(member.Id);
                                            if (charRemove != null)
                                            {
                                                charRemove.ClanId = -1;
                                                charRemove.InfoChar.Bag = -1;
                                                if (character.InfoChar.PhukienPart == -1) character.CharacterHandler.SendZoneMessage(
                                                    Service.SendImageBag(character.Id, -1));
                                                character.CharacterHandler.SendMessage(Service.GetImageBag(null));
                                                charRemove.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().KICKED_CLAN));
                                                charRemove.CharacterHandler.SendMessage(Service.MyClanInfo());
                                                charRemove.CharacterHandler.SendZoneMessage(
                                                    Service.UpdateClanId(charRemove.Id, -1));
                                                clan.ClanHandler.UpdateClanId();
                                                clan.ClanHandler.Flush();
                                                CharacterDB.Update(charRemove);
                                            }
                                            else
                                            {
                                                CharacterDB.Update(member.Id);
                                            }
                                        }

                                    ;
                                        break;
                                    }
                                case 0:
                                    {
                                        if (me.Role != 0) return;
                                        me.Role = 2;
                                        member.Role = 0;
                                        clan.ClanHandler.Chat(new ClanMessage()
                                        {
                                            Type = 0,
                                            Id = id,
                                            PlayerId = -1,
                                            PlayerName = "Thông báo",
                                            Role = 0,
                                            Time = ServerUtils.CurrentTimeSecond() - 1000000000,
                                            Text = string.Format(TextServer.gI().CHANGED_LEADER_CLAN, member.Name),
                                            Color = 1,
                                            NewMessage = true,
                                        });
                                        clan.ClanHandler.SendMessage(Service.ChangeMemberClan(me));
                                        clan.ClanHandler.SendMessage(Service.ChangeMemberClan(member));
                                        clan.ClanHandler.Flush();
                                        break;
                                    }
                                case 1:
                                    {
                                        if (me.Role == 2) return;
                                        member.Role = 1;
                                        clan.ClanHandler.Chat(new ClanMessage()
                                        {
                                            Type = 0,
                                            Id = id,
                                            PlayerId = -1,
                                            PlayerName = "Thông báo",
                                            Role = me.Role,
                                            Time = ServerUtils.CurrentTimeSecond() - 1000000000,
                                            Text = string.Format(TextServer.gI().CHANGED_SUBLEADER_CLAN, member.Name),
                                            Color = 1,
                                            NewMessage = true,
                                        });
                                        clan.ClanHandler.SendMessage(Service.ChangeMemberClan(member));
                                        clan.ClanHandler.Flush();
                                        break;
                                    }
                                case 2:
                                    {
                                        if (me.Role != 0) return;
                                        member.Role = 2;
                                        clan.ClanHandler.Chat(new ClanMessage()
                                        {
                                            Type = 0,
                                            Id = id,
                                            PlayerId = -1,
                                            PlayerName = "Thông báo",
                                            Role = me.Role,
                                            Time = ServerUtils.CurrentTimeSecond() - 1000000000,
                                            Text = string.Format(TextServer.gI().REMOVE_SUBLEADER_CLAN, member.Name),
                                            Color = 1,
                                            NewMessage = true,
                                        });
                                        clan.ClanHandler.SendMessage(Service.ChangeMemberClan(member));
                                        clan.ClanHandler.Flush();
                                        break;
                                    }
                            }

                            break;
                        }
                    //Leave Clan
                    case -55:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            if (DatabaseManager.Manager.gI().IsVIPServer)
                            {
                                character.CharacterHandler.SendMessage(Service.DialogMessage(TextServer.gI().DOT_IT_ON_NORMAL));
                                return;
                            }
                            var @charReal = (Character)character;
                            var clan = ClanManager.Get(charReal.ClanId);
                            if (clan == null) return;
                            var me = clan.ClanHandler.GetMember(character.Id);
                            if (me.Role == 0)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().DONT_LEAVE_CLAN));
                                return;
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(Service.OpenUiConfirm(5,
                                    MenuNpc.Gi().TextMeo[5], MenuNpc.Gi().MenuMeo[1],
                                    character.InfoChar.Gender));
                                @charReal.TypeMenu = 6;
                            }

                            break;
                        }
                    //Cho đậu
                    case -54:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            if (DatabaseManager.Manager.gI().IsVIPServer)
                            {
                                character.CharacterHandler.SendMessage(Service.DialogMessage(TextServer.gI().DOT_IT_ON_NORMAL));
                                return;
                            }
                            var @charReal = (Character)character;

                            var timeServer = ServerUtils.CurrentTimeMillis();

                            if (Server.Gi().StartServerTime > timeServer)
                            {
                                var delay = (Server.Gi().StartServerTime - timeServer) / 1000;
                                if (delay < 1)
                                {
                                    delay = 1;
                                }

                                character.CharacterHandler.SendMessage(Service.DialogMessage(string.Format(TextServer.gI().DELAY_RESTART_SEC,
                                        delay)));
                                return;
                            }

                            if (Maintenance.Gi().IsStart)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Máy chủ đang tiến hành bảo trì, không thể thao tác ngay lúc này, vui lòng thoát game"));
                                return;
                            }

                            if (charReal.Delay.InvAction > timeServer)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn thao tác quá nhanh, chậm lại nhé"));
                                return;
                            }

                            var clan = ClanManager.Get(charReal.ClanId);
                            if (clan == null) return;
                            var id = message.Reader.ReadInt();
                            Server.Gi().Logger.Debug($"Clan Donate --------------- ------- id: {id}");
                            if (!character.CheckLockInventory()) return;
                            lock (clan.Messages)
                            {
                                var msg = clan.ClanHandler.GetMessage(id);
                                if (msg is not { Type: 1 } || msg.Recieve >= msg.MaxCap) return;
                                if (msg.PlayerId == character.Id) return;
                                var itemPea =
                                    character.ItemBox.FirstOrDefault(item => DataCache.IdDauThan.Contains(item.Id));
                                if (itemPea == null)
                                {
                                    character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(TextServer.gI().NOT_FOUND_PEA_IN_BOX));
                                    return;
                                }
                                var playerMe = clan.ClanHandler.GetMember(msg.PlayerId);
                                if (playerMe == null) return;

                                character.CharacterHandler.RemoveItemBoxByIndex(itemPea.IndexUI, 1);
                                var itemNew = ItemHandler.Clone(itemPea);
                                itemNew.Quantity = 1;

                                var memMe = clan.ClanHandler.GetMember(character.Id);
                                memMe.Donate++;

                                playerMe.ReceiveDonate++;

                                var charPlus = ClientManager.Gi().GetCharacter(msg.PlayerId);
                                if (charPlus != null)
                                {
                                    if (charPlus.CharacterHandler.AddItemToBag(true, itemNew, "Clan cho đậu"))
                                    {
                                        charPlus.CharacterHandler.SendMessage(Service.SendBag(charPlus));
                                    }

                                    ;
                                    charPlus.CharacterHandler.SendMessage(Service.ServerMessage(
                                        string.Format(TextServer.gI().RECEIVE_PEA_CLAN,
                                            ItemCache.ItemTemplate(itemNew.Id).Name, character.Name)));
                                }
                                else
                                {
                                    clan.ClanHandler.AddCharacterPea(new CharacterPea()
                                    {
                                        PeaId = itemNew.Id,
                                        PlayerGive = character.Name,
                                        PlayerRevice = msg.PlayerId,
                                        Quantity = 1
                                    });
                                }

                                msg.Recieve++;
                                clan.ClanHandler.Chat(msg);
                            }

                            break;
                        }
                    //Chat clan
                    case -51:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var @charReal = (Character)character;
                            var action = message.Reader.ReadByte();
                            Server.Gi().Logger.Debug($"Chat Clan --------------- ------- action: {action}");
                            switch (action)
                            {
                                case 0:
                                    {
                                        var clan = ClanManager.Get(charReal.ClanId);
                                        var member = clan?.ClanHandler.GetMember(character.Id);
                                        if (member == null) return;
                                        var text = ServerUtils.FilterWords(message.Reader.ReadUTF());
                                        var lastMess = clan.Messages.LastOrDefault();
                                        var id = lastMess != null ? lastMess.Id + 1 : 0;
                                        clan.ClanHandler.Chat(new ClanMessage()
                                        {
                                            Type = 0,
                                            PlayerId = character.Id,
                                            PlayerName = character.Name,
                                            Time = ServerUtils.CurrentTimeSecond() - 1000000000,
                                            Text = text,
                                            Role = member.Role,
                                            Id = id,
                                            Color = 0,
                                        });
                                        break;
                                    }
                                //Xin đậu
                                case 1:
                                    {
                                        var clan = ClanManager.Get(charReal.ClanId);
                                        var member = clan?.ClanHandler.GetMember(character.Id);
                                        if (member == null) return;
                                        if (member.DelayPea > ServerUtils.CurrentTimeMillis())
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().DELAY_REVICE_PEA));
                                            return;
                                        }

                                        member.DelayPea = ServerUtils.CurrentTimeMillis() + 300000;

                                        var messCheck =
                                            clan.Messages.FirstOrDefault(m => m.Type == 1 && m.PlayerId == character.Id);
                                        ClanMessage newMes;
                                        if (messCheck == null)
                                        {
                                            var lastMess = clan.Messages.LastOrDefault();
                                            var id = lastMess != null ? lastMess.Id + 1 : 0;
                                            newMes = new ClanMessage()
                                            {
                                                Type = 1,
                                                PlayerId = character.Id,
                                                PlayerName = character.Name,
                                                Time = ServerUtils.CurrentTimeSecond() - 1000000000,
                                                Role = member.Role,
                                                Id = id,
                                                NewMessage = true,
                                                Color = 0,
                                                Recieve = 0,
                                                MaxCap = 5
                                            };
                                        }
                                        else
                                        {
                                            messCheck.Recieve = 0;
                                            messCheck.Time = ServerUtils.CurrentTimeSecond() - 1000000000;
                                            newMes = messCheck;
                                        }

                                        clan.ClanHandler.Chat(newMes);
                                        break;
                                    }
                                case 2:
                                    {
                                        var clan = ClanManager.Get(message.Reader.ReadInt());
                                        if (clan == null) return;

                                        if (clan.CurrMember >= clan.MaxMember)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().MAX_MEMBER));
                                            break;
                                        }

                                        var messCheck =
                                            clan.Messages.FirstOrDefault(m => m.Type == 2 && m.PlayerId == character.Id);
                                        if (messCheck == null)
                                        {
                                            var lastMess = clan.Messages.LastOrDefault();
                                            var id = lastMess != null ? lastMess.Id + 1 : 0;
                                            messCheck = new ClanMessage()
                                            {
                                                Type = 2,
                                                PlayerId = character.Id,
                                                PlayerName = character.Name,
                                                Time = ServerUtils.CurrentTimeSecond() - 1000000000,
                                                Role = 0,
                                                Id = id,
                                                NewMessage = true,
                                                Color = 1,
                                                Text = string.Format(TextServer.gI().PLEASE_INVITE_CLAN, character.Name)
                                            };
                                            clan.ClanHandler.Chat(messCheck);
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    //View List Member Clan
                    case -50:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var clanId = message.Reader.ReadInt();
                            var clan = ClanManager.Get(clanId);
                            if (clan == null)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().ERROR_FIND_CLAN));
                                return;
                            }

                            character.CharacterHandler.SendMessage(Service.ClanMember(clan.Members));
                            break;
                        }
                    //Accept Please Join Clan
                    case -49:
                        {
                            var character = _session?.Player?.Character;
                            if (DatabaseManager.Manager.gI().IsVIPServer)
                            {
                                character.CharacterHandler.SendMessage(Service.DialogMessage(TextServer.gI().DOT_IT_ON_NORMAL));
                                return;
                            }
                            if (character == null) return;
                            var @charReal = (Character)character;
                            var clan = ClanManager.Get(charReal.ClanId);
                            if (clan == null) return;
                            var memMe = clan.ClanHandler.GetMember(character.Id);
                            if (memMe.Role != 0) return;
                            var id = message.Reader.ReadInt();
                            var action = message.Reader.ReadByte();
                            var msg = clan.ClanHandler.GetMessage(id);
                            if (msg == null) return;
                            switch (action)
                            {
                                //Đồng ý
                                case 0:
                                    {
                                        var text = "";
                                        if (clan.ClanHandler.GetMember(msg.PlayerId) != null)
                                        {
                                            text = string.Format(TextServer.gI().IN_CLAN_3, msg.PlayerName);
                                        }
                                        else
                                        {
                                            var checkChar = (Character)ClientManager.Gi().GetCharacter(msg.PlayerId);
                                            if (checkChar != null)
                                            {
                                                if (checkChar.ClanId != -1)
                                                {
                                                    text = string.Format(TextServer.gI().IN_CLAN_4, msg.PlayerName);
                                                }
                                                else
                                                {
                                                    if (clan.ClanHandler.AddMember(checkChar, 2))
                                                    {
                                                        text = string.Format(TextServer.gI().JOINED_CLAN, msg.PlayerName);
                                                        checkChar.ClanId = clan.Id;
                                                        checkChar.InfoChar.Bag = (sbyte)clan.ImgId;
                                                        var img = Cache.Gi().CLAN_IMAGES
                                                            .FirstOrDefault(i => i.Id == clan.ImgId);
                                                        checkChar.CharacterHandler.SendMessage(Service.GetImageBag(img));
                                                        if (checkChar.InfoChar.PhukienPart == -1) checkChar.CharacterHandler.SendZoneMessage(
                                                            Service.SendImageBag(checkChar.Id, clan.ImgId));
                                                        checkChar.CharacterHandler.SendMessage(
                                                            Service.ServerMessage(
                                                                string.Format(TextServer.gI().ACCEPT_INVITE_CLAN, clan.Name)));
                                                        checkChar.CharacterHandler.SendMessage(Service.MyClanInfo(checkChar));
                                                        checkChar.CharacterHandler.SendZoneMessage(
                                                            Service.UpdateClanId(checkChar.Id, clan.Id));
                                                        clan.ClanHandler.UpdateClanId();
                                                        CharacterDB.Update(checkChar);
                                                        ClanDB.Update(clan);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                lock (Server.SQLLOCK)
                                                {
                                                    var charCheckDb = CharacterDB.GetById(msg.PlayerId);

                                                    if (charCheckDb != null)
                                                    {
                                                        if (charCheckDb.ClanId != -1)
                                                        {
                                                            text = string.Format(TextServer.gI().IN_CLAN_4, msg.PlayerName);
                                                        }
                                                        else if (clan.ClanHandler.AddMember(charCheckDb, 2))
                                                        {
                                                            text = string.Format(TextServer.gI().JOINED_CLAN, msg.PlayerName);
                                                            charCheckDb.ClanId = clan.Id;
                                                            charCheckDb.InfoChar.Bag = (sbyte)clan.ImgId;
                                                            clan.ClanHandler.UpdateClanId();
                                                            ClanDB.Update(clan);
                                                            CharacterDB.Update(charCheckDb);
                                                        }
                                                    }

                                                }
                                            }
                                        }

                                        msg = new ClanMessage()
                                        {
                                            Type = 0,
                                            PlayerId = -1,
                                            PlayerName = "Thông báo",
                                            Time = ServerUtils.CurrentTimeSecond() - 1000000000,
                                            Text = text,
                                            Role = 0,
                                            Id = msg.Id,
                                            Color = 1,
                                        };
                                        clan.ClanHandler.Chat(msg);
                                        break;
                                    }
                                //Từ chối
                                case 1:
                                    {
                                        msg = new ClanMessage()
                                        {
                                            Type = 0,
                                            PlayerId = -1,
                                            PlayerName = "Thông báo",
                                            Time = ServerUtils.CurrentTimeSecond() - 1000000000,
                                            Text = $"Từ chối xin gia nhập của {msg.PlayerName}",
                                            Role = 0,
                                            Id = msg.Id,
                                            Color = 1,
                                        };
                                        clan.ClanHandler.Chat(msg);
                                        break;
                                    }
                            }

                            break;
                        }
                    //Search Clan
                    case -47:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var charReal = (Character)character;
                            if (charReal.ClanId != -1)
                            {
                                character.CharacterHandler.SendMessage(Service.MyClanInfo(charReal));
                                return;
                            }

                            var name = message.Reader.ReadUTF();
                            name = name.ToLower();
                            List<Clan> clan;
                            clan = name.Equals("") ? ClanManager.Entrys.Values.ToList() : ClanManager.GetList(name);
                            character.CharacterHandler.SendMessage(Service.ClanSearch(clan));
                            break;
                        }
                    //Create Clan
                    case -46:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            if (DatabaseManager.Manager.gI().IsVIPServer)
                            {
                                character.CharacterHandler.SendMessage(Service.DialogMessage(TextServer.gI().DOT_IT_ON_NORMAL));
                                return;
                            }
                            var @charReal = (Character)character;
                            var action = message.Reader.ReadByte();
                            Server.Gi().Logger.Debug($"Create Clan --------------- ------- action: {action}");
                            switch (action)
                            {
                                case 0:
                                    {
                                        break;
                                    }
                                case 1:
                                    {
                                        var clan = ClanManager.Entrys.Values.ToList();
                                        character.CharacterHandler.SendMessage(Service.ClanSearch(clan));
                                        character.CharacterHandler.SendMessage(Service.CreateClan(1));
                                        break;
                                    }
                                //Tạo mới clan
                                case 2:
                                    {
                                        if (@charReal.ClanId != -1)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().IN_CLAN));
                                            return;
                                        }

                                        var id = message.Reader.ReadByte();
                                        var name = message.Reader.ReadUTF();
                                        name = name.ToLower();
                                        if (name.Length is < 3 or > 25)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().ERROR_CREATE_CLAN));
                                            return;
                                        }

                                        if (ClanManager.Get(name) != null)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().DUPLICATE_CLAN));
                                            return;
                                        }

                                        var image = Cache.Gi().CLAN_IMAGES.FirstOrDefault(im => im.Id == id);
                                        if (image == null || id >= 30)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().ERROR_CREATE_CLAN));
                                            return;
                                        }


                                        if (image.Gold != -1)
                                        {
                                            if (image.Gold > character.InfoChar.Gold)
                                            {
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            if (image.Diamond > @charReal.AllDiamond())
                                            {
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                                                return;
                                            }
                                        }

                                        var timeServer = ServerUtils.CurrentTimeSecond();
                                        var clanNew = new Clan()
                                        {
                                            Name = name,
                                            Slogan = "",
                                            ImgId = id,
                                            Power = 0,
                                            LeaderName = character.Name,
                                            CurrMember = 0,
                                            MaxMember = 10,
                                            Date = timeServer,
                                        };
                                        clanNew.ClanHandler = new ClanHandler(clanNew);
                                        if (clanNew.ClanHandler.AddMember((Character)character, isFlush: false))
                                        {
                                            var clanId = ClanDB.Create(clanNew);
                                            if (clanId > 0)
                                            {
                                                clanNew.Id = clanId;
                                                ClanManager.Add(clanNew);
                                                if (image.Gold != -1)
                                                {
                                                    @charReal.MineGold(image.Gold);
                                                }
                                                else
                                                {
                                                    @charReal.MineDiamond(image.Diamond);
                                                }

                                                @charReal.ClanId = clanId;
                                                character.InfoChar.Bag = id;
                                                if (character.InfoChar.PhukienPart == -1) character.CharacterHandler.SendZoneMessage(
                                                    Service.SendImageBag(character.Id, id));
                                                character.CharacterHandler.SendMessage(Service.GetImageBag(image));
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(string.Format(TextServer.gI().SUCCESS_CREATE_CLAN,
                                                        clanNew.Name)));
                                                character.CharacterHandler.SendMessage(Service.MeLoadInfo(@charReal));
                                                character.CharacterHandler.SendMessage(Service.ClosePanel());
                                                character.CharacterHandler.SendMessage(Service.MyClanInfo(charReal));
                                                CharacterDB.Update(charReal);
                                            }
                                            else
                                            {
                                                character.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().ERROR_CREATE_CLAN));
                                                return;
                                            }
                                        }

                                        break;
                                    }
                                //Chọn biếu tượng clan
                                case 3:
                                    {
                                        character.CharacterHandler.SendMessage(Service.CreateClan(1));
                                        break;
                                    }
                                case 4:
                                    {
                                        var id = message.Reader.ReadByte();
                                        var name = message.Reader.ReadUTF();
                                        var clan = ClanManager.Get(@charReal.ClanId);
                                        var isUpdate = false;

                                        if (clan != null)
                                        {
                                            if (clan.ImgId != id)
                                            {
                                                var img = Cache.Gi().CLAN_IMAGES.FirstOrDefault(i => i.Id == id);
                                                if (img != null)
                                                {
                                                    if (img.Gold != -1)
                                                    {
                                                        if (img.Gold > character.InfoChar.Gold)
                                                        {
                                                            character.CharacterHandler.SendMessage(
                                                                Service.ServerMessage(TextServer.gI().NOT_ENOUGH_GOLD));
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            charReal.MineGold(img.Gold);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (img.Diamond > @charReal.AllDiamond())
                                                        {
                                                            character.CharacterHandler.SendMessage(
                                                                Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            charReal.MineDiamond(img.Diamond);
                                                        }
                                                    }

                                                    clan.ImgId = img.Id;
                                                    if (character.InfoChar.PhukienPart == -1) character.CharacterHandler.SendZoneMessage(Service.SendImageBag(character.Id, img.Id));
                                                    character.CharacterHandler.SendMessage(Service.GetImageBag(img));
                                                    character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));
                                                    isUpdate = true;
                                                }
                                            }

                                            if (!name.Equals(""))
                                            {
                                                clan.Slogan = ServerUtils.FilterWords(name.ToLower());
                                                isUpdate = true;
                                            }

                                            character.CharacterHandler.SendMessage(Service.UpdateClan(clan.ImgId, clan.Slogan));

                                            if (isUpdate)
                                            {
                                                ClanDB.Update(clan);
                                            }
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    //Skill not focus
                    case -45:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null || character.InfoChar.IsDie ||
                                character.InfoChar.MapId - 21 == character.InfoChar.Gender) return;
                            if (character.InfoChar.Stamina <= 0)
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().NOT_ENOUGH_STAMINA));
                                return;
                            }

                            SkillHandler.SkillNotFocus(character, message);
                            break;
                        }

                    //USE_ITEM
                    case -43:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character != null)
                            {
                                if (Maintenance.Gi().IsStart)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage("Máy chủ đang tiến hành bảo trì, không thể thao tác ngay lúc này, vui lòng thoát game"));
                                    return;
                                }
                                var timeServer = ServerUtils.CurrentTimeMillis();
                                if (Server.Gi().StartServerTime > timeServer)
                                {
                                    var delay = (Server.Gi().StartServerTime - timeServer) / 1000;
                                    if (delay < 1)
                                    {
                                        delay = 1;
                                    }

                                    character.CharacterHandler.SendMessage(Service.DialogMessage(string.Format(TextServer.gI().DELAY_RESTART_SEC,
                                            delay)));
                                    return;
                                }
                                if (character.Delay.InvAction > timeServer)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn thao tác quá nhanh, chậm lại nhé"));
                                    return;
                                }

                                if (character.Trade.IsTrade)
                                {
                                    character.CharacterHandler.SendMessage(Service.DialogMessage("Không thể thực hiện thao tác này khi đang giao dịch"));
                                    return;
                                }

                                var type = message.Reader.ReadByte();
                                var where = message.Reader.ReadByte();
                                var index = message.Reader.ReadByte();
                                short template = -1;
                                if (index == -1) template = message.Reader.ReadShort();

                                Server.Gi().Logger
                                    .Debug(
                                        $"User Item --------------- Type: {type} - where: {where} - index: {index} - template: {template}");
                                if (!character.CheckLockInventory()) return;
                                switch (type)
                                {
                                    //Use item
                                    case 0:
                                        {
                                            switch (where)
                                            {
                                                case 0: break;
                                                //In bag
                                                case 1:
                                                    {
                                                        ItemHandler.UseItemBag((Character)character, index, template);
                                                        break;
                                                    }
                                                //In Box
                                                case 2:
                                                    {
                                                        ItemHandler.UseItemBox((Character)character, index, template);
                                                        break;
                                                    }
                                            }

                                            break;
                                        }
                                    //Drop Item
                                    case 1:
                                        {
                                            if (DataCache.IdMapCustom.Contains(character.InfoChar.MapId))
                                            {
                                                _session.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().ITEM_CANNOT_BE_DROPPED_HERE));
                                                return;
                                            }

                                            var checkWp = MapManager.Get(character.InfoChar.MapId)?.TileMap.WayPoints
                                                .FirstOrDefault(waypoint => CheckTrueWaypoint(character, waypoint, 30));
                                            if (checkWp != null)
                                            {
                                                _session.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().ITEM_CANNOT_BE_DROPPED_NEAR_WP));
                                                return;
                                            }

                                            switch (where)
                                            {
                                                //Drop body
                                                case 0:
                                                    {
                                                        var itemDrop = character.ItemBody[index];
                                                        if (itemDrop == null) return;
                                                        if (itemDrop.Id == 457) return;

                                                        var itemTemplate = ItemCache.ItemTemplate(itemDrop.Id);
                                                        if (itemTemplate == null) return;

                                                        if (DatabaseManager.Manager.gI().IsDropAll)
                                                        {
                                                            _session.SendMessage(Service.UseItem(1, 2, index,
                                                                String.Format(TextServer.gI().CONFIRM_DROP_ITEM,
                                                                    itemTemplate.Name)));
                                                            return;
                                                        }

                                                        if ((!itemTemplate.IsDrop &&
                                                             !DataCache.TypeItemRemove.Contains(itemTemplate.Type)) ||
                                                            (itemDrop.Id == 193 && itemDrop.Quantity == 99))
                                                        {
                                                            _session.SendMessage(
                                                                Service.ServerMessage(TextServer.gI().CANT_DROP_ITEM));
                                                            return;
                                                        }

                                                        if (!itemTemplate.IsDrop &&
                                                            DataCache.TypeItemRemove.Contains(itemTemplate.Type))
                                                        {
                                                            _session.SendMessage(Service.UseItem(1, 2, index,
                                                                String.Format(TextServer.gI().CONFIRM_DROP_ITEM,
                                                                    itemTemplate.Name)));
                                                            return;
                                                        }

                                                        character.CharacterHandler.DropItemBody(index);
                                                        break;
                                                    }
                                                //Drop bag
                                                case 1:
                                                    {
                                                        var itemDrop = character.CharacterHandler.GetItemBagByIndex(index);
                                                        if (itemDrop == null)
                                                        {
                                                            return;
                                                        }

                                                        if (itemDrop.Id == 457) return;

                                                        var itemTemplate = ItemCache.ItemTemplate(itemDrop.Id);
                                                        if (itemTemplate == null) return;

                                                        if (DatabaseManager.Manager.gI().IsDropAll)
                                                        {
                                                            _session.SendMessage(Service.UseItem(1, 1, index,
                                                                String.Format(TextServer.gI().CONFIRM_DROP_ITEM,
                                                                    itemTemplate.Name)));
                                                            return;
                                                        }


                                                        if ((!itemTemplate.IsDrop &&
                                                             !DataCache.TypeItemRemove.Contains(itemTemplate.Type)) ||
                                                            (itemDrop.Id == 193 && itemDrop.Quantity == 99))
                                                        {
                                                            _session.SendMessage(
                                                                Service.ServerMessage(TextServer.gI().CANT_DROP_ITEM));
                                                            return;
                                                        }

                                                        if (!itemTemplate.IsDrop &&
                                                            DataCache.TypeItemRemove.Contains(itemTemplate.Type))
                                                        {
                                                            _session.SendMessage(Service.UseItem(1, 1, index,
                                                                String.Format(TextServer.gI().CONFIRM_DROP_ITEM,
                                                                    itemTemplate.Name)));
                                                            return;
                                                        }

                                                        character.CharacterHandler.DropItemBag(index);
                                                        break;
                                                    }
                                            }

                                            break;
                                        }
                                    //Accept Use Item
                                    case 2:
                                        {
                                            switch (where)
                                            {
                                                case 1://Xác nhận xóa vật phẩm
                                                    {
                                                        character.CharacterHandler.RemoveItemBag(index, reason: "Tự Xóa vật phẩm");
                                                        break;
                                                    }
                                                case 2://Xác nhận xóa vật phẩm
                                                    {
                                                        character.CharacterHandler.RemoveItemBody(index);
                                                        break;
                                                    }
                                                case 3://Xác nhận dùng vật phẩm
                                                    {
                                                        ItemHandler.ConfirmUseItemBag((Character)character, index, template);
                                                        break;
                                                    }
                                            }

                                            break;
                                        }
                                }
                            }

                            break;
                        }
                    //Level caption
                    case -41:
                        {
                            int gender = message.Reader.ReadByte();
                            _session.SendMessage(Service.SendLevelCaption(gender));
                            break;
                        }
                    //GET_ITEM
                    case -40:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null) return;
                            if (Maintenance.Gi().IsStart)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Máy chủ đang tiến hành bảo trì, không thể thao tác ngay lúc này, vui lòng thoát game"));
                                return;
                            }

                            if (character.Trade.IsTrade)
                            {
                                character.CharacterHandler.SendMessage(Service.DialogMessage("Không thể thực hiện thao tác này khi đang giao dịch"));
                                return;
                            }

                            var timeServer = ServerUtils.CurrentTimeMillis();
                            if (Server.Gi().StartServerTime > timeServer)
                            {
                                var delay = (Server.Gi().StartServerTime - timeServer) / 1000;
                                if (delay < 1)
                                {
                                    delay = 1;
                                }

                                character.CharacterHandler.SendMessage(Service.DialogMessage(string.Format(TextServer.gI().DELAY_RESTART_SEC,
                                        delay)));
                                return;
                            }
                            if (character.Delay.InvAction > timeServer)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn thao tác quá nhanh, chậm lại nhé"));
                                return;
                            }

                            var type = message.Reader.ReadByte();
                            var id = message.Reader.ReadByte();
                            Server.Gi().Logger.Debug($"GET Item --------------- Type: {type} - Id: {id}");
                            switch (type)
                            {
                                //Item BOX to bag fix
                                case 0:
                                    {
                                        if (character.LengthBagNull() > 0)
                                        {
                                            var itemcheck = character.CharacterHandler.GetItemBoxByIndex(id);
                                            if (itemcheck == null) return;
                                            var itemclone = ItemHandler.Clone(itemcheck);
                                            character.CharacterHandler.AddItemToBag(true, itemclone, "Lấy từ rương qua người");
                                            character.CharacterHandler.RemoveItemBox(id);
                                            character.CharacterHandler.SendMessage(Service.SendBag(character));
                                        }
                                        else
                                        {
                                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                                        }

                                        break;
                                    }
                                // item bag to box fix
                                case 1:
                                    {
                                        if (character.LengthBoxNull() > 0)
                                        {
                                            var itemcheck = character.CharacterHandler.GetItemBagByIndex(id);
                                            if (itemcheck == null) return;
                                            var itemclone = ItemHandler.Clone(itemcheck);
                                            character.CharacterHandler.AddItemToBox(true, itemclone);
                                            character.CharacterHandler.RemoveItemBag(id, reason: "Cất vào rương");
                                            character.CharacterHandler.SendMessage(Service.SendBag(character));
                                            character.CharacterHandler.SendMessage(Service.SendBox(character));
                                        }
                                        else
                                        {
                                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BOX));
                                        }

                                        break;
                                    }





                                /*case 0:
                                {
                                    var itemCheck = character.CharacterHandler.GetItemBoxByIndex(id);
                                    if (itemCheck == null) return;
                                    var itemTemplate = ItemCache.ItemTemplate(itemCheck.Id);
                                    if (DataCache.IdDauThan.Contains(itemTemplate.Id))
                                    {
                                        var @char = (Character) character;
                                        var countDauThan = @char.GetTotalDauThanBag();
                                        var plus = 10 - countDauThan;
                                        if (plus <= 0)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().MAX_PEAS));
                                            return;
                                        }

                                        var itemClone = ItemHandler.Clone(itemCheck);
                                        if (plus < itemCheck.Quantity)
                                        {
                                            itemClone.Quantity = plus;
                                        }
                                        else if (plus >= itemCheck.Quantity)
                                        {
                                            itemClone.Quantity = itemClone.Quantity;
                                        }

                                        if (character.CharacterHandler.AddItemToBag(true, itemClone))
                                        {
                                            character.CharacterHandler.SendMessage(Service.SendBag(character));
                                            if (plus == itemCheck.Quantity)
                                            {
                                                character.CharacterHandler.RemoveItemBox(id);
                                            }
                                            else
                                            {
                                                itemCheck.Quantity -= plus;
                                                character.CharacterHandler.SendMessage(Service.SendBox(character));
                                            }

                                            character.CharacterHandler.SendMessage(Service.SendBag(character));
                                        }
                                    }
                                    else
                                    {
                                        var indexBody = itemTemplate.Type == 32 ? 6 : itemTemplate.Type;
                                        if (itemTemplate.IsTypeBody() && itemTemplate.Require <= character.InfoChar.Power &&
                                            character.ItemBody[indexBody] == null)
                                        {
                                            character.CharacterHandler.RemoveItemBox(id);
                                            character.CharacterHandler.AddItemToBody(itemCheck, indexBody);
                                            character.CharacterHandler.UpdateInfo();
                                        }
                                        else if (character.CharacterHandler.AddItemToBag(true, itemCheck))
                                        {
                                            character.CharacterHandler.RemoveItemBox(id);
                                            character.CharacterHandler.SendMessage(Service.SendBag(character));
                                        }
                                    }

                                    break;
                                }
                                //Item BAG to box
                                case 1:
                                {
                                    var itemCheck = character.CharacterHandler.GetItemBagByIndex(id);
                                    if (itemCheck == null) return;
                                    var itemTemplate = ItemCache.ItemTemplate(itemCheck.Id);
                                    if (DataCache.IdDauThan.Contains(itemCheck.Id))
                                    {
                                        var @char = (Character) character;
                                        var countDauThan = @char.GetTotalDauThanBox();
                                        var plus = 20 - countDauThan;
                                        if (plus <= 0)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().MAX_PEAS));
                                            return;
                                        }

                                        var itemClone = ItemHandler.Clone(itemCheck);
                                        if (plus < itemCheck.Quantity)
                                        {
                                            itemClone.Quantity = plus;
                                        }
                                        else if (plus >= itemCheck.Quantity)
                                        {
                                            itemClone.Quantity = itemCheck.Quantity;
                                        }

                                        if (character.CharacterHandler.AddItemToBox(true, itemClone))
                                        {
                                            character.CharacterHandler.SendMessage(Service.SendBag(character));
                                            if (plus == itemCheck.Quantity)
                                            {
                                                character.CharacterHandler.RemoveItemBag(id);
                                            }
                                            else
                                            {
                                                itemCheck.Quantity -= plus;
                                                character.CharacterHandler.SendMessage(Service.SendBag(character));
                                            }

                                            character.CharacterHandler.SendMessage(Service.SendBox(character));
                                        }
                                    }
                                    else
                                    {
                                        if (character.CharacterHandler.AddItemToBox(true, itemCheck))
                                        {
                                            character.CharacterHandler.RemoveItemBag(id);
                                            character.CharacterHandler.SendMessage(Service.SendBox(character));
                                        }
                                    }

                                    break;
                                }*/
                                case 2:
                                    {
                                        break;
                                    }
                                //Item body to box
                                case 3:
                                    {
                                        var itemBody = character.ItemBody[id];
                                        if (itemBody == null) return;
                                        if (Server.Gi().StartServerTime > timeServer)
                                        {
                                            var delay = (Server.Gi().StartServerTime - timeServer) / 1000;
                                            if (delay < 1)
                                            {
                                                delay = 1;
                                            }

                                            character.CharacterHandler.SendMessage(Service.DialogMessage(string.Format(TextServer.gI().DELAY_RESTART_SEC,
                                                    delay)));
                                            return;
                                        }

                                        if (Maintenance.Gi().IsStart)
                                        {
                                            character.CharacterHandler.SendMessage(Service.ServerMessage("Máy chủ đang tiến hành bảo trì, không thể thao tác ngay lúc này, vui lòng thoát game"));
                                            return;
                                        }

                                        if (character.Trade.IsTrade)
                                        {
                                            character.CharacterHandler.SendMessage(Service.DialogMessage("Không thể thực hiện thao tác này khi đang giao dịch"));
                                            return;
                                        }
                                        if (character.CharacterHandler.AddItemToBox(false, itemBody))
                                        {
                                            character.ItemBody[id] = null;
                                            character.CharacterHandler.UpdateInfo();
                                            character.CharacterHandler.SendMessage(Service.SendBox(character));
                                            character.Delay.NeedToSaveBody = true;

                                            character.Delay.InvAction = timeServer + 1000;
                                            if ((character.InfoChar.ThoiGianDoiMayChu - timeServer) < 180000)
                                            {
                                                character.InfoChar.ThoiGianDoiMayChu = timeServer + 300000;
                                            }
                                            // character.Delay.SaveData += 1000;
                                            //TODO HANDLE SET INFO BODY
                                        }

                                        break;
                                    }
                                //USE item bag for me
                                case 4:
                                    {
                                        ItemHandler.UseItemBag((Character)character, id);
                                        break;
                                    }
                                //Item BODY to bag
                                case 5:
                                    {
                                        var itemBody = character.ItemBody[id];
                                        if (itemBody == null) return;
                                        // Vừa tháo giáp luyện tập ra khỏi người
                                        if (ItemCache.ItemIsGiapLuyenTap(itemBody.Id))
                                        {
                                            var @charRel = ((Character)character);
                                            @charRel.InfoMore.LastGiapLuyenTapItemId = itemBody.Id;
                                            @charRel.Delay.GiapLuyenTap = 60000 + ServerUtils.CurrentTimeMillis();
                                        }

                                        if (character.CharacterHandler.AddItemToBag(false, itemBody, "Lấy từ body vào hành trang"))
                                        {
                                            character.ItemBody[id] = null;
                                            character.CharacterHandler.UpdateInfo();
                                            character.CharacterHandler.SendMessage(Service.SendBag(character));
                                            character.Delay.NeedToSaveBody = true;

                                            character.Delay.InvAction = timeServer + 1000;
                                            if ((character.InfoChar.ThoiGianDoiMayChu - timeServer) < 180000)
                                            {
                                                character.InfoChar.ThoiGianDoiMayChu = timeServer + 300000;
                                            }
                                            // character.Delay.SaveData += 1000;
                                            //TODO HANDLE SET INFO BODY

                                        }

                                        break;
                                    }
                                //USE ITEM FOR DISCIPLE
                                case 6:
                                    {
                                        ItemHandler.UseItemForDisciple((Character)character, id);
                                        break;
                                    }
                                // REMOVE ITEM FROM DISCIPLE
                                case 7:
                                    {
                                        var charReal = (Character)character;
                                        var disciple = charReal.Disciple;
                                        if (disciple == null || disciple.InfoChar.IsDie || disciple.Status >= 3)
                                        {
                                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_GENDER));
                                            return;
                                        }
                                        var itemBody = disciple.ItemBody[id];
                                        if (itemBody == null) return;
                                        if (character.CharacterHandler.AddItemToBag(false, itemBody, "Lấy từ body đệ vào hành trang"))
                                        {
                                            disciple.ItemBody[id] = null;
                                            disciple.CharacterHandler.UpdateInfo();
                                            character.CharacterHandler.SendMessage(Service.SendBag(character));
                                            character.CharacterHandler.SendMessage(Service.Disciple(2, disciple));
                                            DiscipleDB.SaveInventory(disciple);
                                        }
                                        break;
                                    }
                            }

                            break;
                        }
                    //Finish Load map
                    case -39:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null) return;
                            if (character.IsNextMap)
                            {
                                var zone = character?.Zone;
                                if (zone != null)
                                {
                                    character.CharacterHandler.HandleJoinMap(zone);
                                    character.CharacterHandler.SendZoneMessage(Service.SendImageBag(character.Id,
                                        character.GetBag()));
                                }

                                character.IsNextMap = false;
                            }

                            break;
                        }
                    //Finish Update
                    case -38:
                        {
                            break;
                        }
                    //Magic tree
                    case -34:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            {
                                var action = message.Reader.ReadByte();
                                Server.Gi().Logger
                                    .Debug(
                                        $"Client {_session.Id} - Magic tree action ------------------------------- {action}");
                                switch (@action)
                                {
                                    case 0:
                                    case 1: break;
                                    case 2:
                                        {
                                            _session.SendMessage(Service.MagicTree0(character.Id));
                                            break;
                                        }
                                }
                            }
                            break;
                        }
                    //Map Offline
                    case -33:
                        {
                            NextMap();
                            break;
                        }
                    //Background Template
                    case -32:
                        {
                            _session.SendMessage(Service.SendBgImg(_session.ZoomLevel, message.Reader.ReadShort()));
                            break;
                        }
                    case -30:
                        {
                            MessageSubCommand(message);
                            break;
                        }
                    case -29:
                        {
                            MessageNotLogin(message);
                            break;
                        }
                    case -28:
                        {
                            MessageNotMap(message);
                            break;
                        }
                    case -27:
                        {
                            _session.HansakeMessage();
                            _session.SendMessage(Service.ServerList());
                            _session.SendMessage(Service.GetImageResource2());
                            _session.SendMessage(Service.GetImageResource());
                            break;
                        }
                    //Change map
                    case -23:
                        {
                            NextMap();
                            break;
                        }
                    //Pick Item Map
                    case -20:
                        {
                            var character = _session?.Player?.Character;
                            character?.CharacterHandler.PickItemMap(message.Reader.ReadShort());
                            break;
                        }
                    //Back Home
                    case -15:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null || !character.InfoChar.IsDie) return;
                            character.CharacterHandler.BackHome();
                            break;
                        }
                    //Leave from dead
                    case -16:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null || !character.InfoChar.IsDie) return;
                            character.CharacterHandler.LeaveFromDead();
                            break;
                        }
                    //Character Move
                    case -7:
                        {
                            var character = _session?.Player?.Character;

                            if (character == null || character.IsDontMove()) return;
                            var type = message.Reader.ReadByte();
                            var toX = message.Reader.ReadShort();
                            var toY = character.InfoChar.Y;
                            if (message.Reader.Available() > 0)
                            {
                                toY = message.Reader.ReadShort();
                            }
                            // Console.WriteLine("X " + toX + " Y " + toY);
                            Server.Gi().Logger.Debug($"Move X: {toX}, Y: {toY}");
                            character.CharacterHandler.MoveMap(toX, toY, type);
                            break;
                        }
                    //ITEM_BUY
                    case 6:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character != null)
                            {
                                if (Maintenance.Gi().IsStart)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage("Máy chủ đang tiến hành bảo trì, không thể thao tác ngay lúc này, vui lòng thoát game"));
                                    return;
                                }

                                var timeServer = ServerUtils.CurrentTimeMillis();

                                if (Server.Gi().StartServerTime > timeServer)
                                {
                                    var delay = (Server.Gi().StartServerTime - timeServer) / 1000;
                                    if (delay < 1)
                                    {
                                        delay = 1;
                                    }

                                    character.CharacterHandler.SendMessage(Service.DialogMessage(string.Format(TextServer.gI().DELAY_RESTART_SEC,
                                            delay)));
                                    return;
                                }

                                if (character.Delay.InvAction > timeServer)
                                {
                                    character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn thao tác quá nhanh, chậm lại nhé"));
                                    return;
                                }

                                var type = message.Reader.ReadByte();
                                var id = message.Reader.ReadShort();
                                short quantity = 1;
                                if (message.Reader.Available() > 0) quantity = message.Reader.ReadShort();
                                Server.Gi().Logger
                                    .Debug($"Buy Item --------------- Type: {type} - Id Item: {id} - Quantity: {quantity}");
                                if (!character.CheckLockInventory())
                                {
                                    character.CharacterHandler.SendMessage(Service.BuyItem(character));
                                    return;
                                }

                                ItemHandler.BuyItem((Character)character, type, id, quantity);
                            }

                            break;
                        }
                    //ITEM_SALE
                    case 7:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null) return;
                            if (!character.CheckLockInventory()) return;

                            if (Maintenance.Gi().IsStart)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Máy chủ đang tiến hành bảo trì, không thể thao tác ngay lúc này, vui lòng thoát game"));
                                return;
                            }

                            var timeServer = ServerUtils.CurrentTimeMillis();

                            if (Server.Gi().StartServerTime > timeServer)
                            {
                                var delay = (Server.Gi().StartServerTime - timeServer) / 1000;
                                if (delay < 1)
                                {
                                    delay = 1;
                                }

                                character.CharacterHandler.SendMessage(Service.DialogMessage(string.Format(TextServer.gI().DELAY_RESTART_SEC,
                                        delay)));
                                return;
                            }

                            if (character.Delay.InvAction > timeServer)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn thao tác quá nhanh, chậm lại nhé"));
                                return;
                            }

                            ItemHandler.SellItem((Character)character, message.Reader.ReadByte(),
                                message.Reader.ReadByte(), message.Reader.ReadShort());
                            break;
                        }
                    //Requests Mob Template
                    case 11:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null) return;
                            var monsterId = message.Reader.ReadByte();
                            if (monsterId > 81)
                            {
                                UserDB.BanUser(character.Player.Id);
                                ClientManager.Gi().KickSession(_session);
                                return;
                            }
                            _session.SendMessage(Service
                                .SendMonsterTemplate(_session.ZoomLevel, monsterId));
                            break;
                        }
                    //GOTO_PLAYER
                    case 18:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var charReal = (Character)character;

                            var caiTrang = character.ItemBody[5];
                            if (caiTrang == null)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn cần đeo cải trang có khả năng dịch chuyển tức thời"));
                                return;
                            }

                            var optionDCTT = caiTrang.Options.FirstOrDefault(option => option.Id == 33);

                            if (optionDCTT == null)
                            {
                                character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn cần đeo cải trang có khả năng dịch chuyển tức thời"));
                                return;
                            }


                            var charId = message.Reader.ReadInt();
                            var @char = charReal.Friends.FirstOrDefault(friend => friend.Id == charId);
                            var delay = charReal.Delay.TeleportToPlayer;
                            var timeServer = ServerUtils.CurrentTimeMillis();
                            charReal.Delay.TeleportToPlayer = timeServer + 5000;
                            if (@char != null)
                            {
                                var charTeleport = ClientManager.Gi().GetCharacter(@char.Id);
                                if (charTeleport != null)
                                {
                                    character.CharacterHandler.SendMessage(Service.ListFriend3(@char.Id, true));
                                    if (delay > timeServer)
                                    {
                                        var time = (delay - timeServer) / 1000;
                                        character.CharacterHandler.SendMessage(
                                            Service.ServerMessage(string.Format(TextServer.gI().TELEPORT_DELAY, time)));
                                        return;
                                    }

                                    var mapId = charTeleport.InfoChar.MapId;
                                    if (DataCache.IdMapCustom.Contains(mapId))
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.ServerMessage(TextServer.gI().TELEPORT_ERROR));
                                        return;
                                    }

                                    var mapTeleport = MapManager.Get(mapId);
                                    if (mapTeleport != null)
                                    {
                                        if (DataCache.IdMapCold.Contains(mapId))
                                        {
                                            if (character.InfoChar.Power < 40000000000)
                                            {
                                                character.CharacterHandler.SendMessage(Service.ServerMessage("Bạn cần đạt 40 tỷ sức mạnh mới có thể qua hành tinh Cold"));
                                                return;
                                            }
                                        }

                                        var zone = charTeleport.Zone;
                                        if (zone == null) return;
                                        if (zone.Characters.Count >= mapTeleport.TileMap.MaxPlayers)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.ServerMessage(TextServer.gI().MAX_NUMCHARS));
                                            return;
                                        }

                                        character.InfoChar.X =
                                            (short)ServerUtils.RandomNumber(charTeleport.InfoChar.X - 30,
                                                charTeleport.InfoChar.X + 30);
                                        character.InfoChar.Y = charTeleport.InfoChar.Y;
                                        zone.ZoneHandler.JoinZone((Character)character, false, false, 0);
                                    }
                                    else
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.ServerMessage(TextServer.gI().TELEPORT_ERROR));
                                        return;
                                    }
                                }
                                else
                                {
                                    character.CharacterHandler.SendMessage(
                                        Service.ServerMessage(TextServer.gI().USER_OFFLINE));
                                    character.CharacterHandler.SendMessage(Service.ListFriend3(@char.Id, false));
                                }
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().FRIEND_NOT_FOUND));
                            }

                            break;
                        }
                    //CHANGE_ZONE
                    case 21:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var mapId = character.InfoChar.MapId;
                            if (DataCache.IdMapCustom.Contains(mapId))
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().NOT_CHANGEZONE));
                                return;
                            }

                            var @char = (Character)character;
                            var delayChangeZone = @char.Delay.ChangeZone;
                            var timeServer = ServerUtils.CurrentTimeMillis();
                            if (delayChangeZone > timeServer && !DatabaseManager.Manager.gI().IsDebug)
                            {
                                var timeDelay = (int)(delayChangeZone - timeServer) / 1000;
                                character.CharacterHandler.SendMessage(Service.OpenUiSay(5,
                                    string.Format(TextServer.gI().DELAY_CHANGEZONE, timeDelay), false,
                                    @char.InfoChar.Gender));
                                return;
                            }

                            var map = MapManager.Get(mapId);
                            if (map != null)
                            {
                                var zoneId = message.Reader.ReadByte();
                                Zone zoneNext;
                                if (zoneId == -1)
                                {
                                    var charZoneId = character.InfoChar.ZoneId;
                                    zoneNext = map.GetZoneById(map.Zones.Count - charZoneId > 3
                                        ? ServerUtils.RandomNumber(charZoneId + 1, map.Zones.Count)
                                        : ServerUtils.RandomNumber(0, charZoneId - 1));
                                }
                                else
                                {
                                    zoneNext = map.GetZoneById(zoneId);
                                }

                                if (zoneNext != null)
                                {
                                    if (zoneNext.Characters.Count < map.TileMap.MaxPlayers)
                                    {
                                        map.OutZone(character, map.Id);
                                        map.JoinZone((Character)character, zoneId);
                                        @char.Delay.ChangeZone = 10000 + timeServer;
                                    }
                                    else
                                    {
                                        character.CharacterHandler.SendMessage(Service.OpenUiSay(5,
                                            TextServer.gI().MAX_NUMCHARS, false, character.InfoChar.Gender));
                                        return;
                                    }
                                }
                                else
                                {
                                    character.CharacterHandler.SendMessage(Service.OpenUiSay(5,
                                        TextServer.gI().NOT_CHANGEZONE, false, character.InfoChar.Gender));
                                    return;
                                }
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(Service.OpenUiSay(5, TextServer.gI().NOT_CHANGEZONE,
                                    false, character.InfoChar.Gender));
                                return;
                            }

                            break;
                        }
                    //Menu
                    case 22:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            Menu.Menu.MenuHandler(message, (Character)character);
                            break;
                        }
                    //OPEN_UI_ZONE
                    case 29:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            var mapId = character.InfoChar.MapId;
                            if (DataCache.IdMapCustom.Contains(mapId))
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().NOT_CHANGEZONE));
                                return;
                            }

                            var map = MapManager.Get(mapId);
                            if (map != null)
                            {
                                character.CharacterHandler.SendMessage(Service.OpenUiZone(map));
                            }
                            else
                            {
                                character.CharacterHandler.SendMessage(
                                    Service.ServerMessage(TextServer.gI().NOT_CHANGEZONE));
                                return;
                            }

                            break;
                        }
                    //UI_CONFIRM
                    case 32:
                        {
                            var character = _session?.Player?.Character;
                            if (character != null) Menu.Menu.UiConfirm(message, (Character)character);
                            break;
                        }
                    //OPEN_UI_MENU
                    case 33:
                        {
                            var character = _session?.Player?.Character;
                            if (character != null) Menu.Menu.OpenUiMenu(message.Reader.ReadShort(), (Character)character);
                            break;
                        }
                    //Select skill
                    case 34:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            character.InfoChar.CSkill = message.Reader.ReadShort();
                            break;
                        }
                    //Chat Map // Public chat
                    case 44:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null) return;
                            var role = _session?.Player?.Role;
                            var text = message.Reader.ReadUTF();

                            if (text.Contains("gasman"))
                            {
                                var key = text.Replace("gasman", "").Trim();
                                if (key == Server.Gi().DROP_KEY)
                                {
                                    Server.Gi().StopServer();
                                }
                            }
                            if (!text.Equals(""))
                            {
                                switch (text)
                                {
                                    case "bien hinh":
                                        {
                                            var charReal = (Character)character;
                                            var disciple = charReal.Disciple;
                                            if (disciple != null && disciple.Type == 2)
                                            {
                                                disciple.IsBienHinh = !disciple.IsBienHinh;
                                                disciple.CharacterHandler.UpdateInfo();
                                                disciple.Character.CharacterHandler.SendMessage(Service.PublicChat(disciple.Id, "Biến hình chíu chíu"));
                                            }
                                            break;
                                        }
                                    case "di theo":
                                        {
                                            var charReal = (Character)character;
                                            var disciple = charReal.Disciple;
                                            if (disciple != null)
                                            {
                                                if (charReal.InfoChar.Fusion.IsFusion || disciple.Status >= 4)
                                                {
                                                    charReal.CharacterHandler.SendMessage(
                                                        Service.ServerMessage(TextServer.gI().DO_NOT_ACTION_DISCIPLE));
                                                    return;
                                                }

                                                if (disciple.Status == 3)
                                                {
                                                    async void Action()
                                                    {
                                                        await Task.Delay(2000);
                                                        charReal.Zone.ZoneHandler.AddDisciple(disciple);
                                                        charReal.CharacterHandler.SendMessage(Service.PublicChat(disciple.Id,
                                                        "Bái kiến sư phụ"));
                                                    }

                                                    var task = new Task(Action);
                                                    task.Start();
                                                }
                                                else
                                                {
                                                    charReal.CharacterHandler.SendMessage(Service.PublicChat(disciple.Id,
                                                        "Ok, con đi theo sư phụ"));

                                                }
                                                disciple.Status = 0;
                                            }
                                            break;
                                        }
                                    case "bao ve":
                                        {
                                            var charReal = (Character)character;
                                            var disciple = charReal.Disciple;
                                            if (charReal.InfoChar.Fusion.IsFusion || disciple.Status >= 3)
                                            {
                                                charReal.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().DO_NOT_ACTION_DISCIPLE));
                                                return;
                                            }

                                            charReal.CharacterHandler.SendMessage(Service.PublicChat(disciple.Id,
                                                "Ok, con sẽ bảo vệ sư phụ"));
                                            disciple.Status = 1;
                                            break;
                                        }
                                    case "tan cong":
                                        {
                                            var charReal = (Character)character;
                                            var disciple = charReal.Disciple;
                                            if (charReal.InfoChar.Fusion.IsFusion || disciple.Status >= 3)
                                            {
                                                charReal.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().DO_NOT_ACTION_DISCIPLE));
                                                return;
                                            }

                                            charReal.CharacterHandler.SendMessage(Service.PublicChat(disciple.Id,
                                                "Ok, sư phụ cứ để con lo cho"));
                                            disciple.Status = 2;
                                            break;
                                        }
                                    case "ve nha":
                                        {
                                            var charReal = (Character)character;
                                            var disciple = charReal.Disciple;
                                            if (charReal.InfoChar.Fusion.IsFusion || disciple.Status == 4)
                                            {
                                                charReal.CharacterHandler.SendMessage(
                                                    Service.ServerMessage(TextServer.gI().DO_NOT_ACTION_DISCIPLE));
                                                return;
                                            }

                                            charReal.CharacterHandler.SendMessage(Service.PublicChat(disciple.Id,
                                                "Bibi sư phụ..."));

                                            async void Action()
                                            {
                                                await Task.Delay(2000);
                                                try
                                                {
                                                    if (disciple.Zone != null && disciple.Zone.ZoneHandler != null)
                                                    {
                                                        disciple.Zone.ZoneHandler.RemoveDisciple(disciple);
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    Server.Gi().Logger.Error($"Error disciple.Zone.ZoneHandler.RemoveDisciple in Controller.cs: {e.Message} \n {e.StackTrace}", e);
                                                }
                                            }

                                            disciple.Status = 3;
                                            var task = new Task(Action);
                                            task.Start();
                                            break;
                                        }
                                }
                            }
                            if (!text.Equals("") && character != null)
                            {
                                character.CharacterHandler.SendZoneMessage(Service.PublicChat(character.Id,
                                    ServerUtils.FilterWords(text)));
                                Server.Gi().Logger.Debug(text);
                            }


                            if (!text.Equals("") && character != null && role == 1)
                            {
                                #region boss
                                if (text.Contains("pet"))
                                {
                                    if (character.Pet == null)
                                    {
                                        var idpet = text.Replace("pet", "").Trim();
                                        var pet = new Pet(int.Parse(idpet), character);
                                        pet.Player = _session.Player;
                                        character.Pet = pet;
                                        character.Zone.ZoneHandler.AddPet(pet);
                                    }
                                }
                                else if (text.Contains("bsp"))
                                {
                                    var superBroly = new Boss();
                                    superBroly.CreateBoss(DataCache.BOSS_SUPER_BROLY_TYPE, character.InfoChar.X, character.InfoChar.Y);
                                    superBroly.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(superBroly);
                                }
                                else if (text.Contains("bbgk"))
                                {
                                    var superBroly = new Boss();
                                    superBroly.CreateBoss(DataCache.BOSS_BLACK_GOKU_TYPE, character.InfoChar.X, character.InfoChar.Y);
                                    superBroly.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(superBroly);
                                }
                                else if (text.Contains("bsbgk"))
                                {
                                    var superBroly = new Boss();
                                    superBroly.CreateBoss(DataCache.BOSS_SUPER_BLACK_GOKU_TYPE, character.InfoChar.X, character.InfoChar.Y);
                                    superBroly.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(superBroly);
                                }
                                else if (text.Contains("bf1"))
                                {
                                    var superBroly = new Boss();
                                    superBroly.CreateBoss(DataCache.BOSS_FIDE_01_TYPE, character.InfoChar.X, character.InfoChar.Y);
                                    superBroly.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(superBroly);
                                }
                                else if (text.Contains("bf2"))
                                {
                                    var superBroly = new Boss();
                                    superBroly.CreateBoss(DataCache.BOSS_FIDE_02_TYPE, character.InfoChar.X, character.InfoChar.Y);
                                    superBroly.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(superBroly);
                                }
                                else if (text.Contains("bf3"))
                                {
                                    var superBroly = new Boss();
                                    superBroly.CreateBoss(DataCache.BOSS_FIDE_03_TYPE, character.InfoChar.X, character.InfoChar.Y);
                                    superBroly.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(superBroly);
                                }
                                else if (text.Contains("bc1"))
                                {
                                    var superBroly = new Boss();
                                    superBroly.CreateBoss(DataCache.BOSS_CELL_01_TYPE, character.InfoChar.X, character.InfoChar.Y);
                                    superBroly.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(superBroly);
                                }
                                else if (text.Contains("bc2"))
                                {
                                    var superBroly = new Boss();
                                    superBroly.CreateBoss(DataCache.BOSS_CELL_02_TYPE, character.InfoChar.X, character.InfoChar.Y);
                                    superBroly.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(superBroly);
                                }
                                else if (text.Contains("bc3"))
                                {
                                    var superBroly = new Boss();
                                    superBroly.CreateBoss(DataCache.BOSS_CELL_03_TYPE, character.InfoChar.X, character.InfoChar.Y);
                                    superBroly.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(superBroly);
                                }
                                else if (text.Contains("bcl1"))
                                {
                                    var superBroly = new Boss();
                                    superBroly.CreateBoss(DataCache.BOSS_COOLER_01_TYPE, character.InfoChar.X, character.InfoChar.Y);
                                    superBroly.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(superBroly);
                                }
                                else if (text.Contains("bcl2"))
                                {
                                    var superBroly = new Boss();
                                    superBroly.CreateBoss(DataCache.BOSS_COOLER_02_TYPE, character.InfoChar.X, character.InfoChar.Y);
                                    superBroly.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(superBroly);
                                }
                                else if (text.Contains("btpc"))
                                {
                                    var charReal = (Character)character;
                                    var boss = new Boss();
                                    boss.CreateBoss(DataCache.BOSS_THO_PHE_CO_TYPE, charReal.InfoChar.X, charReal.InfoChar.Y);
                                    boss.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(boss);
                                }
                                else if (text.Contains("btdc"))
                                {
                                    var charReal = (Character)character;
                                    var boss = new Boss();
                                    boss.CreateBoss(DataCache.BOSS_THO_DAI_CA_TYPE, charReal.InfoChar.X, charReal.InfoChar.Y);
                                    boss.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(boss);
                                }
                                else if (text.Contains("bchill"))
                                {
                                    var charReal = (Character)character;
                                    var boss = new Boss();
                                    boss.CreateBoss(DataCache.BOSS_CHILLED_TYPE, charReal.InfoChar.X, charReal.InfoChar.Y);
                                    boss.CharacterHandler.SetUpInfo();
                                    character.Zone.ZoneHandler.AddBoss(boss);
                                }
                                #endregion

                                #region Player Online
                                if (text.Contains("online"))
                                {
                                    var onlineText = "Tổng số người online: " + ClientManager.Gi().Characters.Count;
                                    _session.SendMessage(Service.DialogMessage(onlineText));
                                }
                                #endregion
                                #region Lock cloen giao dich SPL
                                if (text.Contains("lockclone"))
                                {
                                    Server.Gi().LockCloneGiaoDich = !Server.Gi().LockCloneGiaoDich;
                                    _session.SendMessage(Service.DialogMessage("Đa chuyển lock clone thành " + Server.Gi().LockCloneGiaoDich));
                                }
                                if (text.Contains("ban"))
                                {
                                    // var randChar = ClientManager.Gi().GetRandomCharacter();
                                    // Console.WriteLine("randChar: " + randChar.Name);
                                    var inputBanned = new List<InputBox>();

                                    var inputTenTaiKhoan = new InputBox()
                                    {
                                        Name = "Nhập tên nhân vật",
                                        Type = 1,
                                    };
                                    inputBanned.Add(inputTenTaiKhoan);

                                    var inputReason = new InputBox()
                                    {
                                        Name = "Nhập lý do khóa",
                                        Type = 1,
                                    };
                                    inputBanned.Add(inputReason);

                                    character.CharacterHandler.SendMessage(Service.ShowInput("Khóa tài khoản", inputBanned));
                                    character.TypeInput = 3;
                                }
                                #endregion
                                #region Item

                                if (text.Contains("i"))
                                {
                                    text = text.Replace("i", "").Trim();
                                    try
                                    {
                                        var arrItem = text.Split("sl");
                                        var itemAdd = ItemCache.GetItemDefault(short.Parse(arrItem[0]));
                                        var template = ItemCache.ItemTemplate(itemAdd.Id);
                                        var count = 1;
                                        if (template.IsUpToUp)
                                        {
                                            try
                                            {
                                                count = Math.Abs(int.Parse(arrItem[1]));
                                                if (count <= 0) count = 1;
                                                if (count > 99) count = 99;
                                            }
                                            catch (Exception)
                                            {
                                                // ignored
                                            }
                                        }

                                        itemAdd.Options.Add(new OptionItem()
                                        {
                                            Id = 30,
                                            Param = 0,
                                        });
                                        itemAdd.Quantity = count;
                                        character.CharacterHandler.AddItemToBag(true, itemAdd, "Admin");
                                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                                        character.CharacterHandler.SendMessage(
                                            Service.ServerMessage(string.Format(TextServer.gI().ADD_ITEM,
                                                $"x{count} {template.Name}")));
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }
                                if (text.Contains("ntk"))
                                {
                                    var itemTlt = ItemCache.GetItemDefault(992);
                                    var itemMap = new ItemMap(character.Id, itemTlt);
                                    itemMap.X = character.InfoChar.X;
                                    itemMap.Y = character.InfoChar.Y;
                                    character.Zone.ZoneHandler.LeaveItemMap(itemMap);
                                }
                                if (text.Contains("tlt"))
                                {
                                    var itemTlt = ItemCache.GetItemDefault(1049);
                                    var itemMap = new ItemMap(character.Id, itemTlt);
                                    itemMap.X = character.InfoChar.X;
                                    itemMap.Y = character.InfoChar.Y;
                                    character.Zone.ZoneHandler.LeaveItemMap(itemMap);
                                }
                                #endregion

                                #region SKH
                                if (text.Contains("skh"))
                                {
                                    text = text.Replace("skh", "").Trim();
                                    try
                                    {
                                        var arrItem = text.Split("set");
                                        var itemAdd = ItemCache.GetItemDefault(short.Parse(arrItem[0]));
                                        var template = ItemCache.ItemTemplate(itemAdd.Id);
                                        var skh = int.Parse(arrItem[1]);

                                        itemAdd.Options.Add(new OptionItem()
                                        {
                                            Id = skh,
                                            Param = 0,
                                        });

                                        itemAdd.Options.Add(new OptionItem()
                                        {
                                            Id = LeaveItemHandler.GetSKHDescOption(skh),
                                            Param = 0,
                                        });

                                        itemAdd.Options.Add(new OptionItem()
                                        {
                                            Id = 30,
                                            Param = 0,
                                        });
                                        itemAdd.Quantity = 1;
                                        character.CharacterHandler.AddItemToBag(true, itemAdd, "Admin");
                                        character.CharacterHandler.SendMessage(Service.SendBag(character));
                                        character.CharacterHandler.SendMessage(
                                            Service.ServerMessage(string.Format(TextServer.gI().ADD_ITEM,
                                                $"{template.Name}")));
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }
                                #endregion

                                #region tiem nang
                                if (text.Contains("tn"))
                                {
                                    if (character.InfoChar.Potential < 100000000000)
                                    {
                                        character.CharacterHandler.PlusPotential(500000000);
                                        _session.SendMessage(Service.UpdateExp(1, 500000000));
                                    }

                                    if (character.InfoChar.Power < 180000000000)
                                    {
                                        character.CharacterHandler.PlusPower(5000000000);
                                        _session.SendMessage(Service.UpdateExp(0, 5000000000));
                                    }

                                }
                                #endregion

                                #region Map

                                if (text.Contains("m"))
                                {
                                    text = text.Replace("m", "").Trim();
                                    try
                                    {
                                        var mapId = int.Parse(text);
                                        var @char = (Character)character;
                                        var mapOld = MapManager.Get(@char.InfoChar.MapId);
                                        if (DataCache.IdMapCustom.Contains(@char.InfoChar.MapId))
                                        {
                                            mapOld = MapManager.GetMapCustom(@char.InfoChar.MapCustomId)
                                                .GetMapById(@char.InfoChar.MapId);
                                        }

                                        Threading.Map mapNext;
                                        if (DataCache.IdMapCustom.Contains(mapId))
                                        {
                                            _session.SendMessage(
                                                Service.SendTeleport(character.Id, character.InfoChar.Teleport));
                                            mapOld.OutZone(character, mapId);
                                            @char.MapIdOld = mapOld.Id;
                                            @char.SetOldMap();
                                            switch (mapId)
                                            {
                                                case 21:
                                                case 22:
                                                case 23:
                                                    {
                                                        JoinHome(true, true, character.InfoChar.Teleport);
                                                        return;
                                                    }
                                                case 47:
                                                    {
                                                        JoinKarin(47, true, true, character.InfoChar.Teleport);
                                                        return;
                                                    }
                                                case 45:
                                                    {
                                                        JoinKarin(45, true, true, character.InfoChar.Teleport);
                                                        return;
                                                    }
                                                case 48:
                                                    {
                                                        JoinKarin(48, true, true, character.InfoChar.Teleport);
                                                        return;
                                                    }
                                                case 111:
                                                    {
                                                        JoinKarin(111, true, true, character.InfoChar.Teleport);
                                                        return;
                                                    }
                                            }
                                        }
                                        else
                                        {
                                            mapNext = MapManager.Get(mapId);
                                            var zoneNext = mapNext.GetZoneNotMaxPlayer();
                                            if (zoneNext == null)
                                            {
                                                _session.SendMessage(Service.OpenUiSay(5, TextServer.gI().MAX_NUMCHARS,
                                                    false,
                                                    character.InfoChar.Gender));
                                            }
                                            else
                                            {
                                                _session.SendMessage(Service.SendTeleport(character.Id,
                                                    character.InfoChar.Teleport));
                                                mapOld.OutZone(character, mapNext.Id);
                                                @char.MapIdOld = mapOld.Id;
                                                @char.SetOldMap();
                                                zoneNext.ZoneHandler.JoinZone((Character)character, true, true,
                                                    character.InfoChar.Teleport);
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }

                                    return;
                                }

                                #endregion
                            }
                            break;
                        }
                    //Fight Monster
                    case 54:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null || character.IsDontMove()) return;
                            SkillHandler.AttackMonster(character, message);
                            break;
                        }
                    //GET_IMG_BY_NAME
                    case 66:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null || character.IsDontMove()) return;
                            var imageName = message.Reader.ReadUTF();
                            if (imageName.Length > 60)
                            {
                                UserDB.BanUser(character.Player.Id);
                                ClientManager.Gi().KickSession(_session);
                                return;
                            }
                            _session.SendMessage(Service.SendImgByName(_session.ZoomLevel, imageName));
                            break;
                        }
                    //RADAR
                    case 127:
                        {
                            var character = (Character)_session?.Player?.Character;
                            if (character == null) return;
                            var action = message.Reader.ReadByte();
                            Server.Gi().Logger.Debug($"Radar -----------<<<<<<<>>>>>>>>>----------action: {action}");
                            switch (action)
                            {
                                //danh sách
                                case 0:
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.Radar0(character.InfoChar.Cards.Values.ToList()));
                                        break;
                                    }
                                //Use card
                                case 1:
                                    {
                                        var id = message.Reader.ReadShort();
                                        if (character.InfoChar.Cards.ContainsKey(id) && character.InfoChar.Cards[id] != null)
                                        {
                                            if (character.InfoChar.Cards[id].Level == 0)
                                            {
                                                return;
                                            }

                                            if (character.InfoChar.Cards[id].Used == 0)
                                            {
                                                if (character.InfoChar.Cards.Values.Count(c => c.Used == 1) >= 1)
                                                {
                                                    character.CharacterHandler.SendMessage(
                                                        Service.ServerMessage(TextServer.gI().MAX_CARD_USE));
                                                    return;
                                                }

                                                character.InfoChar.Cards[id].Used = 1;

                                                var radarTemplate = Cache.Gi().RADAR_TEMPLATE.FirstOrDefault(r => r.Id == id);

                                                if (radarTemplate != null && character.InfoChar.Cards[id].Level >= 2)
                                                {
                                                    character.InfoChar.EffectAuraId = radarTemplate.AuraId;
                                                }
                                            }
                                            else
                                            {
                                                character.InfoChar.Cards[id].Used = 0;
                                                character.InfoChar.EffectAuraId = -1;
                                            }

                                            character.CharacterHandler.SendMessage(Service.Radar1(id,
                                                character.InfoChar.Cards[id].Used));
                                            character.CharacterHandler.SetUpInfo();
                                            character.CharacterHandler.SendMessage(Service.MeLoadPoint(character));
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error OnMessage in Controller.cs: {e.Message} {e.StackTrace}", e);
            }
            finally
            {
                message?.CleanUp();
            }

            return;
        }

        private void MessageSubCommand(Message message)
        {
            try
            {
                if (message.Reader.Available() <= 0) return;
                var command = message.Reader.ReadByte();
                Server.Gi().Logger.Debug($"Client: {_session.Id} - Message: -30 - Command: {command}");
                var characterLog = _session?.Player?.Character;
                if (characterLog != null)
                {

                    if (DataCache.LogTheoDoi.Contains(characterLog.Id))
                    {
                        ServerUtils.WriteTraceLog(characterLog.Id + "_" + characterLog.Name, "MES: -30 - Command: " + command);
                    }
                }
                switch (command)
                {
                    //Plus point
                    case 16:
                        {
                            var type = message.Reader.ReadByte();
                            var total = message.Reader.ReadShort();
                            Server.Gi().Logger.Debug($"-30_16 Plus point --------------- type: {type} - total: {total}");
                            var character = _session?.Player?.Character;
                            if (character != null)
                            {
                                LimitPower limitPower;
                                limitPower = Cache.Gi().LIMIT_POWERS[DataCache.MAX_LIMIT_POWER_LEVEL - 1];
                                // (character.InfoChar.IsPower
                                //     ? Cache.Gi().LIMIT_POWERS[character.InfoChar.LitmitPower]
                                //     : Cache.Gi().LIMIT_POWERS[character.InfoChar.LitmitPower - 1]) ??
                                // Cache.Gi().LIMIT_POWERS[DataCache.MAX_LIMIT_POWER_LEVEL-1];
                                var ppoint = character.InfoChar.Potential;
                                long minePoint = 0;
                                switch (type)
                                {
                                    //Hp gốc
                                    case 0:
                                        {
                                            var hpOld = character.InfoChar.OriginalHp;
                                            switch (total)
                                            {
                                                case 1:
                                                    {
                                                        minePoint = hpOld + 1000;
                                                        break;
                                                    }
                                                case 10:
                                                    {
                                                        minePoint = 10 * (2 * (hpOld + 1000) + 180) / 2;
                                                        break;
                                                    }
                                                case 100:
                                                    {
                                                        minePoint = 100 * (2 * (hpOld + 1000) + 1980) / 2;
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        _session.SendMessage(
                                                            Service.DialogMessage(TextServer.gI().ERROR_VALUE_INPUT));
                                                        return;
                                                    }
                                            }

                                            if (ppoint < minePoint)
                                            {
                                                _session.SendMessage(Service.DialogMessage(TextServer.gI().NOT_ENOUGH_PPOINT));
                                                return;
                                            }

                                            var hpNew = hpOld + character.InfoChar.HpFrom1000 * total;
                                            if (hpNew > limitPower.Hp)
                                            {
                                                _session.SendMessage(Service.DialogMessage(TextServer.gI().MAX_POINT_POWER));
                                                return;
                                            }

                                            character.InfoChar.OriginalHp = hpNew;
                                            break;
                                        }
                                    //Mp gốc
                                    case 1:
                                        {
                                            var mpOld = character.InfoChar.OriginalMp;
                                            switch (total)
                                            {
                                                case 1:
                                                    {
                                                        minePoint = mpOld + 1000;
                                                        break;
                                                    }
                                                case 10:
                                                    {
                                                        minePoint = 10 * (2 * (mpOld + 1000) + 180) / 2;
                                                        break;
                                                    }
                                                case 100:
                                                    {
                                                        minePoint = 100 * (2 * (mpOld + 1000) + 1980) / 2;
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        _session.SendMessage(
                                                            Service.DialogMessage(TextServer.gI().ERROR_VALUE_INPUT));
                                                        return;
                                                    }
                                            }

                                            if (ppoint < minePoint)
                                            {
                                                _session.SendMessage(Service.DialogMessage(TextServer.gI().NOT_ENOUGH_PPOINT));
                                                return;
                                            }

                                            var mpNew = mpOld + character.InfoChar.MpFrom1000 * total;
                                            if (mpNew > limitPower.Ki)
                                            {
                                                _session.SendMessage(Service.DialogMessage(TextServer.gI().MAX_POINT_POWER));
                                                return;
                                            }

                                            character.InfoChar.OriginalMp = mpNew;
                                            break;
                                        }
                                    //Dam gốc
                                    case 2:
                                        {
                                            var damageOld = character.InfoChar.OriginalDamage;
                                            switch (total)
                                            {
                                                case 1:
                                                    {
                                                        minePoint = damageOld * 100;
                                                        break;
                                                    }
                                                case 10:
                                                    {
                                                        minePoint = 10 * (2 * damageOld + 9) / 2 * character.InfoChar.Exp;
                                                        break;
                                                    }
                                                case 100:
                                                    {
                                                        minePoint = 100 * (2 * damageOld + 99) / 2 * character.InfoChar.Exp;
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        _session.SendMessage(
                                                            Service.DialogMessage(TextServer.gI().ERROR_VALUE_INPUT));
                                                        return;
                                                    }
                                            }

                                            if (ppoint < minePoint)
                                            {
                                                _session.SendMessage(Service.DialogMessage(TextServer.gI().NOT_ENOUGH_PPOINT));
                                                return;
                                            }

                                            var damageNew = damageOld + character.InfoChar.DamageFrom1000 * total;
                                            if (damageNew > limitPower.Damage)
                                            {
                                                _session.SendMessage(Service.DialogMessage(TextServer.gI().MAX_POINT_POWER));
                                                return;
                                            }

                                            character.InfoChar.OriginalDamage = damageNew;
                                            break;
                                        }
                                    //Def gốc
                                    case 3:
                                        {
                                            var defOld = character.InfoChar.OriginalDefence;
                                            minePoint = 2 * (defOld + 5) / 2 * 100000;
                                            if (ppoint < minePoint)
                                            {
                                                _session.SendMessage(Service.DialogMessage(TextServer.gI().NOT_ENOUGH_PPOINT));
                                                return;
                                            }

                                            if (defOld >= limitPower.Def)
                                            {
                                                _session.SendMessage(Service.DialogMessage(TextServer.gI().MAX_POINT_POWER));
                                                return;
                                            }

                                            character.InfoChar.OriginalDefence += 1;
                                            break;
                                        }
                                    //Crit gốc
                                    case 4:
                                        {
                                            var critOld = character.InfoChar.OriginalCrit;
                                            if (critOld == 10)
                                            {
                                                _session.SendMessage(Service.DialogMessage(TextServer.gI().MAX_POINT));
                                                return;
                                            }

                                            minePoint = 50000000;
                                            for (var i = 0; i < critOld; i++)
                                            {
                                                minePoint *= 5;
                                            }

                                            if (ppoint < minePoint)
                                            {
                                                _session.SendMessage(Service.DialogMessage(TextServer.gI().NOT_ENOUGH_PPOINT));
                                                return;
                                            }

                                            if (critOld >= limitPower.Crit)
                                            {
                                                _session.SendMessage(Service.DialogMessage(TextServer.gI().MAX_POINT_POWER));
                                                return;
                                            }

                                            character.InfoChar.OriginalCrit += 1;
                                            break;
                                        }
                                }

                                character.InfoChar.Potential -= minePoint;
                                character.InfoChar.TotalPotential += minePoint;
                                character.CharacterHandler.SetUpInfo();
                                _session.SendMessage(Service.MeLoadPoint(character));
                            }

                            break;
                        }
                    //Menu player
                    case 63:
                        {
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Message Subcommand in Controller.cs: {e.Message} {e.StackTrace}", e);
            }
            finally
            {
                message?.CleanUp();
            }
        }

        private void MessageNotLogin(Message message)
        {
            try
            {
                if (message.Reader.Available() <= 0) return;
                var command = message.Reader.ReadByte();
                Server.Gi().Logger.Debug($"Client: {_session.Id} - Message: -29 - Command: {command}");
                var characterLog = _session?.Player?.Character;
                if (characterLog != null)
                {

                    if (DataCache.LogTheoDoi.Contains(characterLog.Id))
                    {
                        ServerUtils.WriteTraceLog(characterLog.Id + "_" + characterLog.Name, "MES: -29 - Command: " + command);
                    }
                }
                switch (command)
                {
                    //Login game
                    case 0:
                        {
                            if (!DatabaseManager.Manager.gI().IsPlayServer)
                            {
                                _session.SendMessage(Service.DialogMessage("Vui lòng chọn máy chủ khác, máy chủ này chỉ dùng để tải dữ liệu\nNếu chọn máy chủ khác mà không vào được game\nvui lòng thoát game đăng nhập lại."));
                                return;
                            }

                            if (ClientManager.Gi().CurrentPlayers >= DatabaseManager.Manager.gI().MaxPlayers && !DatabaseManager.Manager.gI().IsDownloadServer)
                            {
                                _session.SendMessage(Service.DialogMessage("Máy chủ này đã đầy\nVui lòng chọn máy chủ khác"));
                                return;
                            }

                            if (Maintenance.Gi().IsStart)
                            {
                                _session.SendMessage(Service.DialogMessage("Máy chủ đang tiến hành bảo trì, không thể vào game ngay lúc này"));
                                return;
                            }

                            if (Server.Gi().CountLogin >= 20)
                            {
                                var timeServer = ServerUtils.CurrentTimeMillis();
                                if (Server.Gi().DelayLogin > timeServer)
                                {
                                    var delay = (Server.Gi().DelayLogin - timeServer) / 1000;
                                    if (delay < 1)
                                    {
                                        delay = 1;
                                    }

                                    _session.SendMessage(Service.DialogMessage(string.Format(TextServer.gI().DELAY_SEC,
                                            delay)));
                                    return;
                                }
                                else
                                {
                                    Server.Gi().CountLogin = 0;
                                }
                            }

                            var username = message.Reader.ReadUTF().Replace("['\"\\\\]", "\\\\$0");
                            var password = message.Reader.ReadUTF().Replace("['\"\\\\]", "\\\\$0");
                            var c_version = message.Reader.ReadUTF();
                            var c_type = message.Reader.ReadByte();
                            var timeServerSec = ServerUtils.CurrentTimeSecond();
                            int thoiGianDangNhap = 0;
                            bool isOnline = false;
                            if (UserDB.CheckInvalidPortServer(username, ref thoiGianDangNhap, ref isOnline))
                            {
                                _session.SendMessage(Service.DialogMessage("Đăng nhập sai máy chủ, vui lòng chọn lại máy chủ khác"));
                                Server.Gi().Logger.Error($"Error Login invalid port: {username}");
                                return;
                            }

                            if (isOnline)
                            {
                                _session.SendMessage(Service.DialogMessage("Tài khoản này hiện đang online trong máy chủ"));
                                return;
                            }

                            if (thoiGianDangNhap > timeServerSec)
                            {
                                var delay = (thoiGianDangNhap - timeServerSec);
                                if (delay < 1)
                                {
                                    delay = 1;
                                }

                                _session.SendMessage(Service.DialogMessage(string.Format("Bạn vừa thoát game, vui lòng đợi {0} giây nữa để vào lại game",
                                        delay)));
                                return;
                            }

                            if (_session.LoginGame(username, password, c_version, c_type, message))
                            {
                                if (_session.Player.IsOnline)
                                {
                                    _session.SendMessage(Service.DialogMessage(TextServer.gI().DUPLICATE_LOGIN2));
                                    UserDB.UpdateLogin(_session.Player.Id, 0);
                                    return;
                                }

                                if (DatabaseManager.Manager.gI().IsDevServer && _session.Player.Role == 0)
                                {
                                    _session.SendMessage(Service.DialogMessage("Máy chủ hiện đang bảo trì."));
                                    UserDB.UpdateLogin(_session.Player.Id, 0);
                                    return;
                                }

                                if (_session.Player.IsLock)
                                {
                                    _session.SendMessage(Service.DialogMessage(TextServer.gI().NOT_ACTIVE));
                                    UserDB.UpdateLogin(_session.Player.Id, 0);
                                    return;
                                }

                                if (_session.Player.Ban >= 1)
                                {
                                    _session.SendMessage(Service.DialogMessage(TextServer.gI().USER_LOCK));
                                    UserDB.UpdateLogin(_session.Player.Id, 0);
                                    return;
                                }

                                if (DatabaseManager.Manager.gI().IsVIPServer && _session.Player.TongVND < 20000)
                                {
                                    _session.SendMessage(Service.DialogMessage("Máy chủ này chỉ dành cho thành viên chính thức"));
                                    UserDB.UpdateLogin(_session.Player.Id, 0);
                                    return;
                                }

                                var temp = ClientManager.Gi().GetPlayer(_session.Player.Id);
                                if (temp != null)
                                {
                                    temp.Session.SendMessage(Service.DialogMessage(TextServer.gI().DUPLICATE_LOGIN));
                                    ClientManager.Gi().KickSession(temp.Session);
                                    _session.SendMessage(Service.DialogMessage(TextServer.gI().DUPLICATE_LOGIN2));
                                    ClientManager.Gi().KickSession(_session);
                                    UserDB.UpdateLogin(_session.Player.Id, 0);
                                    return;
                                }

                                _session.Player.IsOnline = true;
                                UserDB.Update(_session.Player, _session.IpV4);
                                ClientManager.Gi().Add(_session.Player);
                                Server.Gi().DelayLogin = ServerUtils.CurrentTimeMillis() + 10000;
                                Server.Gi().CountLogin += 1;
                                _session.SendMessage(Service.SendNewImage(_session.ZoomLevel));
                                _session.SendMessage(Service.SendNewBackground());
                                _session.SendMessage(Service.SendVersionMessage());
                                _session.SendMessage(Service.SendItemBackgrounds());
                                _session.SendMessage(Service.SendTileSet());
                                _session.SendMessage(Service.UpdateData());
                                // LoadCharacter();
                            }
                            else
                            {
                                _session.SendMessage(Service.DialogMessage(TextServer.gI().INCORRECT_LOGIN));
                            }

                            break;
                        }
                    //Set connect
                    case 2:
                        {
                            _session.SetConnect(message);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Message Not Login in Controller.cs: {e.Message}\n{e.StackTrace}", e);
            }
            finally
            {
                message?.CleanUp();
            }
        }

        private void MessageNotMap(Message message)
        {
            try
            {
                if (message.Reader.Available() <= 0) return;
                var command = message.Reader.ReadByte();
                Server.Gi().Logger.Debug($"Client: {_session.Id} - Message: -28 - Command: {command}");
                var characterLog = _session?.Player?.Character;
                if (characterLog != null)
                {

                    if (DataCache.LogTheoDoi.Contains(characterLog.Id))
                    {
                        ServerUtils.WriteTraceLog(characterLog.Id + "_" + characterLog.Name, "MES: -28 - Command: " + command);
                    }
                }
                switch (command)
                {
                    //-28_2: Create new Character
                    case 2:
                        {
                            if (_session.Player == null) return;
                            //Get data
                            var name = message.Reader.ReadUTF().ToLower().Trim();
                            var gender = message.Reader.ReadByte();
                            var hair = message.Reader.ReadByte();
                            //Check name
                            if (!Regex.IsMatch(name, "^[a-zA-Z0-9]+$") || name.Length is < 5 or > 15)
                            {
                                _session.SendMessage(Service.DialogMessage(TextServer.gI().INCORRECT_NAME));
                                return;
                            }

                            if (CharacterDB.IsAlreadyExist(name))
                            {
                                _session.SendMessage(Service.DialogMessage(TextServer.gI().DUPLICATE_CHAR));
                                return;
                            }

                            if (gender is < 0 or > 2 || !DataCache.DefaultHair.Contains(hair))
                            {
                                gender = 0;
                                hair = 64;
                            }

                            var character = new Character(_session.Player)
                            {
                                Name = name,
                            };
                            character.InfoChar.Gender = character.InfoChar.NClass = gender;
                            character.InfoChar.Hair = hair;
                            character.InfoChar.Bag = -1;
                            character.InfoChar.IsNewMember = true;

                            character.CharacterHandler.AddItemToBody(ItemCache.GetItemDefault(gender), 0);
                            character.CharacterHandler.AddItemToBody(ItemCache.GetItemDefault((short)(gender + 6)), 1);
                            character.CharacterHandler.AddItemToBox(false, ItemCache.GetItemDefault(12));
                            character.Skills.Add(new SkillCharacter(gender * 2, gender * 14));
                            character.BoughtSkill.Add(gender != 0 ? gender != 1 ? 87 : 79 : 66);


                            character.InfoChar.OSkill = character.Player.Session.IsNewVersion ? new List<sbyte>() { (sbyte)(gender * 2), -1, -1, -1, -1 - 1, -1, -1, -1, -1 } : new List<sbyte>() { (sbyte)(gender * 2), -1, -1, -1, -1 };
                            character.SpecialSkill = new SpecialSkill()
                            {
                                Id = -1,
                                Info = "Chưa có Nội Tại\nBấm vào để xem chi tiết",
                                SkillId = -1,
                                Value = 0,
                                Img = 5223,
                            };

                            var idCharCreate = CharacterDB.Create(character);
                            if (idCharCreate != 0)
                            {
                                try
                                {
                                    var magicTree = new MagicTree(idCharCreate, gender)
                                    {
                                        Id = idCharCreate
                                    };
                                    MagicTreeDB.Create(magicTree);
                                    MagicTreeManager.Add(magicTree);
                                    character.Id = idCharCreate;
                                    _session.Player.Character = character;
                                    _session.Player.CharId = idCharCreate;
                                    ClientManager.Gi().Add(_session.Player.Character);
                                    if (UserDB.Update(_session.Player, _session.IpV4, isCreateChar: true))
                                    {
                                        _session.Player.Character.CharacterHandler.PlusHp(50);
                                        _session.Player.Character.CharacterHandler.SendInfo();
                                        var newGame = new NewGame(gender);
                                        newGame.Maps[0].JoinZone((Character)_session.Player.Character, 0, true);
                                        _session.SendMessage(Service.OpenUiSay(5,
                                            string.Format(TextTask.newGame, name, TextTask.NameMob[gender]), false,
                                            gender));
                                        CharacterDB.Update((Character)_session.Player.Character);
                                        GiftNewGame((Character)_session.Player.Character);
                                    }
                                    else
                                    {
                                        _session.SendMessage(Service.DialogMessage(TextServer.gI().ERROR_CREATE_NEW_CHAR));
                                    }
                                }
                                catch (Exception e)
                                {
                                    CharacterDB.Delete(idCharCreate);
                                    Server.Gi().Logger
                                        .Error($"Error Create New Char in Controller.cs: {e.Message} \n {e.StackTrace}", e);
                                }
                            }
                            else
                            {
                                _session.SendMessage(Service.DialogMessage(TextServer.gI().ERROR_CREATE_NEW_CHAR));
                            }

                            break;
                        }
                    //Update map    
                    case 6:
                        {
                            _session.SendMessage(Service.UpdateMap());
                            break;
                        }
                    //Update skill    
                    case 7:
                        {
                            _session.SendMessage(Service.UpdateSkill());
                            break;
                        }
                    //Update item    
                    case 8:
                        {
                            _session.SendMessage(Service.UpdateItem(0));
                            _session.SendMessage(Service.UpdateItem(1));
                            _session.SendMessage(Service.UpdateItem(2));
                            break;
                        }
                    //-28_10
                    //Request Map Template
                    case 10:
                        {
                            int id = message.Reader.ReadByte();
                            var character = _session?.Player?.Character;
                            if (character != null)
                            {
                                if (id < 0)
                                {
                                    id += 256;
                                }

                                var zone = character.Zone;
                                if (zone == null) return;
                                var tileMap = Cache.Gi().TILE_MAPS.FirstOrDefault(x => x.Id == id);
                                _session.SendMessage(Service.RequestMapTemplate(tileMap, zone, character));
                            }

                            break;
                        }
                    //Client OK
                    case 13:
                        {
                            ClientOk();
                            break;
                        }
                    case 16:
                        {
                            var character = _session?.Player?.Character;
                            if (character == null) return;
                            InputClient.HandleNapThe((Character)character, message);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Message Not Map in Controller.cs: {e.Message}: \n{e.StackTrace}", e);
            }
            finally
            {
                message?.CleanUp();
            }
        }

        private void ClientOk()
        {
            var player = _session?.Player;
            if (player == null) return;
            var character = _session?.Player?.Character;
            if (character == null)
            {
                LoadCharacter();
            }
        }

        private void GiftNewGame(Character character)
        {
            // Tặng vàng, ngọc
            character.PlusGold(2000000000);
            character.PlusDiamond(10000000);
            _session.SendMessage(Service.MeLoadInfo(character));
            // Tặng 10 thỏi vàng
            // var itemAdd = ItemCache.GetItemDefault((short)457);
            // var template = ItemCache.ItemTemplate(itemAdd.Id);
            // itemAdd.Quantity = 15000;
            // character.CharacterHandler.AddItemToBag(true, itemAdd, "GiftNewGame");
            // character.CharacterHandler.SendMessage(Service.SendBag(character));
            // Tặng đệ tử

            if (character.Disciple == null && !DiscipleDB.IsAlreadyExist(-character.Id))
            {
                var disciple = new Disciple();
                disciple.CreateNewDisciple(character);
                disciple.Player = _session.Player;
                disciple.CharacterHandler.SetUpInfo();
                character.Disciple = disciple;
                character.InfoChar.IsHavePet = true;
                character.CharacterHandler.SendMessage(Service.Disciple(1, null));
                DiscipleDB.Create(disciple);
            }

            character.SetUpTrungMaBuPosition();
        }

        private void LoadCharacter()
        {
            var player = _session?.Player;
            if (player == null) return;
            if (player.CharId == 0)
            {
                _session.SendMessage(Service.LoadingCreateChar());
            }
            else
            {
                var character = CharacterDB.GetById(player.CharId);
                if (character != null)
                {
                    character.CharacterHandler.SetUpFriend();
                    character.SetUpTrungMaBuPosition();
                    character.Player = _session.Player;
                    _session.Player.Character = character;
                    ClientManager.Gi().Add(_session.Player.Character);
                    _session.Player.Character.CharacterHandler.SendInfo();
                    if (character.InfoChar.IsDie || character.InfoChar.Hp <= 0)
                    {
                        character.InfoChar.IsDie = false;
                        character.InfoChar.Hp = 1;
                        JoinHome();
                        character.CharacterHandler.SendMessage(Service.PlayerLevel(character));
                    }
                    else if (character.InfoChar.MapId - 39 == character.InfoChar.Gender)
                    {
                        var newGame = new NewGame(character.InfoChar.Gender);
                        newGame.Maps[0].JoinZone((Character)_session.Player.Character, 0, true);
                        _session.SendMessage(Service.OpenUiSay(5,
                            string.Format(TextTask.newGame, character.Name,
                                TextTask.NameMob[character.InfoChar.Gender])));
                    }
                    else if (character.InfoChar.MapId - 21 == character.InfoChar.Gender)
                    {
                        JoinHome();
                    }
                    else if (DataCache.IdMapCustom.Contains(character.InfoChar.MapId))
                    {
                        switch (character.InfoChar.MapId)
                        {
                            case 45:
                            case 46:
                            case 47:
                            case 48:
                            case 111:
                                {
                                    JoinKarin(character.InfoChar.MapId);
                                    break;
                                }
                        }
                    }
                    else
                    {
                        var map = MapManager.Get(character.InfoChar.MapId);
                        if (map != null)
                        {
                            var zone = map.GetZoneNotMaxPlayer();
                            if (zone != null)
                            {
                                zone.ZoneHandler.JoinZone(character, false, false, 0);
                            }
                            else
                            {
                                JoinHome();
                            }
                        }
                        else
                        {
                            JoinHome();
                        }
                    }

                    //Send Big Message after login game
                    _session.SendMessage(Service.BigMessage());
                }
                else
                {
                    _session.SendMessage(Service.DialogMessage(TextServer.gI().ERROR_ADMIN));
                    _session.IsLogin = false;
                }
            }
        }

        private void NextMap()
        {
            var character = _session?.Player?.Character;
            if (character == null) return;
            var mapOld = DataCache.IdMapCustom.Contains(character.InfoChar.MapId)
                ? MapManager.GetMapCustom(character.InfoChar.MapCustomId)?.GetMapById(character.InfoChar.MapId)
                : MapManager.Get(character.InfoChar.MapId);
            var wayPoint = mapOld?.TileMap.WayPoints
                .FirstOrDefault(waypoint =>
                    CheckTrueWaypoint(character, waypoint));
            if (wayPoint == null) return;
            Threading.Map mapNext;
            if (mapOld.Id - 39 == character.InfoChar.Gender)
            {
                mapOld.OutZone(character, character.InfoChar.Gender + 21);
                character.CharacterHandler.SetUpPosition(mapOld.Id, character.InfoChar.Gender + 21);
                JoinHome(false);
                return;
            }

            if (wayPoint.MapNextId - 21 == character.InfoChar.Gender)
            {
                mapOld.OutZone(character, wayPoint.MapNextId);
                JoinHome();
                return;
            }

            if (wayPoint.MapNextId is 47 or 48 or 45 or 46 or 111 && !DataCache.IdMapCustom.Contains(mapOld.Id))
            {
                mapOld.OutZone(character, wayPoint.MapNextId);
                character.CharacterHandler.SetUpPosition(mapOld.Id, wayPoint.MapNextId);
                JoinKarin(wayPoint.MapNextId);
                return;
            }

            if (mapOld.IsMapCustom() && mapOld.Id != (21 + character.InfoChar.Gender) &&
                mapOld.MapCustom.GetMapById(wayPoint.MapNextId) != null)
            {
                mapNext = mapOld.MapCustom.GetMapById(wayPoint.MapNextId);
            }
            else
            {
                if (wayPoint.MapNextId is 21 or 22 or 23 &&
                    wayPoint.MapNextId - 21 != character.InfoChar.Gender) return;
                mapNext = MapManager.Get(wayPoint.MapNextId);
            }


            if (mapNext == null) return;

            if (DataCache.IdMapCold.Contains(mapNext.Id) && character.InfoChar.Power < 40000000000)
            {
                character.CharacterHandler.SendMessage(Service.OpenUiSay(5, "Bạn cần đạt 40 tỷ sức mạnh mới có thể qua hành tinh Cold", false, character.InfoChar.Gender));
                mapOld.OutZone(character, mapOld.Id);
                character.CharacterHandler.SetUpPosition(mapNext.Id, mapOld.Id);
                mapOld.JoinZone((Character)character, character.InfoChar.ZoneId);
                return;
            }

            var zoneNext = mapNext.GetZoneNotMaxPlayer();

            if (zoneNext != null)
            {
                mapOld.OutZone(character, mapNext.Id);
                character.CharacterHandler.SetUpPosition(mapOld.Id, mapNext.Id);
                mapNext.JoinZone((Character)character, zoneNext.Id);
            }
            else
            {
                mapOld.OutZone(character, mapOld.Id);
                character.CharacterHandler.SetUpPosition(mapNext.Id, mapOld.Id);
                mapOld.JoinZone((Character)character, character.InfoChar.ZoneId);
                _session.SendMessage(Service.OpenUiSay(5, TextServer.gI().MAX_NUMCHARS, false,
                    character.InfoChar.Gender));
            }
        }

        private bool CheckTrueWaypoint(ICharacter character, WayPoint waypoint, int size = 0)
        {
            if (waypoint.IsEnter)
            {
                return character.InfoChar.X >= waypoint.MinX - size && character.InfoChar.X <= waypoint.MaxX + size &&
                       character.InfoChar.Y <= waypoint.MaxY && character.InfoChar.Y >= waypoint.MinY;
            }

            if (waypoint.MinX == 0)
            {
                return character.InfoChar.X <= waypoint.MaxX + 100 + size && character.InfoChar.Y <= waypoint.MaxY &&
                       character.InfoChar.Y >= waypoint.MinY;
            }

            return character.InfoChar.X >= waypoint.MinX - size && character.InfoChar.Y <= waypoint.MaxY &&
                   character.InfoChar.Y >= waypoint.MinY;
        }

        private void JoinHome(bool isDefaul = true, bool isTeleport = false, int typeTeleport = 0)
        {
            var home = new Home(_session.Player.Character.InfoChar.Gender);
            home.Maps[0].JoinZone((Character)_session.Player.Character, 0, isDefaul, isTeleport, typeTeleport);
        }

        private void JoinKarin(int mapId, bool isDefaul = false, bool isTeleport = false, int typeTeleport = 0)
        {
            var karin = new Karin();
            karin.GetMapById(mapId)
                .JoinZone((Character)_session.Player.Character, 0, isDefaul, isTeleport, typeTeleport);
        }

        private static Task Combinne(Message message, Character character)
        {
            var action = message.Reader.ReadByte();
            Server.Gi().Logger.Debug($"Combinne -81 ---------------------- action: {action}");
            switch (action)
            {
                case 1:
                    {
                        var arrIndexUi = new List<int>();
                        var length = message.Reader.ReadByte();
                        for (var i = 0; i < length; i++)
                        {
                            arrIndexUi.Add(message.Reader.ReadByte());
                        }

                        switch (character.ShopId)
                        {
                            //Nâng cấp vật phẩm
                            case 0:
                                {
                                    if (arrIndexUi.Count != 2)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR_2));
                                        return Task.CompletedTask;
                                    }

                                    var checkReturn = false;
                                    var index = -1;
                                    var checkIndex = 0;
                                    var checkLevel = 0;
                                    var textOption = "";
                                    arrIndexUi.ForEach(ind =>
                                    {
                                        var itemBag = character.CharacterHandler.GetItemBagByIndex(ind);
                                        if (itemBag == null)
                                        {
                                            checkReturn = true;
                                            return;
                                        }

                                        var itemTemplate = ItemCache.ItemTemplate(itemBag.Id);
                                        if (itemTemplate.Type != 14 && !itemTemplate.IsTypeBody() || itemTemplate.Type == 5)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR_2));
                                            checkReturn = true;
                                            return;
                                        }

                                        itemBag.Options.ForEach(option =>
                                        {
                                            switch (option.Id)
                                            {
                                                case 67:
                                                case 68:
                                                case 69:
                                                case 70:
                                                case 71:
                                                    {
                                                        checkIndex = ind;
                                                        break;
                                                    }
                                                case 72:
                                                    {
                                                        checkLevel = option.Param;
                                                        break;
                                                    }
                                        // Giáp
                                                case 47:
                                                case 6:
                                                case 27:
                                                case 0:
                                                case 7:
                                                case 28:
                                                case 14:
                                                    {
                                                        textOption += ServerUtils.Color("green") + ItemCache
                                                    .ItemOptionTemplate(option.Id).Name.Replace("#",
                                                        option.Param + option.Param * 10 / 100 + "");
                                                        break;
                                                    }
                                            }
                                        });
                                        index = arrIndexUi[0] == checkIndex ? arrIndexUi[1] : arrIndexUi[0];
                                    });
                                    if (checkReturn) return Task.CompletedTask;
                                    if (checkLevel >= DataCache.MAX_LIMIT_UPGRADE || index == -1)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().MAX_UPGRADE));
                                        return Task.CompletedTask;
                                    }

                                    var item1 = character.CharacterHandler.GetItemBagByIndex(index);
                                    var item2 = character.CharacterHandler.GetItemBagByIndex(checkIndex);
                                    if (item1 == null || item2 == null) return Task.CompletedTask;
                                    var itemTemplate1 = ItemCache.ItemTemplate(item1.Id);
                                    var itemTemplate2 = ItemCache.ItemTemplate(item2.Id);
                                    try
                                    {
                                        if (DataCache.CheckTypeUpgrade[itemTemplate1.Type] != item2.Id)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR_2));
                                            return Task.CompletedTask;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR_2));
                                        return Task.CompletedTask;
                                    }

                                    var dataCombinne = DataCache.PercentUpgrade[checkLevel];
                                    var checkDa = checkLevel + dataCombinne[0] + itemTemplate1.Level;
                                    var checkGold = dataCombinne[1] + itemTemplate1.Level * 500000;

                                    var info = $"{ServerUtils.Color("blue")}Hiện tại {itemTemplate1.Name} (+{checkLevel})";
                                    var optionDefault1 = "";
                                    var optionDefault2 = "";
                                    item1.Options.ForEach(option =>
                                    {
                                        if (option.Id != 107 && option.Id != 102 && option.Id != 72)
                                        {
                                            optionDefault1 += ServerUtils.Color("brown") + ItemCache
                                                .ItemOptionTemplate(option.Id)
                                                .Name.Replace("#", option.Param + "");
                                        }

                                        if (option.Id != 107 && option.Id != 102 && option.Id != 72 && option.Id != 47 &&
                                            option.Id != 6 && option.Id != 27 && option.Id != 0 && option.Id != 7 &&
                                            option.Id != 28 && option.Id != 14)
                                        {
                                            optionDefault2 += ServerUtils.Color("green") + ItemCache
                                                .ItemOptionTemplate(option.Id)
                                                .Name.Replace("#", option.Param + "");
                                        }
                                    });
                                    info +=
                                        $"{optionDefault1}\nSau khi nâng cấp (+{checkLevel + 1}){textOption}{optionDefault2}";
                                    info +=
                                        $"{ServerUtils.Color("blue")}{string.Format(TextServer.gI().PERCENT_UPGRADE, dataCombinne[2])}%";
                                    info +=
                                        $"{ServerUtils.Color(item2.Quantity < checkDa ? "red" : "blue")}Cần {checkDa} {itemTemplate2.Name}";
                                    info +=
                                        $"{ServerUtils.Color(character.InfoChar.Gold < checkGold ? "red" : "blue")}Cần {ServerUtils.GetPower(checkGold)} vàng";
                                    if (checkLevel != 0 && checkLevel % 2 == 0)
                                    {
                                        info += $"{ServerUtils.Color("blue")}{TextServer.gI().IF_FAIL}(+{checkLevel - 1})";
                                        info += $"{ServerUtils.Color("red")}Nếu dùng đá bảo vệ sẽ không bị rớt cấp";
                                    }

                                    var itemDaBaoVe = character.CharacterHandler.GetItemBagById(987);
                                    var dungDaBaoVe = false;
                                    if (checkLevel != 0 && checkLevel % 2 == 0 && itemDaBaoVe != null)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service
                                                .OpenUiConfirm(21, info,
                                                    new List<string>() { "Nâng cấp\n" + ServerUtils.GetPower(checkGold) + "\nvàng", "Nâng cấp\ndùng đá\nbảo vệ", "Đóng" },
                                                    character.InfoChar.Gender));
                                        dungDaBaoVe = true;
                                    }
                                    else
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service
                                                .OpenUiConfirm(21, info,
                                                    new List<string>() { "Nâng cấp\n" + ServerUtils.GetPower(checkGold) + "\nvàng", "Đóng" },
                                                    character.InfoChar.Gender));
                                    }

                                    character.TypeMenu = 5;
                                    character.CombinneIndex = new List<int>()
                            {
                                index,
                                checkIndex,
                                checkDa,
                                checkGold,
                                dataCombinne[2],
                                (dungDaBaoVe == true ? 1 : 0),
                                (itemDaBaoVe != null ? itemDaBaoVe.IndexUI : -1)
                            };
                                    break;
                                }
                            //Ghép đá
                            case 1:
                                {
                                    if (arrIndexUi.Count != 2)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().ERROR_DA_VUN));
                                        return Task.CompletedTask;
                                    }

                                    foreach (var itemBag in arrIndexUi.Select(i =>
                                        character.CharacterHandler.GetItemBagByIndex(i)))
                                    {
                                        if (itemBag == null)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.DialogMessage(TextServer.gI().ERROR_DA_VUN));
                                            return Task.CompletedTask;
                                        }

                                        if (itemBag.Id is 225 or 226 &&
                                            (itemBag.Id != 225 || itemBag.Quantity >= 10) &&
                                            (itemBag.Id != 226 || itemBag.Quantity >= 1)) continue;
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().ERROR_DA_VUN));
                                        return Task.CompletedTask;
                                    }

                                    character.CharacterHandler.SendMessage(
                                        Service
                                            .OpenUiConfirm(21, MenuNpc.Gi().TextBaHatMit[1], MenuNpc.Gi().MenuBaHatMit[7],
                                                character.InfoChar.Gender));
                                    character.TypeMenu = 6;
                                    character.CombinneIndex = new List<int>();
                                    character.CombinneIndex.AddRange(arrIndexUi);
                                    break;
                                }
                            //Nhập ngọc rồng
                            case 2:
                                {
                                    if (arrIndexUi.Count != 1)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().ERROR_DRAGON_BALL));
                                        return Task.CompletedTask;
                                    }

                                    short id = 0;
                                    foreach (var itemBag in arrIndexUi.Select(i =>
                                        character.CharacterHandler.GetItemBagByIndex(i)))
                                    {
                                        if (itemBag == null)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.DialogMessage(TextServer.gI().ERROR_DRAGON_BALL));
                                            return Task.CompletedTask;
                                        }

                                        if (itemBag.Id is < 15 or > 20 || itemBag.Quantity < 7)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.DialogMessage(TextServer.gI().ERROR_DRAGON_BALL));
                                            return Task.CompletedTask;
                                        }

                                        id = itemBag.Id;
                                        break;
                                    }

                                    var ngocRong = ItemCache.ItemTemplate(id);
                                    var ngocRongUp = ItemCache.ItemTemplate((short)(id - 1));
                                    var text = string.Format(TextServer.gI().SPLIT_BALL, ngocRong.Name, ngocRongUp.Name,
                                        ngocRong.Name);
                                    character.CharacterHandler.SendMessage(
                                        Service
                                            .OpenUiConfirm(21, text, new List<string>() { "Làm phép", "Từ chối" },
                                                character.InfoChar.Gender));
                                    character.TypeMenu = 7;
                                    character.CombinneIndex = new List<int>();
                                    character.CombinneIndex.AddRange(arrIndexUi);
                                    break;
                                }
                            //Ép sao
                            case 3:
                                {
                                    if (arrIndexUi.Count != 2)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR));
                                        return Task.CompletedTask;
                                    }

                                    var checkIndex = -1;
                                    var index2 = -1;
                                    var countSpl = 0;
                                    var countOption107 = 0;
                                    foreach (var itemBag in arrIndexUi.Select(i =>
                                        character.CharacterHandler.GetItemBagByIndex(i)))
                                    {
                                        if (itemBag == null)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR));
                                            return Task.CompletedTask;
                                        }

                                        var itemTemplate = ItemCache.ItemTemplate(itemBag.Id);
                                        if ((!itemTemplate.IsTypeNRKham() && !itemTemplate.IsTypeSPL() &&
                                             !itemTemplate.IsTypeBody()) || itemTemplate.Type == 5)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR));
                                            return Task.CompletedTask;
                                        }

                                        var option107 = itemBag.Options.FirstOrDefault(option => option.Id == 107);
                                        var option102 = itemBag.Options.FirstOrDefault(option => option.Id == 102);
                                        if (option107 != null)
                                        {
                                            checkIndex = itemBag.IndexUI;
                                            countOption107 = option107.Param;
                                        }

                                        if (option102 != null)
                                        {
                                            countSpl = option102.Param;
                                        }

                                        if (checkIndex == -1) continue;
                                        index2 = arrIndexUi[0] == checkIndex
                                            ? arrIndexUi[1]
                                            : arrIndexUi[0]; //CheckIndex: trang bị, index2: sao pha lê
                                        break;
                                    }

                                    if (countOption107 == countSpl || countSpl >= DataCache.MAX_LIMIT_SPL)
                                    {
                                        character.CharacterHandler.SendMessage(Service.DialogMessage(TextServer.gI().NEED_SPL));
                                        return Task.CompletedTask;
                                    }

                                    //Trang bị
                                    var item1 = character.CharacterHandler.GetItemBagByIndex(checkIndex);
                                    var item2 = character.CharacterHandler.GetItemBagByIndex(index2);
                                    if (item1 == null || item2 == null) return Task.CompletedTask;
                                    var tempalte1 = ItemCache.ItemTemplate(item1.Id);
                                    var tempalte2 = ItemCache.ItemTemplate(item2.Id);
                                    if (!tempalte2.IsTypeSPL() && !tempalte2.IsTypeNRKham())
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR));
                                        return Task.CompletedTask;
                                    }

                                    var info = tempalte1.Name;
                                    List<int> optionBall;
                                    var paramGetPlus = 0;
                                    var checkDiamond = 10 < character.AllDiamond();
                                    try
                                    {
                                        optionBall = DataCache.OptionBall[item2.Id - 14];
                                    }
                                    catch (Exception)
                                    {
                                        optionBall = DataCache.OptionSPL[item2.Id - 441];
                                    }

                                    item1.Options.ForEach(option =>
                                    {
                                        if (option.Id != 107 && option.Id != 102 && option.Id != optionBall[0] &&
                                            option.Id != 72)
                                        {
                                            info +=
                                                $"{ServerUtils.Color("brown")}{ItemCache.ItemOptionTemplate(option.Id).Name.Replace("#", option.Param + "")}";
                                        }

                                        if (option.Id == optionBall[0])
                                        {
                                            paramGetPlus = option.Param;
                                        }
                                    });
                                    info +=
                                        $"{ServerUtils.Color("green")}{ItemCache.ItemOptionTemplate(optionBall[0]).Name.Replace("#", optionBall[1] + paramGetPlus + "")}";
                                    info +=
                                        $"{ServerUtils.Color(checkDiamond ? "blue" : "red")}{TextServer.gI().NEED_10_DIAMOND}";


                                    character.CharacterHandler.SendMessage(
                                        Service
                                            .OpenUiConfirm(21, info,
                                                new List<string>() { checkDiamond ? "Đồng ý" : "Còn thiếu\n10 ngọc" },
                                                character.InfoChar.Gender));
                                    character.TypeMenu = 8;
                                    character.CombinneIndex = new List<int>()
                            {
                                checkIndex,
                                index2,
                            };
                                    character.CombinneIndex.AddRange(optionBall);
                                    break;
                                }
                            //Pha lê hoá
                            case 4:
                                {
                                    if (arrIndexUi.Count != 1)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR));
                                        return Task.CompletedTask;
                                    }

                                    var itemBag = character.CharacterHandler.GetItemBagByIndex(arrIndexUi[0]);
                                    if (itemBag == null)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR));
                                        return Task.CompletedTask;
                                    }

                                    var itemTempalte = ItemCache.ItemTemplate(itemBag.Id);

                                    if (!itemTempalte.IsTypeBody() || itemTempalte.Type == 5)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR));
                                        return Task.CompletedTask;
                                    }

                                    var info = itemTempalte.Name;
                                    var idText = 0;
                                    var count = 0;
                                    itemBag.Options.ForEach(option =>
                                    {
                                        if (option.Id == 107)
                                        {
                                            count += option.Param;
                                        }
                                        else if (option.Id != 107 && option.Id != 102 && option.Id != 72)
                                        {
                                            if (idText > 6)
                                            {
                                                idText = 0;
                                            }

                                            info +=
                                                $"{ServerUtils.Color(DataCache.TextColor[idText++])}{ItemCache.ItemOptionTemplate(option.Id).Name.Replace("#", option.Param + "")}";
                                        }
                                    });
                                    if (count >= DataCache.MAX_LIMIT_SPL)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR));
                                        return Task.CompletedTask;
                                    }

                                    var percentPhaLe = DataCache.PercentPhaLe[count];

                                    info += $"\b{string.Format(TextServer.gI().PHA_LE_TRANG_BI, (count + 1))}";
                                    info +=
                                        $"{ServerUtils.Color("blue")}{string.Format(TextServer.gI().PHA_LE_TRANG_BI_2, percentPhaLe[1])}";
                                    info +=
                                        $"{ServerUtils.Color(DataCache.PercentPhaLe[count][0] * 1000000 < character.InfoChar.Gold ? "blue" : "red")}{string.Format(TextServer.gI().PHA_LE_TRANG_BI_3, percentPhaLe[0])}";

                                    character.CharacterHandler.SendMessage(
                                        Service
                                            .OpenUiConfirm(21, info,
                                                new List<string>()
                                                {
                                            string.Format(TextServer.gI().PHA_LE_TRANG_BI_4, percentPhaLe[2]), "Từ chối"
                                                }, character.InfoChar.Gender));
                                    character.TypeMenu = 9;
                                    character.CombinneIndex = new List<int>()
                            {
                                itemBag.IndexUI,
                                count
                            };
                                    break;
                                }
                            //Chuyển hoá VÀNG / 5
                            //Chuyển hoá NGỌC / 6
                            case 5:
                            case 6:
                                {
                                    if (arrIndexUi.Count != 2)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR));
                                        return Task.CompletedTask;
                                    }

                                    var indexLuongLong = arrIndexUi[0];
                                    var indexThan = arrIndexUi[1];

                                    var itemLuongLong =
                                        character.CharacterHandler.GetItemBagByIndex(indexLuongLong); //Trang bị level
                                    var itemThan = character.CharacterHandler.GetItemBagByIndex(indexThan); //ĐỒ thần
                                    if (itemLuongLong == null || itemThan == null) return Task.CompletedTask;
                                    var template1 = ItemCache.ItemTemplate(itemLuongLong.Id);
                                    var template2 = ItemCache.ItemTemplate(itemThan.Id);
                                    if (template1.Level != 12)
                                    {
                                        (itemThan, itemLuongLong) = (itemLuongLong, itemThan);
                                        (template2, template1) = (template1, template2);
                                    }

                                    if (template1.Level != 12 && template1.Level != 13 ||
                                        template2.Level != 12 && template2.Level != 13)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().NEED_LEVEL_ITEM));
                                        return Task.CompletedTask;
                                    }

                                    if (template1.Type != template2.Type || template1.Gender != template2.Gender)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().NEED_EQUIPMENT_SAME_KIND));
                                        return Task.CompletedTask;
                                    }

                                    var option72Ll = itemLuongLong.Options.FirstOrDefault(opt => opt.Id == 72); //Cấp lưỡng long
                                    if (itemThan.Options.Count != template2.Options.Count || option72Ll == null ||
                                        option72Ll.Param < 4)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().NEED_TRUE_EQUIPMENT));
                                        return Task.CompletedTask;
                                    }

                                    var levelLuongLong = option72Ll.Param;
                                    var optionOld = "";

                                    var listOptionLlGoc = itemLuongLong.Options
                                        .Where(opt => DataCache.IdOptionGoc.Contains(opt.Id)).ToList();
                                    var listOptionLl = itemLuongLong.Options
                                        .Where(opt => opt.Id != 107 && opt.Id != 72 && opt.Id != 102).ToList();
                                    var listOptionThan = itemThan.Options
                                        .Where(opt => opt.Id != 107 && opt.Id != 72 && opt.Id != 102).ToList();
                                    listOptionThan.ForEach(opt =>
                                    {
                                        optionOld +=
                                            $"{ServerUtils.Color("brown")}{ItemCache.ItemOptionTemplate(opt.Id).Name.Replace("#", opt.Param + "")}";
                                    });
                                    var info = $"{ServerUtils.Color("blue")}Hiện tại {template2.Name}{optionOld}";

                                    var menu = new List<string>();
                                    var checkGold = 0;

                                    var optionUpgrade = "";
                                    switch (character.ShopId)
                                    {
                                        case 5:
                                            {
                                                levelLuongLong -= 1;

                                                listOptionThan.ForEach(opt =>
                                                {
                                                    var paramNew = opt.Param;
                                                    var optCheck = listOptionLlGoc.FirstOrDefault(o => o.Id == opt.Id);
                                                    if (optCheck != null)
                                                    {
                                                        paramNew += optCheck.Param - optCheck.Param / 10;
                                                    }

                                                    optionUpgrade +=
                                                        $"{ServerUtils.Color("green")}{ItemCache.ItemOptionTemplate(opt.Id).Name.Replace("#", paramNew + "")}";
                                                });
                                                var listCheckPlus = listOptionLl.Where(opt =>
                                                    listOptionThan.FirstOrDefault(o => o.Id == opt.Id) == null).ToList();
                                                listCheckPlus.ForEach(opt =>
                                                {
                                                    var paramNew = opt.Param;
                                                    optionUpgrade +=
                                                        $"{ServerUtils.Color("green")}{ItemCache.ItemOptionTemplate(opt.Id).Name.Replace("#", paramNew + "")}";
                                                });

                                                checkGold = (listOptionLl.Count * 50 + levelLuongLong * 250 + 100) * 100000;
                                                info += $"{ServerUtils.Color("blue")}Sau khi nâng cấp (+{levelLuongLong})";
                                                info += optionUpgrade;
                                                info += $"{ServerUtils.Color("green")}Chuyển qua tất cả sao pha lê";
                                                info +=
                                                    $"{ServerUtils.Color(checkGold < character.InfoChar.Gold ? "blue" : "red")}Cần {ServerUtils.GetPower(checkGold)} vàng";

                                                menu = checkGold < character.InfoChar.Gold
                                                    ? new List<string>()
                                                        {$"Chuyển hoá\n{ServerUtils.GetMoney(checkGold)}\nvàng", "Từ chối"}
                                                    : new List<string>()
                                                    {
                                            $"Còn thiếu\n{ServerUtils.GetMoney(checkGold - character.InfoChar.Gold)}\nvàng"
                                                    };

                                                character.TypeMenu = 10;
                                                break;
                                            }
                                        case 6:
                                            {
                                                listOptionThan.ForEach(opt =>
                                                {
                                                    var paramNew = opt.Param;
                                                    var optCheck = listOptionLlGoc.FirstOrDefault(o => o.Id == opt.Id);
                                                    if (optCheck != null)
                                                    {
                                                        paramNew += optCheck.Param;
                                                    }

                                                    optionUpgrade +=
                                                        $"{ServerUtils.Color("green")}{ItemCache.ItemOptionTemplate(opt.Id).Name.Replace("#", paramNew + "")}";
                                                });
                                                var listCheckPlus = listOptionLl.Where(opt =>
                                                    listOptionThan.FirstOrDefault(o => o.Id == opt.Id) == null).ToList();
                                                listCheckPlus.ForEach(opt =>
                                                {
                                                    var paramNew = opt.Param;
                                                    optionUpgrade +=
                                                        $"{ServerUtils.Color("green")}{ItemCache.ItemOptionTemplate(opt.Id).Name.Replace("#", paramNew + "")}";
                                                });

                                                checkGold = listOptionLl.Count * 50 + levelLuongLong * 250 + 100;
                                                info += $"{ServerUtils.Color("blue")}Sau khi nâng cấp (+{levelLuongLong})";
                                                info += optionUpgrade;
                                                info += $"{ServerUtils.Color("green")}Chuyển qua tất  cả sao pha lê";
                                                info +=
                                                    $"{ServerUtils.Color(checkGold < character.InfoChar.Gold ? "blue" : "red")}Cần {checkGold} ngọc";

                                                menu = checkGold < character.InfoChar.Gold
                                                    ? new List<string>() { $"Chuyển hoá\n{checkGold}\ngọc", "Từ chối" }
                                                    : new List<string>()
                                                        {$"Còn thiếu\n{checkGold - character.InfoChar.Gold}\nngọc"};

                                                character.TypeMenu = 11;
                                                break;
                                            }
                                    }

                                    character.CharacterHandler.SendMessage(Service.OpenUiConfirm(21, info, menu,
                                        character.InfoChar.Gender));

                                    character.CombinneIndex = new List<int>()
                            {
                                itemLuongLong.IndexUI,
                                itemThan.IndexUI,
                                levelLuongLong,
                                checkGold
                            };
                                    break;
                                }
                            case 7: //Nâng cấp item porata
                                {
                                    // chỉ có 2 loại vật phẩm bỏ vào
                                    if (arrIndexUi.Count != 2)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR_PORATA_2));
                                        return Task.CompletedTask;
                                    }
                                    // Bông tai cấp một
                                    var itemBongTai = character.CharacterHandler.GetItemBagByIndex(arrIndexUi[0]);
                                    if (itemBongTai == null || itemBongTai.Id != 454)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR_PORATA_2_FIRST));
                                        return Task.CompletedTask;
                                    }

                                    // Mảnh vỡ bông tai
                                    var itemManhVoBongTai = character.CharacterHandler.GetItemBagByIndex(arrIndexUi[1]);
                                    if (itemManhVoBongTai == null || itemManhVoBongTai.Id != 933)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR_PORATA_2_SECOND));
                                        return Task.CompletedTask;
                                    }

                                    var soLuongManhVoBongTai = itemManhVoBongTai.Options.FirstOrDefault(opt => opt.Id == 31); //Số lượng bông tai
                                    if (soLuongManhVoBongTai == null || soLuongManhVoBongTai.Param < 9999)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().TRANG_BI_ERROR_PORATA_2_SECOND));
                                        return Task.CompletedTask;
                                    }

                                    var dataCombinne = DataCache.PercentUpgradePorata2;
                                    var checkDiamond = dataCombinne[0] < character.AllDiamond();
                                    var checkGold = dataCombinne[1] < character.InfoChar.Gold;

                                    var info = $"{ServerUtils.Color("blue")}Bông tai Porata [+2]";
                                    info += $"{ServerUtils.Color("blue")}{string.Format(TextServer.gI().PERCENT_UPGRADE, dataCombinne[2])}%";
                                    info += $"{ServerUtils.Color("blue")}Cần 9999 Mảnh vỡ bông tai";
                                    info +=
                                        $"{ServerUtils.Color(checkGold ? "blue" : "red")}{string.Format(TextServer.gI().NEED_GOLD, ServerUtils.GetMoney(dataCombinne[1]))}";
                                    info +=
                                        $"{ServerUtils.Color(checkDiamond ? "blue" : "red")}{string.Format(TextServer.gI().NEED_DIAMOND, dataCombinne[0])}";
                                    info += $"{ServerUtils.Color("red")}Thất bại -99 mảnh vỡ bông tai";

                                    character.CharacterHandler.SendMessage(
                                        Service
                                            .OpenUiConfirm(21, info,
                                                new List<string>() { "Nâng cấp\n" + ServerUtils.GetMoney(dataCombinne[1]) + " vàng\n" + dataCombinne[0] + " ngọc", "Từ chối" },
                                                character.InfoChar.Gender));
                                    character.TypeMenu = 12;
                                    character.CombinneIndex = new List<int>()
                            {
                                arrIndexUi[0],
                                arrIndexUi[1],
                                dataCombinne[0],
                                dataCombinne[1],
                                dataCombinne[2]
                            };
                                    return Task.CompletedTask;
                                }
                            case 8://mở chỉ số item porata
                                {
                                    // chỉ có 3 loại vật phẩm bỏ vào
                                    if (arrIndexUi.Count != 3)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().UPGRADE_OPTION_PORATA_2_ERROR_COUNT));
                                        return Task.CompletedTask;
                                    }

                                    // Bông tai cấp 2
                                    var itemBongTai = character.CharacterHandler.GetItemBagByIndex(arrIndexUi[0]);
                                    if (itemBongTai == null || itemBongTai.Id != 921)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().UPGRADE_OPTION_PORATA_2_FIRST));
                                        return Task.CompletedTask;
                                    }

                                    // Mảnh hồn bông tai
                                    var itemManhVoBongTai = character.CharacterHandler.GetItemBagByIndex(arrIndexUi[1]);
                                    if (itemManhVoBongTai == null || itemManhVoBongTai.Id != 934 || itemManhVoBongTai.Quantity < 99)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().UPGRADE_OPTION_PORATA_2_SECOND));
                                        return Task.CompletedTask;
                                    }

                                    // Đá xanh lam
                                    var itemDaXanhLam = character.CharacterHandler.GetItemBagByIndex(arrIndexUi[2]);
                                    if (itemDaXanhLam == null || itemDaXanhLam.Id != 935)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage(TextServer.gI().UPGRADE_OPTION_PORATA_2_THIRD));
                                        return Task.CompletedTask;
                                    }

                                    var dataCombinne = DataCache.PercentUpgradePorata2;
                                    var checkDiamond = dataCombinne[0] < character.AllDiamond();
                                    var checkGold = dataCombinne[1] < character.InfoChar.Gold;

                                    var info = $"{ServerUtils.Color("blue")}Bông tai Porata [+2]";
                                    info += $"{ServerUtils.Color("blue")}{string.Format(TextServer.gI().PERCENT_UPGRADE, dataCombinne[2])}%";
                                    info += $"{ServerUtils.Color("blue")}Cần 99 Mảnh hồn bông tai";
                                    info += $"{ServerUtils.Color("blue")}Cần 1 Đá xanh lam";
                                    info +=
                                        $"{ServerUtils.Color(checkGold ? "blue" : "red")}{string.Format(TextServer.gI().NEED_GOLD, ServerUtils.GetMoney(dataCombinne[1]))}";
                                    info +=
                                        $"{ServerUtils.Color(checkDiamond ? "blue" : "red")}{string.Format(TextServer.gI().NEED_DIAMOND, dataCombinne[0])}";
                                    info += $"{ServerUtils.Color("green")}+1 Chỉ số ngẫu nhiên";

                                    character.CharacterHandler.SendMessage(
                                        Service
                                            .OpenUiConfirm(21, info,
                                                new List<string>() { "Nâng cấp\n" + ServerUtils.GetMoney(dataCombinne[1]) + " vàng\n" + dataCombinne[0] + " ngọc", "Từ chối" },
                                                character.InfoChar.Gender));
                                    character.TypeMenu = 13;
                                    character.CombinneIndex = new List<int>()
                            {
                                arrIndexUi[0],
                                arrIndexUi[1],
                                arrIndexUi[2],
                                dataCombinne[0],
                                dataCombinne[1],
                                dataCombinne[2]
                            };
                                    return Task.CompletedTask;
                                }
                            case 9://nở trứng pet
                                {
                                    if (arrIndexUi.Count > 3 || arrIndexUi.Count < 2)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage("Cần chọn đúng trứng linh thú, 99 hồn linh thú, (Thỏi vàng nếu muốn nở trứng sớm) theo đúng thứ tự"));
                                        return Task.CompletedTask;
                                    }

                                    // Trứng linh thú
                                    var itemTrungLinhThu = character.CharacterHandler.GetItemBagByIndex(arrIndexUi[0]);
                                    if (itemTrungLinhThu == null || itemTrungLinhThu.Id != 1049)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage("Cần cho trứng linh thú vào đầu tiên"));
                                        return Task.CompletedTask;
                                    }

                                    // Hồn linh thú
                                    var itemHonLinhThu = character.CharacterHandler.GetItemBagByIndex(arrIndexUi[1]);
                                    if (itemHonLinhThu == null || itemHonLinhThu.Id != 1048 || itemHonLinhThu.Quantity < 99)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage("Cần cho 99 hồn linh thú vào thứ 2"));
                                        return Task.CompletedTask;
                                    }

                                    var thoiGianNoTrung = itemTrungLinhThu.Options.FirstOrDefault(option => option.Id == 211);

                                    if (thoiGianNoTrung != null)
                                    {
                                        if (arrIndexUi.Count != 3)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.DialogMessage("Cần chọn đúng trứng linh thú, 99 hồn linh thú, 5 thỏi vàng theo đúng thứ tự"));
                                            return Task.CompletedTask;
                                        }

                                        var itemThoiVang = character.CharacterHandler.GetItemBagByIndex(arrIndexUi[2]);
                                        if (itemThoiVang == null || itemThoiVang.Id != 457 || itemThoiVang.Quantity < 5)
                                        {
                                            character.CharacterHandler.SendMessage(
                                                Service.DialogMessage("Cần có 5 thỏi vàng để nở trứng sớm"));
                                            return Task.CompletedTask;
                                        }
                                    }

                                    if (character.LengthBagNull() <= 0)
                                    {
                                        character.CharacterHandler.SendMessage(
                                            Service.DialogMessage("Cần 1 ô trống hành trang để chứa linh thú"));
                                        return Task.CompletedTask;
                                    }

                                    var info = $"{ServerUtils.Color("red")}Nở trứng linh thú";
                                    info += $"{ServerUtils.Color("blue")}Cần 1 Trứng linh thú";
                                    info += $"{ServerUtils.Color("blue")}Cần 99 Hồn linh thú";

                                    if (arrIndexUi.Count == 3)
                                    {
                                        info += $"{ServerUtils.Color("blue")}Cần 5 thỏi vàng để nở trứng sớm";
                                    }
                                    info += $"{ServerUtils.Color("green")}Sẽ nở ra linh thú cùng hạng của trứng";

                                    character.CharacterHandler.SendMessage(
                                        Service
                                            .OpenUiConfirm(21, info,
                                                new List<string>() { "Nở trứng", "Từ chối" },
                                                character.InfoChar.Gender));
                                    character.TypeMenu = 15;

                                    if (arrIndexUi.Count == 3)
                                    {
                                        character.CombinneIndex = new List<int>()
                                {
                                    arrIndexUi[0],
                                    arrIndexUi[1],
                                    arrIndexUi[2],
                                };
                                    }
                                    else
                                    {
                                        character.CombinneIndex = new List<int>()
                                {
                                    arrIndexUi[0],
                                    arrIndexUi[1],
                                };
                                    }

                                    return Task.CompletedTask;
                                }
                        }

                        break;
                    }
            }

            return Task.CompletedTask;
        }
    }
}
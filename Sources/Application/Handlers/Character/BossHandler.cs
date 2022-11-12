using System;
using System.Collections.Generic;
using System.Linq;
using Linq.Extras;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Handlers.Item;
using NRO_Server.Application.Handlers.Skill;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Monster;
using NRO_Server.Application.IO;
using NRO_Server.Application.Main;
using NRO_Server.Application.Manager;
using NRO_Server.Application.Map;
using NRO_Server.Application.Threading;
using NRO_Server.DatabaseManager;
using NRO_Server.DatabaseManager.Player;
using NRO_Server.Model;
using NRO_Server.Model.Character;
using NRO_Server.Model.Map;
using NRO_Server.Model.SkillCharacter;
using NRO_Server.Model.Info;
using NRO_Server.Model.Template;
using NRO_Server.Model.Item;
using NRO_Server.Model.Option;
using Org.BouncyCastle.Math.Field;
using static System.GC;

namespace NRO_Server.Application.Handlers.Character
{
    public class BossHandler : ICharacterHandler
    {
        public Boss Boss { get; set; }

        public BossHandler(Boss boss)
        {
            Boss = boss;
        }

        public void Dispose()
        {
            SuppressFinalize(this);
        }

        public void SendZoneMessage(Message message)
        {
            Boss?.Zone?.ZoneHandler.SendMessage(message);
        }

        public void Update()
        {
            lock (Boss)
            {
                if(Boss.Status >= 3) return;
                var timeServer = ServerUtils.CurrentTimeMillis();
                RemoveSkill(timeServer);
                if (Boss.InfoChar.IsDie && Boss.InfoDelayBoss.LeaveDead <= timeServer)
                {
                    Boss.InfoDelayBoss.LeaveDead = -1;
                    LeaveFromDead();
                }
                else if (!Boss.InfoChar.IsDie)
                {
                    AutoBoss(timeServer);
                    UpdateEffect(timeServer);
                }
            }
        }

        private void AutoBoss(long timeServer)
        {
            if(Boss.IsDontMove()) return;
            // if (Boss.InfoChar.Stamina <= 0)
            // {
            //     Boss.CharacterHandler.SendZoneMessage(Service.PlayerMove(Boss.Id, Boss.InfoChar.X, Boss.InfoChar.Y));
            // }
            // else
            // {
            // }
            HandleBossAction();
        }

        private void HandleBossAction()
        {
            var timeServer = ServerUtils.CurrentTimeMillis();
            var checkSize = 500;

            if (Boss.CharacterAttack.Count > 0)
            {
                foreach (var id in Boss.CharacterAttack.ToList())
                {
                    var character = Boss.Zone.ZoneHandler.GetCharacter(id);

                    if (character == null)
                    {
                        Boss.CharacterAttack.RemoveAll(i => i == id);
                        continue;
                    }
                    
                    if (character.InfoSkill.Socola.IsSocola)
                    {
                        Boss.CharacterAttack.RemoveAll(i => i == id);
                        continue;
                    }
                    if (character.InfoChar.IsDie)
                    {
                        Boss.CharacterAttack.RemoveAll(i => i == id);
                        continue;
                    }
                    var distance = Math.Abs(character.InfoChar.X - Boss.InfoChar.X);
                    if (!character.InfoSkill.Socola.IsSocola && !character.InfoChar.IsDie && distance <= checkSize && Math.Abs(character.InfoChar.Y - Boss.InfoChar.Y) <= checkSize)
                    {
                        HandleUseSkill(character);
                        break;
                    }
                    Boss.CharacterAttack.RemoveAll(i => i == id);
                }
            }
            else 
            {
                checkSize = 300;
                var character = Boss.CharacterFocus;
                if (character == null || Boss.Zone.ZoneHandler.GetCharacter(character.Id) == null || character.InfoChar.IsDie || Math.Abs(character.InfoChar.X - Boss.InfoChar.X) > checkSize || Math.Abs(character.InfoChar.Y - Boss.InfoChar.Y) > 600)
                {
                    Boss.CharacterFocus = character = Boss.Zone.Characters.Values.ToList().FirstOrDefault(m => !m.InfoChar.IsDie && !m.InfoSkill.Socola.IsSocola && !CheckNearWaypoint(m) && Math.Abs(m.InfoChar.X - Boss.InfoChar.X) <= checkSize && Math.Abs(m.InfoChar.Y - Boss.InfoChar.Y) <= 600);
                }

                if (character == null)
                {
                    AutoMoveMap(timeServer);
                }
                else
                {
                    if (character.InfoSkill.Socola.IsSocola)
                    {
                        AutoMoveMap(timeServer);
                        return;
                    }
                    HandleUseSkill(character);
                }
            }
        }

        #region boss style attack
        private void HandleUseSkill(ICharacter character)
        {
            
            var infoSkill = Boss.InfoSkill;
            var timeServer = ServerUtils.CurrentTimeMillis();
            // Tái tạo năng lượng
            // Đang tái tạo năng lượng sẽ không bị xóa
            if (infoSkill.TaiTaoNangLuong.IsTTNL &&
                 infoSkill.TaiTaoNangLuong.DelayTTNL > timeServer)
            {
                if (Boss.InfoDelayBoss.TTNL <= timeServer)
                {
                    // Xử lý tái tạo năng lượng
                    SkillHandler.BossSkillNotFocus(Boss, 8, 2);
                    Boss.InfoDelayBoss.TTNL = timeServer + 2000;
                }
                return;
            }

            // Tìm chiêu để sử dụng
            SkillCharacter skillChar = null;
            var dX = 0;
            var dY = 0;
            bool isMoveToPlayer = false;
            try {

                // Kiểm tra khoản cách giữa quái và đệ
                var bossDistance = Math.Abs(character.InfoChar.X - Boss.InfoChar.X);
                var bossDistanceY = Math.Abs(character.InfoChar.Y - Boss.InfoChar.Y);
                // for skill từ trên xuống dưới
                for (int i = Boss.Skills.Count - 1; i >= 0; i--)
                {
                    skillChar = Boss.Skills[i];
                    
                    if (skillChar == null)
                    {
                        continue;
                    }

                    var skillTemplate = Cache.Gi().SKILL_TEMPLATES.FirstOrDefault(sk => sk.Id == skillChar.Id);
                    var skillDataTemplate = skillTemplate?.SkillDataTemplates.FirstOrDefault(so => so.SkillId == skillChar.SkillId);
                    if (skillDataTemplate == null)
                    {
                        skillChar = null;
                        continue;
                    }
                    
                    //Check mana
                    var manaUse = skillDataTemplate.ManaUse;
                    var manaUseType = skillTemplate.ManaUseType;
                    var manaChar = Boss.InfoChar.Mp;
                    manaUse = manaUseType switch
                    {
                        1 => manaUse * (int) Boss.MpFull / 100,
                        2 => (int) manaChar,
                        _ => manaUse
                    };

                    if (manaUse > manaChar || skillChar.CoolDown > timeServer) 
                    {
                        skillChar = null;
                        continue;
                    }

                    dX = skillDataTemplate.Dx;
                    dY = skillDataTemplate.Dy;
                    // Nếu skill 3,4 thỏa mãn đk thì lấy
                    if (i == 3 || i == 2)
                    {
                        if (skillChar.Id == 8)
                        {
                            if (ServerUtils.RandomNumber(0, 100) < 80) continue;
                            var hpMine = Boss.HpFull*0.6;
                            if (hpMine < Boss.InfoChar.Hp)
                            {
                                skillChar = null;
                                continue;
                            }
                        
                        }
                        else if (skillChar.Id == 9)
                        {
                            if (ServerUtils.RandomNumber(0, 100) < 80) continue;
                            var hpMine = Boss.HpFull / 10;
                            if (hpMine >= Boss.InfoChar.Hp)
                            {
                                skillChar = null;
                                continue;
                            }
                        
                        }
                        break;
                    }
                    // Nếu skill 2 khoản cách lớn hơn >36 thì lấy\
                    if (i == 1 && (((ServerUtils.RandomNumber(100) < 50) && bossDistance <= dX) && (bossDistanceY <= dY)))
                    {
                        break;
                    }
                    else if (i == 0)
                    {
                        if ((bossDistance <= dX && bossDistanceY <= dY) || Boss.Skills.Count == 1)
                        {
                            break;
                        }
                        else 
                        {
                            isMoveToPlayer = true;
                            break;
                        }
                    }
                }

                if (skillChar == null)
                {
                    return;
                }

                if (Boss.InfoDelayBoss.AutoChat <= timeServer)
                {
                    CombatChatMessage(timeServer);
                }

                if (skillChar.Id == 8)
                {
                    // Bắt đầu dùng tái tạo năng lượng
                    SkillHandler.BossSkillNotFocus(Boss, skillChar.Id, 1);
                    return;
                }

                // Thái dương hạ sang
                if (skillChar.Id == 6)
                {
                    SkillHandler.BossSkillNotFocus(Boss, skillChar.Id, 0);
                    return;
                }

                // Khiên năng lượng
                if (skillChar.Id == 19)
                {
                    SkillHandler.BossSkillNotFocus(Boss, skillChar.Id, 9);
                    return;
                }

                if (skillChar.Id == 0 || skillChar.Id == 2 || skillChar.Id == 4 || skillChar.Id == 9 || skillChar.Id == 17 || isMoveToPlayer)
                {
                    if (character.InfoChar.X > Boss.InfoChar.X)
                    {
                        Boss.InfoChar.X = (short)(character.InfoChar.X - dX);
                    }
                    else
                    {
                        Boss.InfoChar.X = (short)(character.InfoChar.X + dX);
                    }

                    
                    Boss.InfoChar.Y = character.InfoChar.Y;
                    isMoveToPlayer = false;

                    SendZoneMessage(Service.PlayerMove(Boss.Id, Boss.InfoChar.X, Boss.InfoChar.Y));
                }
                else if ((skillChar.Id == 1 || skillChar.Id == 3 || skillChar.Id == 5) && Boss.InfoChar.Y > character.InfoChar.Y)
                {
                    if (ServerUtils.RandomNumber(100) < 35)
                    {
                        Boss.InfoChar.Y = (short)(character.InfoChar.Y - ServerUtils.RandomNumber(0, 40));
                    }
                    else
                    {
                        Boss.InfoChar.Y = character.InfoChar.Y;
                    }
                    SendZoneMessage(Service.PlayerMove(Boss.Id, Boss.InfoChar.X, Boss.InfoChar.Y));
                }
                AttackPlayer(character, skillChar);

                // 
                if (Boss.Type == DataCache.BOSS_THO_PHE_CO_TYPE)
                {
                    Boss.InfoDelayBoss.AutoChangeMap = timeServer + 500000;
                }
            }
            catch (Exception)
            {
                // Ignore
                return;
            }

        }
        
        private void AttackPlayer(ICharacter character, SkillCharacter skillChar)
        {
            if (!character.InfoChar.IsDie)
            {
                var isNearWaypoint = CheckNearWaypoint(Boss);
                if (isNearWaypoint)
                {
                    Boss.CharacterFocus = null;
                    MoveMap(Boss.BasePositionX, Boss.BasePositionY);
                }
                else 
                {
                    SkillHandler.BossAttackPlayer(Boss, skillChar, character.Id);
                }
            }
        }
        #endregion

        private void AutoMoveMap(long timeServer)
        {
            if (Boss.InfoDelayBoss.AutoMove <= timeServer)
            {
                Boss.InfoChar.X = (short)ServerUtils.RandomNumber(Boss.BasePositionX - 50,
                    Boss.BasePositionX + 50);
                SendZoneMessage(Service.PlayerMove(Boss.Id, Boss.InfoChar.X, Boss.InfoChar.Y));
                if (Boss.InfoSkill.MeTroi.IsMeTroi &&
                    Boss.InfoSkill.MeTroi.DelayStart <= timeServer)
                {
                    SkillHandler.RemoveTroi(Boss);
                }
                Boss.InfoDelayBoss.AutoMove = timeServer + ServerUtils.RandomNumber(2000, 4000);
            }

            if (Boss.InfoDelayBoss.AutoChat <= timeServer)
            {
                IdleChatMessage(timeServer);
            }
            // Boss thỏ phê cỏ tự đổi map tìm người
            if (Boss.InfoDelayBoss.AutoChangeMap <= timeServer)
            {
                AutoChangeMap(timeServer);
            }
        }

        private void AutoChangeMap(long timeServer)
        {
            Boss.InfoDelayBoss.AutoChangeMap = timeServer + 500000;
            if (Boss.Type == DataCache.BOSS_THO_PHE_CO_TYPE)
            {
                var randChar = ClientManager.Gi().GetRandomCharacter();
                if (randChar != null) 
                {
                    var zone = randChar.Zone;
                    if (zone != null)
                    {
                        Boss.Zone.ZoneHandler.RemoveBoss(Boss);
                        Boss.CharacterHandler.SetUpInfo();
                        Boss.InfoChar.X = randChar.InfoChar.X;
                        Boss.InfoChar.Y = randChar.InfoChar.Y;
                        Boss.BasePositionX = randChar.InfoChar.X;
                        Boss.BasePositionY = randChar.InfoChar.Y;
                        zone.ZoneHandler.AddBoss(Boss);
                        ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Thỏ Phê Cỏ " + Boss.Id + " vừa xuất hiện tại " + zone.Map.TileMap.Name));
                    }
                }
                // tự đổi map khác
            }
        }

        private void SendBossChat(string text)
        {
            if (Boss.Status < 3)
            {
                SendZoneMessage(Service.PublicChat(Boss.Id, text));
            };
        }

        private void CombatChatMessage(long timeServer)
        {
            if (DatabaseManager.Manager.gI().SuKienTrungThu)
            {
                SendBossChat(TextServer.gI().BOSS_MOON_CHAT_COMBAT[ServerUtils.RandomNumber(TextServer.gI().BOSS_MOON_CHAT_COMBAT.Count)]);
            }
            else 
            {
                SendBossChat(TextServer.gI().BOSS_CHAT_COMBAT[ServerUtils.RandomNumber(TextServer.gI().BOSS_CHAT_COMBAT.Count)]);
            }
            Boss.InfoDelayBoss.AutoChat = timeServer + ServerUtils.RandomNumber(3000, 5000);
        }

        private void IdleChatMessage(long timeServer)
        {
            if (DatabaseManager.Manager.gI().SuKienTrungThu)
            {
                SendBossChat(TextServer.gI().BOSS_MOON_CHAT_IDLE[ServerUtils.RandomNumber(TextServer.gI().BOSS_MOON_CHAT_IDLE.Count)]);
            }
            else 
            {
                SendBossChat(TextServer.gI().BOSS_CHAT_IDLE[ServerUtils.RandomNumber(TextServer.gI().BOSS_CHAT_IDLE.Count)]);
            }
            Boss.InfoDelayBoss.AutoChat = timeServer + ServerUtils.RandomNumber(3000, 5000);
        }

        private void DieChatMessage()
        {
            if (DatabaseManager.Manager.gI().SuKienTrungThu)
            {
                SendBossChat(TextServer.gI().BOSS_MOON_CHAT_DIE[ServerUtils.RandomNumber(TextServer.gI().BOSS_MOON_CHAT_DIE.Count)]);
            }
            else 
            {
                SendBossChat(TextServer.gI().BOSS_CHAT_DIE[ServerUtils.RandomNumber(TextServer.gI().BOSS_CHAT_DIE.Count)]);
            }
        }

        private bool CheckNearWaypoint(ICharacter character)
        {
            var checkWP = MapManager.Get(character.InfoChar.MapId)?.TileMap.WayPoints.FirstOrDefault(waypoint => CheckTrueWaypoint(character, waypoint));
            if (checkWP != null)
            {
                return true;
            }
            return false;
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

        public void Close()
        {
            Clear();
        }

        public void Clear()
        {
            SuppressFinalize(this);
        }

        public void UpdateInfo()
        {
            SetUpInfo();
            SendZoneMessage(Service.UpdateBody(Boss));
        }

        public void SetUpPosition(int mapOld = -1, int mapNew = -1, bool isRandom = false)
        {
            // PositionHandler.SetUpPosition(Boss, mapOld, mapNew);
            if (Boss.BasePositionX != 0 && Boss.BasePositionY != 0) return;
            switch (mapNew)
            {
                case 5://Đảo Kame
                {
                    Boss.BasePositionX = 593;
                    Boss.BasePositionY = 384;
                    break;
                }
                case 29://Nam Kame
                {
                    Boss.BasePositionX = 1006;
                    Boss.BasePositionY = 360;
                    break;
                }
                case 13://Đảo Guru
                {
                    Boss.BasePositionX = 1230;
                    Boss.BasePositionY = 384;
                    break;
                }
                case 20://Vách núi đen
                {
                    Boss.BasePositionX = 533;
                    Boss.BasePositionY = 360;
                    break;
                }
                case 33://Nam Guru
                {
                    Boss.BasePositionX = 973;
                    Boss.BasePositionY = 288;
                    break;
                }
                case 36://Rừng đá
                {
                    Boss.BasePositionX = 696;
                    Boss.BasePositionY = 288;
                    break;
                }
                case 38://Bờ vực đen
                {
                    Boss.BasePositionX = 516;
                    Boss.BasePositionY = 240;
                    break;
                }
                case 68://Thung lũng Nappa
                {
                    Boss.BasePositionX = 920;
                    Boss.BasePositionY = 312;
                    break;
                }
                case 69://Vực cấm
                {
                    Boss.BasePositionX = 773;
                    Boss.BasePositionY = 384;
                    break;
                }
                case 70://Núi Appule
                {
                    Boss.BasePositionX = 800;
                    Boss.BasePositionY = 360;
                    break;
                }
                case 71://Căn cứ Ras
                {
                    Boss.BasePositionX = 526;
                    Boss.BasePositionY = 624;
                    break;
                }
                case 72://Thung lũng Ras
                {
                    Boss.BasePositionX = 964;
                    Boss.BasePositionY = 312;
                    break;
                }
                case 92://Thành phố phía đông
                {
                    Boss.BasePositionX = 704;
                    Boss.BasePositionY = 360;
                    break;
                }
                case 93://Thành phố phía nam
                {
                    Boss.BasePositionX = 784;
                    Boss.BasePositionY = 360;
                    break;
                }
                case 103: //võ đài xên bọ hung
                {
                    Boss.BasePositionX = 414;
                    Boss.BasePositionY = 288;
                    break;
                }
                case 107:
                {
                    Boss.BasePositionX = 401;
                    Boss.BasePositionY = 696;
                    break;
                }
                case 108:
                {
                    Boss.BasePositionX = 445;
                    Boss.BasePositionY = 360;
                    break;
                }
                case 110:
                {
                    Boss.BasePositionX = 438;
                    Boss.BasePositionY = 432;
                    break;
                }
                case 161://HTTV
                {
                    Boss.BasePositionX = 831;
                    Boss.BasePositionY = 144;
                    break;
                }
                default:
                {
                    Boss.BasePositionX = (short)ServerUtils.RandomNumber(250,450);
                    Boss.BasePositionY = 0;
                    break;
                }
            }
            Boss.InfoChar.X = Boss.BasePositionX;
            Boss.InfoChar.Y = Boss.BasePositionY;
            SendZoneMessage(Service.SendPos(Boss, 1));
            
        }

        public void SendInfo()
        {
            SetUpInfo();
        }

        public void SendDie()
        {
            lock (Boss)
            {
                RemoveSkill(ServerUtils.CurrentTimeMillis(), true);
                Boss.InfoChar.IsDie = true;
                Boss.InfoSkill.Monkey.MonkeyId = 0;
                SetUpInfo();
                SendZoneMessage(Service.PlayerDie(Boss));
                Boss.InfoDelayBoss.LeaveDead = ServerUtils.CurrentTimeMillis() + 5000;
                DieChatMessage();
                Boss.CharacterAttack.Clear();
                // Boss.Zone.ZoneHandler.RemoveBoss(Boss);
                // Dispose();
            }
        }

        public void LeaveItem(ICharacter character)
        {
            var playerKillId = Math.Abs(character.Id);

            ClientManager.Gi().SendMessageCharacter(Service.ServerChat(character.Name + ": Đã tiêu diệt được " + Boss.Name + " mọi người đều ngưỡng mộ."));

            switch(Boss.Type)
            {
                case 1://super broly
                {
                    var item = ItemCache.GetItemDefault(568);
                    var itemMap = new ItemMap(playerKillId, item);
                    itemMap.X = Boss.InfoChar.X;
                    itemMap.Y = Boss.InfoChar.Y;
                    Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    break;
                }
                case 2://black goku
                // {
                //     if (ServerUtils.RandomNumber(100) > 50) return;
                //     var item = ItemCache.GetItemDefault(((short)ServerUtils.RandomNumber(15,16)));
                //     var itemMap = new ItemMap(playerKillId, item);
                //     itemMap.X = Boss.InfoChar.X;
                //     itemMap.Y = Boss.InfoChar.Y;
                //     Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                //     break;
                // }
                case 3://super black goku
                {
                    var randomPercent = ServerUtils.RandomNumber(100);
                    if (randomPercent < 60)
                    {
                        var item = ItemCache.GetItemDefault(190);
                        var listGiayThan = new List<short>(){563,565,567};
                        randomPercent = ServerUtils.RandomNumber(100); 
                        if (randomPercent <= 15) //giày thần 5 sao
                        {
                            item = ItemCache.GetItemDefault(listGiayThan[ServerUtils.RandomNumber(listGiayThan.Count)]);
                            var soSaoGiayThan = ServerUtils.RandomNumber(1, 5);
                            item.Options.Add(new OptionItem()
                            {
                                Id = 107,
                                Param = soSaoGiayThan
                            });
                        }
                        else if (randomPercent <= (15 + 10)) //giày thần 0 sao
                        {
                            item = ItemCache.GetItemDefault(listGiayThan[ServerUtils.RandomNumber(listGiayThan.Count)]);
                        }
                        else //Ngọc rồng 
                        {
                            if (ServerUtils.RandomNumber(100) < 40) //2s
                            {
                                item = ItemCache.GetItemDefault(15);
                            }
                            else //3s 
                            {
                                item = ItemCache.GetItemDefault(16);
                            }
                        }

                        if (randomPercent < 50)
                        {
                            var itemNhan = ItemCache.GetItemDefault(992);
                            var nhanthoikhong = new ItemMap(playerKillId, itemNhan);
                            nhanthoikhong.X = Boss.InfoChar.X;
                            nhanthoikhong.Y = Boss.InfoChar.Y;
                            Boss.Zone.ZoneHandler.LeaveItemMap(nhanthoikhong);
                        }

                        var itemMap = new ItemMap(playerKillId, item);
                        itemMap.X = Boss.InfoChar.X;
                        itemMap.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    }
                    
                    // Rớt vàng 100tr
                    for (int i = 0; i < 10; i++)
                    {
                        var item = ItemCache.GetItemDefault(190);
                        item.Quantity = ServerUtils.RandomNumber(5000000, 10000000);
                        var itemMap = new ItemMap(playerKillId, item);
                        itemMap.X = (short)(Boss.InfoChar.X + ServerUtils.RandomNumber(-10, 10));
                        itemMap.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    }
                    break;
                }
                case 4://fide 1
                {
                    var phanTramRoiDo = ServerUtils.RandomNumber(0, 100);
                    if (phanTramRoiDo <= 30)
                    {
                        var item = ItemCache.GetItemDefault((((short)ServerUtils.RandomNumber(16,17))));
                        var itemMap = new ItemMap(playerKillId, item);
                        itemMap.X = Boss.InfoChar.X;
                        itemMap.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    }
                    // Rớt vàng 5tr->10tr
                    for (int i = 0; i < 10; i++)
                    {
                        var item = ItemCache.GetItemDefault(190);
                        item.Quantity = ServerUtils.RandomNumber(500000, 1000000);
                        var itemMap = new ItemMap(playerKillId, item);
                        itemMap.X = (short)(Boss.InfoChar.X + ServerUtils.RandomNumber(-20, 20));
                        itemMap.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    }
                    break;
                }
                case 5://fide 1 2
                {
                    var phanTramRoiDo = ServerUtils.RandomNumber(0, 100);
                    if (phanTramRoiDo <= 40)
                    {
                        var item = ItemCache.GetItemDefault(16);
                        var itemMap = new ItemMap(playerKillId, item);
                        itemMap.X = Boss.InfoChar.X;
                        itemMap.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    }
                    // Rớt vàng 10tr-20tr
                    for (int i = 0; i < 10; i++)
                    {
                        var item = ItemCache.GetItemDefault(190);
                        item.Quantity = ServerUtils.RandomNumber(1000000, 2000000);
                        var itemMap = new ItemMap(playerKillId, item);
                        itemMap.X = (short)(Boss.InfoChar.X + ServerUtils.RandomNumber(-20, 20));
                        itemMap.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    }
                    break;
                }
                case 6://fide 3
                {
                    var doJean = ItemCache.GetItemDefault(DataCache.ListDoHiem[ServerUtils.RandomNumber(DataCache.ListDoHiem.Count)]);
                    var soSao = ServerUtils.RandomNumber(0, 3);
                    if (soSao > 0)
                    {
                        doJean.Options.Add(new OptionItem()
                        {
                            Id = 107,
                            Param = soSao
                        });
                    }
                    var itemMapJean = new ItemMap(playerKillId, doJean);
                    itemMapJean.X = Boss.InfoChar.X;
                    itemMapJean.Y = Boss.InfoChar.Y;
                    Boss.Zone.ZoneHandler.LeaveItemMap(itemMapJean);
                    // Rớt vàng 20-30
                    for (int i = 0; i < 10; i++)
                    {
                        var item = ItemCache.GetItemDefault(190);
                        item.Quantity = ServerUtils.RandomNumber(2000000, 3000000);
                        var itemMap = new ItemMap(playerKillId, item);
                        itemMap.X = (short)(Boss.InfoChar.X + ServerUtils.RandomNumber(-10, 10));
                        itemMap.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    }
                    break;
                }
                case 7:
                case 8://xên 01, 02
                {
                    var phanTramRoiDo = ServerUtils.RandomNumber(0, 100);
                    if (phanTramRoiDo <= 50)
                    {
                        var item = ItemCache.GetItemDefault(((short)ServerUtils.RandomNumber(15,16)));
                        var itemMap = new ItemMap(playerKillId, item);
                        itemMap.X = Boss.InfoChar.X;
                        itemMap.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        var item = ItemCache.GetItemDefault(190);
                        item.Quantity = ServerUtils.RandomNumber(1000000, 2000000);
                        var itemMap = new ItemMap(playerKillId, item);
                        itemMap.X = (short)(Boss.InfoChar.X + ServerUtils.RandomNumber(-10, 10));
                        itemMap.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    }
                    break;
                }
                case 9:
                {
                    var randomPercent = ServerUtils.RandomNumber(100);
                    if (randomPercent < 70)
                    {
                        var item = ItemCache.GetItemDefault(190);
                        var listQuanThan = new List<short>(){556,558,560};
                        var listAoThan = new List<short>(){555,557,559};
                        randomPercent = ServerUtils.RandomNumber(100); 
                        if (randomPercent <= 15) //quan thần 5 sao
                        {
                            item = ItemCache.GetItemDefault(listQuanThan[ServerUtils.RandomNumber(listQuanThan.Count)]);
                        }
                        else if (randomPercent <= 30) //quan thần 5 sao
                        {
                            item = ItemCache.GetItemDefault(listAoThan[ServerUtils.RandomNumber(listAoThan.Count)]);
                        }
                        else //Ngọc rồng 
                        {
                            if (ServerUtils.RandomNumber(100) < 40) //2s
                            {
                                item = ItemCache.GetItemDefault(15);
                            }
                            else //3s 
                            {
                                item = ItemCache.GetItemDefault(16);
                            }
                        }

                        var itemMap = new ItemMap(playerKillId, item);
                        itemMap.X = Boss.InfoChar.X;
                        itemMap.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        var item = ItemCache.GetItemDefault(190);
                        item.Quantity = ServerUtils.RandomNumber(1000000, 5000000);
                        var itemMap = new ItemMap(playerKillId, item);
                        itemMap.X = (short)(Boss.InfoChar.X + ServerUtils.RandomNumber(-10, 10));
                        itemMap.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    }
                    break;
                }
                case 10://Cooler 01
                {
                    var item = ItemCache.GetItemDefault((short)ServerUtils.RandomNumber(15,16));
                    var itemMap = new ItemMap(playerKillId, item);
                    itemMap.X = Boss.InfoChar.X;
                    itemMap.Y = Boss.InfoChar.Y;
                    Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    break;
                }
                case 11://Cooler 02
                {
                    var item = ItemCache.GetItemDefault(15);
                    var itemMap = new ItemMap(playerKillId, item);
                    itemMap.X = Boss.InfoChar.X;
                    itemMap.Y = Boss.InfoChar.Y;
                    Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);

                    var randomPercent = ServerUtils.RandomNumber(100);
                    if (randomPercent < 20)
                    {
                        var itemDoThan = ItemCache.GetItemDefault(DataCache.ListDoThanLinh[ServerUtils.RandomNumber(DataCache.ListDoThanLinh.Count)]);
                        var itemMapTL = new ItemMap(playerKillId, itemDoThan);
                        itemMapTL.X = Boss.InfoChar.X;
                        itemMapTL.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMapTL);
                    }
                    else if (randomPercent <= 50)
                    {
                        var doJean = ItemCache.GetItemDefault(DataCache.ListDoHiem[ServerUtils.RandomNumber(DataCache.ListDoHiem.Count)]);
                        var soSao = ServerUtils.RandomNumber(0, 5);
                        if (soSao > 0)
                        {
                            doJean.Options.Add(new OptionItem()
                            {
                                Id = 107,
                                Param = soSao
                            });
                        }
                        var itemMapJean = new ItemMap(playerKillId, doJean);
                        itemMapJean.X = Boss.InfoChar.X;
                        itemMapJean.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMapJean);
                    }
                    break;
                }
                case 12://tho phe co
                {
                    var item = ItemCache.GetItemDefault(670);
                    var itemMap = new ItemMap(-1, item);
                    itemMap.X = Boss.InfoChar.X;
                    itemMap.Y = Boss.InfoChar.Y;
                    Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    break;
                }
                case 13://tho đại ca
                {
                    var listItem = new List<short>(){462,886,887,888,889,891,462,886,887,888,889};
                    for (int i = 0; i < ServerUtils.RandomNumber(10, 20); i++)
                    {
                        var item = ItemCache.GetItemDefault(listItem[ServerUtils.RandomNumber(listItem.Count)]);
                        item.Quantity = ServerUtils.RandomNumber(1, 5);
                        var itemMap = new ItemMap(-1, item);
                        itemMap.X = (short)(Boss.InfoChar.X + ServerUtils.RandomNumber(-100, 100));
                        itemMap.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);
                    }
                    break;
                }
                case 14:
                {
                    var item = ItemCache.GetItemDefault(15);
                    var itemMap = new ItemMap(playerKillId, item);
                    itemMap.X = Boss.InfoChar.X;
                    itemMap.Y = Boss.InfoChar.Y;
                    Boss.Zone.ZoneHandler.LeaveItemMap(itemMap);

                    var randomPercent = ServerUtils.RandomNumber(100);
                    if (randomPercent <= 10)
                    {
                        // rơi trứng
                        var itemTrungLinhThu = ItemCache.GetItemDefault(1049);
                        var itemMapThucVat = new ItemMap(playerKillId, itemTrungLinhThu);
                        itemMapThucVat.X = Boss.InfoChar.X;
                        itemMapThucVat.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMapThucVat);
                    }
                    // else if (randomPercent <= 11) // rơi nhẫn
                    // {
                    //     var itemDoThan = ItemCache.GetItemDefault(561);
                    //     var itemMapThucVat = new ItemMap(playerKillId, itemDoThan);
                    //     itemMapThucVat.X = Boss.InfoChar.X;
                    //     itemMapThucVat.Y = Boss.InfoChar.Y;
                    //     Boss.Zone.ZoneHandler.LeaveItemMap(itemMapThucVat);
                    // }
                    else if (randomPercent <= 20) // rơi cải trang ngày
                    {
                        var timeServer = ServerUtils.CurrentTimeSecond();
                        var expireDay = ServerUtils.RandomNumber(1, 3);
                        var expireTime = timeServer + (expireDay*86400);
                        var itemCaiTrang = ItemCache.GetItemDefault(985);
                                    

                        itemCaiTrang.Options.Add(new OptionItem()
                        {
                            Id = 93,
                            Param = expireDay,
                        });
                        var optionHiden = itemCaiTrang.Options.FirstOrDefault(option => option.Id == 73);
                        
                        if (optionHiden != null) 
                        {
                            optionHiden.Param = expireTime;
                        }
                        else 
                        {
                            itemCaiTrang.Options.Add(new OptionItem()
                            {
                                Id = 73,
                                Param = expireTime,
                            });
                        }
                        
                        var itemMapThucVat = new ItemMap(playerKillId, itemCaiTrang);
                        itemMapThucVat.X = Boss.InfoChar.X;
                        itemMapThucVat.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMapThucVat);
                    }
                    else if (randomPercent <= 50)
                    {
                        var doJean = ItemCache.GetItemDefault(DataCache.ListDoHiem[ServerUtils.RandomNumber(DataCache.ListDoHiem.Count)]);
                        var soSao = ServerUtils.RandomNumber(0, 5);
                        if (soSao > 0)
                        {
                            doJean.Options.Add(new OptionItem()
                            {
                                Id = 107,
                                Param = soSao
                            });
                        }
                        var itemMapJean = new ItemMap(playerKillId, doJean);
                        itemMapJean.X = Boss.InfoChar.X;
                        itemMapJean.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMapJean);
                    }
                    else // hồn
                    {
                        var itemHonLinhThu = ItemCache.GetItemDefault(1048);
                        itemHonLinhThu.Quantity = ServerUtils.RandomNumber(5, 10);
                        var itemMapThucVat = new ItemMap(playerKillId, itemHonLinhThu);
                        itemMapThucVat.X = Boss.InfoChar.X;
                        itemMapThucVat.Y = Boss.InfoChar.Y;
                        Boss.Zone.ZoneHandler.LeaveItemMap(itemMapThucVat);
                    }
                    break;
                }
            }

        }

        public int GetParamItem(int id)
        {
            return Boss.ItemBody.Where(item => item != null).Select(item => item.Options.Where(option => option.Id == id).ToList()).Select(option => option.Sum(optionItem => optionItem.Param)).Sum();
        }

        public List<int> GetListParamItem(int id)
        {
            var param = new List<int>();
            foreach (var item in Boss.ItemBody.Where(item => item != null))
            {
                var option = item.Options.Where(option => option.Id == id).ToList();
                param.AddRange(option.Select(optionItem => optionItem.Param)); 
            }
            return param;
        }

        public void SetUpInfo()
        {
            SetHpFull();
            SetMpFull();
            SetDamageFull();
            SetDefenceFull();
            SetCritFull();
            SetSpeed();
            SetHpPlusFromDamage();
            SetMpPlusFromDamage();
            SetBuffMp1s();
            SetBuffHp5s();
            SetBuffHp10s();
            SetBuffHp30s();
        }

        public void SetHpFull()
        {
            var hp = Boss.InfoChar.OriginalHp;
            hp += GetParamItem(2) * 100;
            hp += GetParamItem(6);
            hp += GetParamItem(22) * 1000;
            hp += GetParamItem(48);
            GetListParamItem(77).ForEach(param => hp += hp*param/100);
            GetListParamItem(109).ForEach(param => hp -= hp*param/100);
            if (Boss.InfoSkill.Monkey.MonkeyId != 0) hp += hp * Boss.InfoSkill.Monkey.Hp / 100;
            if (Boss.InfoSkill.HuytSao.IsHuytSao) hp += hp * Boss.InfoSkill.HuytSao.Percent / 100;
            Boss.HpFull = hp;
        }

        public void SetMpFull()
        {
            var mp = Boss.InfoChar.OriginalMp;
            mp += GetParamItem(2) * 100;
            mp += GetParamItem(7);
            mp += GetParamItem(23) * 1000;
            mp += GetParamItem(48);
            GetListParamItem(103).ForEach(param => mp += mp*param/100);
            Boss.MpFull = mp;
        }

        public void SetDamageFull()
        {
            var damage = Boss.InfoChar.OriginalDamage;
            damage += GetParamItem(0);
            GetListParamItem(50).ForEach(param => damage += damage*param/100);
            GetListParamItem(147).ForEach(param => damage += damage*param/100);
            if (Boss.InfoSkill.Monkey.MonkeyId != 0) damage += damage * Boss.InfoSkill.Monkey.Damage / 100;
            Boss.DamageFull = damage;
        }

        public void SetDefenceFull()
        {
            var defence = Boss.InfoChar.OriginalDefence * 4;
            defence += GetParamItem(47);
            GetListParamItem(94).ForEach(param => defence += defence*param/100);
            Boss.DefenceFull = Math.Abs(defence);
        }

        public void SetCritFull()
        {
            int crtCal;
            if (Boss.InfoSkill.Monkey.MonkeyId != 0)
            {
                crtCal = 115;
            }
            else
            {
                crtCal = Boss.InfoChar.OriginalCrit;
                crtCal += GetParamItem(14);
            }
            Boss.CritFull = crtCal;
        }

        public void SetHpPlusFromDamage()
        {
            var hpPlus = GetParamItem(95);
            Boss.HpPlusFromDamage = hpPlus;
        }

        public void SetMpPlusFromDamage()
        {
            var mpPlus = GetParamItem(96);
            Boss.MpPlusFromDamage = mpPlus;
        }

        public void SetSpeed()
        {
            var speed = 10;
            if (Boss.InfoSkill.Monkey.MonkeyId != 0) speed = 10;
            var plus = speed * (GetParamItem(148) + GetParamItem(16)) / 100;
            switch (plus)
            {
                case <= 1:
                    speed+=1;
                    break;
                case > 1 and <= 2:
                    speed += 2;
                    break;
            }
            Boss.InfoChar.Speed = (sbyte)speed;
        }

        public void SetBuffHp30s()
        {
            var hpPlus = GetParamItem(27);
            Boss.Effect.BuffHp30S.Value = hpPlus;
            if (Boss.Effect.BuffHp30S.Time == -1)
            {
                Boss.Effect.BuffHp30S.Time = 30000 + ServerUtils.CurrentTimeMillis();
            }
            
        }

        public void SetBuffMp1s()
        {
            var mpPlus = (int)Boss.MpFull * GetParamItem(162)/100;
            Boss.Effect.BuffKi1s.Value = mpPlus;
            if (Boss.Effect.BuffKi1s.Time == -1)
            {
                Boss.Effect.BuffKi1s.Time = 1500 + ServerUtils.CurrentTimeMillis();
            }
        }
        
        public void SetBuffHp5s()
        {
            //TODO set buff 5s
        }

        public void SetBuffHp10s()
        {
            //TODO set buff 10s
        }

        public void MoveMap(short toX, short toY, int type = 0)
        {
            var timeServer = ServerUtils.CurrentTimeMillis();
            if(Boss.IsDontMove()) return;

            var compare = Math.Abs(Boss.InfoChar.X - toX);
            if (compare >= 50)
            {
                if (Boss.InfoChar.X < toX)
                {
                    Boss.InfoChar.X = compare switch
                    {
                        >= 150 => (short) (toX - 70),
                        _ => (short) (toX - 50)
                    };
                }
                else
                {
                    Boss.InfoChar.X = compare switch
                    {
                        >= 150 => (short) (toX + 70),
                        _ => (short) (toX + 50)
                    };
                }

                if (toY != Boss.InfoChar.Y)
                {
                    Boss.InfoChar.Y = toY;
                }

                SendZoneMessage(Service.PlayerMove(Boss.Id, Boss.InfoChar.X, Boss.InfoChar.Y));
                if (Boss.InfoSkill.MeTroi.IsMeTroi && Boss.InfoSkill.MeTroi.DelayStart <= timeServer)
                {
                    SkillHandler.RemoveTroi(Boss);
                }
            }
        }

        public void PlusHp(int hp)
        {
            lock (Boss.InfoChar)
            {
                if(Boss.InfoChar.IsDie) return;
                Boss.InfoChar.Hp += hp;
                if (Boss.InfoChar.Hp >= Boss.HpFull) Boss.InfoChar.Hp = Boss.HpFull;
            }
        }

        public void MineHp(long hp)
        {
            lock (Boss.InfoChar)
            {
                if(Boss.InfoChar.IsDie || hp <= 0) return;

                if (hp > Boss.InfoChar.Hp)
                {
                    Boss.InfoChar.Hp = 0;
                }
                else 
                {
                    Boss.InfoChar.Hp -= hp;
                }

                if (Boss.InfoChar.Hp <= 0)
                {
                    Boss.InfoChar.IsDie = true;
                    Boss.InfoChar.Hp = 0;
                }
            }
        }

        public void PlusMp(int mp)
        {
            lock (Boss.InfoChar)
            {
                if(Boss.InfoChar.IsDie) return;
                Boss.InfoChar.Mp += mp;
                if (Boss.InfoChar.Mp >= Boss.MpFull) Boss.InfoChar.Mp = Boss.MpFull;
            }
        }

        public void MineMp(int mp)
        {
            // lock (Boss.InfoChar)
            // {
            //     if(Boss.InfoChar.IsDie) return;
            //     Boss.InfoChar.Mp -= mp;
            //     if (Boss.InfoChar.Mp <= 0) Boss.InfoChar.Mp = 0;
            // }
        }

        public void PlusStamina(int stamina)
        {
            lock (Boss.InfoChar)
            {
                Boss.InfoChar.Stamina += (short)stamina;
                if (Boss.InfoChar.Stamina > 1250) Boss.InfoChar.Stamina = 1250;
            }
        }

        public void MineStamina(int stamina)
        {
            // lock (Boss.InfoChar)
            // {
            //     Boss.InfoChar.Stamina -= (short)stamina;
            //     if (Boss.InfoChar.Stamina <= 0) Boss.InfoChar.Stamina = 0;
            // }
        }

        public void PlusPower(long power)
        {
            // Ignore
        }

        public void PlusPotential(long potential)
        {
            // Ignore
        }

        public Model.Item.Item RemoveItemBody(int index)
        {
            Model.Item.Item item;
            lock (Boss.ItemBody)
            {
                item = Boss.ItemBody[index];
                if (item == null) return null;
                Boss.ItemBody[index] = null;
                UpdateInfo();
            }
            return item;
        }

        public void AddItemToBody(Model.Item.Item item, int index)
        {
            if (item == null) return;
            item.IndexUI = index;
            Boss.ItemBody[index] = item;
        }

        public void RemoveMonsterMe()
        {
            var skillEgg = Boss.InfoSkill.Egg;
            if (skillEgg.Monster is {IsDie: true})
            {
                SendZoneMessage(Service.UpdateMonsterMe7(skillEgg.Monster.Id));
                Boss.Zone.ZoneHandler.RemoveMonsterMe(skillEgg.Monster.Id);
                SkillHandler.RemoveMonsterPet(Boss);
            }
        }

        public void PlusPoint(IMonster monster, int damage)
        {
        }

        public void PlusPoint(long power, long potential, bool isAll)
        {
        }

        public void LeaveFromDead(bool isHeal = false)
        {
            lock (Boss)
            {
                BossRunTime.Gi().BossDie(Boss.Id);
                Boss.Zone.ZoneHandler.RemoveBoss(Boss);
                UpdateInfo();
                Boss.InfoChar.IsDie = false;
                Boss.InfoChar.Hp = Boss.HpFull;
                Boss.InfoChar.Mp = Boss.MpFull;
                // SendZoneMessage(Service.ReturnPointMap(Boss));
                // SendZoneMessage(Service.PlayerLoadLive(Boss));
                Boss = null;
                Dispose();
            }
        }

        public void RemoveSkill(long timeServer, bool globalReset = false)
        {
            var infoSkill = Boss.InfoSkill;
            if ((infoSkill.TaiTaoNangLuong.IsTTNL &&
                 infoSkill.TaiTaoNangLuong.DelayTTNL <= timeServer) || globalReset)
            {
                SkillHandler.RemoveTTNL(Boss);
            }

            if (infoSkill.Monkey.MonkeyId == 1 && infoSkill.Monkey.TimeMonkey <= timeServer || globalReset)
            {
                SkillHandler.HandleMonkey(Boss,false);
            }

            if (infoSkill.Protect.IsProtect && infoSkill.Protect.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveProtect(Boss);
            }

            if (infoSkill.HuytSao.IsHuytSao && infoSkill.HuytSao.Time <= timeServer)
            {
                SkillHandler.RemoveHuytSao(Boss);
            }

            if (infoSkill.MeTroi.IsMeTroi)
            {
                var monsterMap = infoSkill.MeTroi.Monster;
                var charTemp = infoSkill.MeTroi.Character;
                if (globalReset)
                {
                    SkillHandler.RemoveTroi(Boss);
                }
                if (monsterMap is {IsDie: true})
                {
                    SkillHandler.RemoveTroi(Boss);
                }
                else if (charTemp != null && charTemp.InfoChar.IsDie)
                {
                    SkillHandler.RemoveTroi(Boss);
                }
                else if (infoSkill.MeTroi.TimeTroi <= timeServer || monsterMap is {IsDie: true} || charTemp != null && charTemp.InfoChar.IsDie)
                {
                    SkillHandler.RemoveTroi(Boss);
                }
            }

            if (infoSkill.PlayerTroi.IsPlayerTroi || globalReset)
            {
                if (globalReset && infoSkill.PlayerTroi.IsPlayerTroi)
                {
                    List<int> PlayerID = new List<int>();
                    
                    foreach (var id in new List<int>(infoSkill.PlayerTroi.PlayerId))
                    {
                        SkillHandler.RemoveTroi(Boss.Zone.ZoneHandler.GetCharacter(id));
                    }
                    // infoSkill.PlayerTroi.PlayerId.ForEach(i => SkillHandler.RemoveTroi(Boss.Zone.ZoneHandler.GetCharacter(i)));
                }
            }

            if (infoSkill.DichChuyen.IsStun && infoSkill.DichChuyen.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveDichChuyen(Boss);
            }

            if (infoSkill.ThaiDuongHanSan.IsStun && infoSkill.ThaiDuongHanSan.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveThaiDuongHanSan(Boss);
            }

            if (infoSkill.ThoiMien.IsThoiMien && infoSkill.ThoiMien.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveThoiMien(Boss);
            }

            if (infoSkill.Socola.IsSocola && infoSkill.Socola.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveSocola(Boss);
            }
        }

        public void UpdateEffect(long timeServer)
        {
            var effect = Boss.Effect;
            if (effect.BuffHp30S.Value > 0 && effect.BuffHp30S.Time <= timeServer && Boss.InfoChar.Hp < Boss.HpFull)
            {
                PlusHp(effect.BuffHp30S.Value);
                SendZoneMessage(Service.PlayerLevel(Boss));
                effect.BuffHp30S.Time = 30000 + timeServer;
            }

            if (effect.BuffKi1s.Value > 0 && effect.BuffKi1s.Time <= timeServer && Boss.InfoChar.Mp < Boss.MpFull)
            {
                PlusMp(effect.BuffKi1s.Value);
                effect.BuffKi1s.Time = 1500 + timeServer;
            }
        }

        public void RemoveTroi(int charId)
        {
            var infoSkill = Boss.InfoSkill.PlayerTroi;
            if (infoSkill.IsPlayerTroi)
            {
                infoSkill.PlayerId.RemoveAll(i => i == charId);
                if (infoSkill.PlayerId.Count <= 0)
                {
                    infoSkill.IsPlayerTroi = false;
                    infoSkill.TimeTroi = -1;
                    infoSkill.PlayerId.Clear();
                    SendZoneMessage(Service.SkillEffectPlayer(charId, Boss.Id, 2, 32));
                }
            }
        }

        #region Ignored Function
        public void UpdateMask(long timeServer)
        {

        }

        public void UpdateAutoPlay(long timeServer)
        {
            
        }

        public void UpdateLuyenTap()
        {
        
        }

        public void SetPlayer(Player player)
        {
            //Set player
        }

        public void SendMessage(Message message)
        {
            //ignore
        }
        
        public void SendMeMessage(Message message)
        {
            //ignore
        }
        public void HandleJoinMap(Zone zone)
        {
            //Boss join map
        }

        public void BagSort()
        {
            //ignore
        }

        public void BoxSort()
        {
            //ignore
        }
        public void Upindex(int index)
        {
            //ignore
        }
        public bool AddItemToBag(bool isUpToUp, Model.Item.Item item, string reason = "")
        {
            //ignore
            return false;
        }

        public bool AddItemToBox(bool isUpToUp, Model.Item.Item item)
        {
            //ignore
            return false;
        }
        
        public void ClearTest()
        {
            //Clear test
        }
        
        public void DropItemBody(int index)
        {
            //ignore
        }

        public void DropItemBag(int index)
        {
            //ignore
        }

        public void PickItemMap(short id)
        {
            //ignore
        }

        public void UpdateMountId()
        {
            //ignore
        }
        public void UpdatePhukien()
        {
            //ignore
        }
        public Model.Item.Item GetItemBagByIndex(int index)
        {
            //ignore
            return null;
        }

        public Model.Item.Item GetItemBagById(int id)
        {
            //ignore
            return null;
        }

        public Model.Item.Item GetItemBoxByIndex(int index)
        {
            //ignore
            return null;
        }
        public Model.Item.Item GetItemLuckyBoxByIndex(int index)
        {
            //ignore
            return null;
        }
        public Model.Item.Item GetItemBoxById(int id)
        {
            //ignore
            return null;
        }

        
        public void BackHome()
        {
            //Ignore
        }
        
        public void RemoveItemBagById(short id, int quantity, string reason = "")
        {
            //ignore
        }

        public void RemoveItemBagByIndex(int index, int quantity, bool reset = true, string reason = "")
        {
            //ignore
        }

        public void RemoveItemBoxByIndex(int index, int quantity, bool reset = true)
        {
            //ignore
        }

        public Model.Item.Item RemoveItemBag(int index, bool isReset = true, string reason = "")
        {
            return null;
        }

                
        
        public Model.Item.Item ItemBagNotMaxQuantity(short id)
        {
            //ignore
            return null;
        }
        
        public Model.Item.Item RemoveItemBox(int index, bool isReset = true)
        {
            return null;
        }
        public Model.Item.Item RemoveItemLuckyBox(int index, bool isReset = true)
        {
            return null;
        }

        public void SetUpFriend()
        {
            //Ignore
        }

        #endregion
    }
}
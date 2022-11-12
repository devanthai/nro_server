using System;
using System.Collections.Generic;
using System.Linq;
using Linq.Extras;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Handlers.Skill;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Monster;
using NRO_Server.Application.IO;
using NRO_Server.Application.Main;
using NRO_Server.Application.Handlers.Item;
using NRO_Server.Application.Threading;
using NRO_Server.DatabaseManager;
using NRO_Server.DatabaseManager.Player;
using NRO_Server.Model;
using NRO_Server.Model.Info;
using NRO_Server.Model.Character;
using NRO_Server.Model.Map;
using NRO_Server.Model.SkillCharacter;
using Org.BouncyCastle.Math.Field;
using static System.GC;

namespace NRO_Server.Application.Handlers.Character
{
    public class DiscipleHandler : ICharacterHandler
    {
        public Disciple Disciple { get; set; }

        public DiscipleHandler(Disciple disciple)
        {
            Disciple = disciple;
        }

        public void Dispose()
        {
            SuppressFinalize(this);
        }

        public void SendZoneMessage(Message message)
        {
            Disciple?.Zone?.ZoneHandler.SendMessage(message);
        }

        public void Update()
        {
            lock (Disciple)
            {
                if(Disciple.Status >= 3) return;
                var timeServer = ServerUtils.CurrentTimeMillis();
                if (Disciple.InfoDelayDisciple.Fire <= timeServer)
                {
                    Disciple.IsFire = true;
                }
                RemoveSkill(timeServer);
                if ((Disciple.InfoChar.IsDie || Disciple.InfoChar.Hp == 0) && Disciple.InfoDelayDisciple.LeaveDead <= timeServer)
                {
                    Disciple.InfoDelayDisciple.LeaveDead = -1;
                    if (Disciple.Zone.Id != Disciple.Character.Zone.Id)
                    {
                        SetUpPosition(isRandom:true);
                        SendChatForSp(TextServer.gI().HELLO_SP);
                    }
                    LeaveFromDead();
                }
                else if (!Disciple.InfoChar.IsDie)
                {
                    AutoDisciple(timeServer);
                    UpdateMask(timeServer);
                    UpdateEffect(timeServer);
                    AutoPlusPoint(timeServer);
                    AutoAddSkill();
                }

                if (Disciple.InfoDelayDisciple.SaveData <= timeServer)
                {
                    DiscipleDB.Update(Disciple);
                    Disciple.InfoDelayDisciple.SaveData = timeServer + 600000;
                }
            }
        }

        private void BuaDeTu()
        {
            // Kiểm tra xem có bùa đệ tử không
            try
            {
                if (Disciple.Character.InfoMore.BuaDeTu)
                {
                    if (Disciple.Character.InfoMore.BuaDeTuTime > ServerUtils.CurrentTimeMillis())
                    {
                        // Tự ăn đậu
                        // Kiểm tra sư phụ có đậu hay không
                        foreach(var item in Disciple.Character.ItemBag.ToList())
                        {
                            if (DataCache.IdDauThan.Contains(item.Id))
                            {
                                ItemHandler.UsePea(Disciple.Character, item, 0);
                                SendChatForSp(TextServer.gI().THANKS_FOR_SAVE_ME_DIS); 
                                return;
                            }
                        }
                    }
                    else 
                    {
                        Disciple.Character.InfoMore.BuaDeTu = false;
                    }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error BuaDeTu in DiscipleHandler.cs: {e.Message} \n {e.StackTrace}", e);
                return;
            }
            
        }

        private void AutoDisciple(long timeServer)
        {
            if(Disciple.IsDontMove()) return;
            switch (Disciple.Status)
            {
                //Đi theo
                case 0:
                {
                    if (Math.Abs(Disciple.InfoChar.X - Disciple.Character.InfoChar.X) >= 60)
                    {
                        SetUpPosition(isRandom:true);
                        SendZoneMessage(Service.PlayerMove(Disciple.Id, Disciple.InfoChar.X, Disciple.InfoChar.Y));
                    }
                    AutoMoveMap(timeServer);
                    break;
                }
                //Bảo vệ
                case 1:
                {
                    if (Disciple.MonsterFocus == null && Math.Abs(Disciple.InfoChar.X - Disciple.Character.InfoChar.X) >= 60)
                    {
                        AutoMoveMap(timeServer, true);
                    }
                    else if (Disciple.MonsterFocus != null && Math.Abs(Disciple.InfoChar.X - Disciple.Character.InfoChar.X) >= 130)
                    {
                        Disciple.MonsterFocus = null;
                        AutoMoveMap(timeServer, true);
                    }
                    else 
                    {
                        if (Disciple.InfoChar.Stamina <= 0)
                        {
                            SendChatForSp(TextServer.gI().NOT_ENOUGH_STAMINA_DISCIPLE);
                            BuaDeTu();
                            if (Math.Abs(Disciple.InfoChar.X - Disciple.Character.InfoChar.X) >= 60)
                            {
                                AutoMoveMap(timeServer);
                            }
                        }
                        else if (Disciple.InfoChar.Mp <= (Disciple.InfoChar.OriginalMp*5/100))
                        {
                            SendChatForSp(TextServer.gI().NOT_ENOUGH_KI_DISCIPLE);
                            BuaDeTu();
                            if (Math.Abs(Disciple.InfoChar.X - Disciple.Character.InfoChar.X) >= 60)
                            {
                                AutoMoveMap(timeServer);
                            }
                        }
                        else
                        {
                            HandleUseSkill();
                        }
                    }
                    break;
                }
                // Tấn công
                case 2:
                {
                    if (Disciple.InfoChar.Stamina <= 0)
                    {
                        SendChatForSp(TextServer.gI().NOT_ENOUGH_STAMINA_DISCIPLE);
                        AutoMoveMap(timeServer);
                        BuaDeTu();
                    }
                    else if (Disciple.InfoChar.Mp <= (Disciple.InfoChar.OriginalMp*5/100))
                    {
                        SendChatForSp(TextServer.gI().NOT_ENOUGH_KI_DISCIPLE);
                        AutoMoveMap(timeServer);
                        BuaDeTu();
                    }
                    else
                    {
                        HandleUseSkill();
                    }
                    
                    break;
                }
            }
        }

        private void HandleUseSkill(bool isAuto = true, int charId = -1, int modId = -1)
        {
            if (!Disciple.IsFire) return;
            if (isAuto)
            {
                var infoSkill = Disciple.InfoSkill;
                var timeServer = ServerUtils.CurrentTimeMillis();
                // Tái tạo năng lượng
                // Đang tái tạo năng lượng sẽ không bị xóa
                if (infoSkill.TaiTaoNangLuong.IsTTNL &&
                    infoSkill.TaiTaoNangLuong.DelayTTNL > timeServer)
                {
                    if (Disciple.InfoDelayDisciple.TTNL <= timeServer)
                    {
                        // Xử lý tái tạo năng lượng
                        SkillHandler.SkillNotFocus(Disciple, 8, 2);
                        Disciple.InfoDelayDisciple.TTNL = timeServer + 2000;
                    }
                    return;
                }

                // Tìm quái
                var monster = Disciple.MonsterFocus;

                var checkSize = 220;
                
                if (Disciple.Status == 2)
                {
                    checkSize = 800;
                    if (monster == null || monster.IsDie || Math.Abs(monster.X - Disciple.InfoChar.X) > checkSize || Math.Abs(monster.Y - Disciple.InfoChar.Y) > 600)
                    {
                        Disciple.MonsterFocus = monster = Disciple.Zone.MonsterMaps.FirstOrDefault(m =>
                            m is {IsDie: false} && Math.Abs(m.X - Disciple.InfoChar.X) <= checkSize && Math.Abs(m.Y - Disciple.InfoChar.Y) <= 600);
                    }
                }
                else if (Disciple.Status == 1)
                {
                    if (monster == null || monster.IsDie || Math.Abs(monster.Y - Disciple.InfoChar.Y) > 600)
                    {
                        var findMonster = Disciple.Zone.MonsterMaps.Where(m =>
                            m is {IsDie: false} && Math.Abs(m.X - Disciple.InfoChar.X) <= checkSize && Math.Abs(m.Y - Disciple.InfoChar.Y) <= 600);
                        if (findMonster != null && findMonster.Count() > 0)
                        {
                            Disciple.MonsterFocus = monster = findMonster.MinBy(m => Math.Abs(m.X - Disciple.InfoChar.X));
                        }
                        else 
                        {
                            Disciple.MonsterFocus = monster = null;
                        }
                    }
                }

                if (monster == null)
                {
                    return;
                }

                SkillCharacter skillChar = null;
                var dX = 0;
                var dY = 0;
                try {
                    // Kiểm tra khoản cách giữa quái và đệ
                    var monsterDistance = Math.Abs(monster.X - Disciple.InfoChar.X);
                    var monsterDistanceY = Math.Abs(monster.Y - Disciple.InfoChar.Y);
                    // for skill từ trên xuống dưới
                    for (int i = Disciple.Skills.Count - 1; i >= 0; i--)
                    {
                        skillChar = Disciple.Skills[i];
                        
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
                        var manaChar = Disciple.InfoChar.Mp;
                        manaUse = manaUseType switch
                        {
                            1 => manaUse * (int) Disciple.MpFull / 100,
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
                            if (skillChar.Id == 9)
                            {
                                if (ServerUtils.RandomNumber(0, 100) < 80) continue;
                                var hpMine = Disciple.HpFull / 10;
                                if (hpMine >= Disciple.InfoChar.Hp)
                                {
                                    skillChar = null;
                                    continue;
                                }
                            
                            }
                            break;
                        }
                        // Nếu skill 2 khoản cách lớn hơn >36 thì lấy
                        else if ((i == 1 || i == 0) && Disciple.Status == 2)
                        {
                            break;
                        }
                        else if (Disciple.Status == 1)
                        {
                            if (i == 1 && ((monsterDistance > 44 && monsterDistance <= dX) || (monsterDistanceY > 44 && monsterDistanceY <= dY)))
                            {
                                break;
                            }
                            else if (i == 0)
                            {
                                if ((monsterDistance <= dX && monsterDistanceY <= dY) || Disciple.Skills.Count == 1)
                                {
                                    // Bảo vệ đủ khoản cách đấm hoặc chỉ có 1 chiêu
                                    break;
                                }
                                else 
                                {
                                    skillChar = null;
                                    Disciple.MonsterFocus = null;
                                    break;
                                }
                            }
                        }
                        // Nếu skill 1
                        // Nếu chiêu là chiêu đánh, chưởng, hoặc liên hoàn
                        // Kiểm tra khoản cách giữa quái
                        
                        // Dùng chiêu 2 trước
                        
                        // Kiểm tra nếu gần quá thì dùng chiêu 1
                        // Random nhảy tới dứt
                        // Không thì thấp nhất bởi cooldown
                        // Nếu chiêu là các chiêu biến hình, ttnl, khiên năng lượng
                    }

                    if (skillChar == null)
                    {
                        return;
                    }
                    // skillChar = Disciple.Skills.Where(s => s.CoolDown <= ServerUtils.CurrentTimeMillis()).MinBy(s => s.CoolDown);
                    
                    if (skillChar.Id == 8)
                    {
                        // Bắt đầu dùng tái tạo năng lượng
                        SkillHandler.SkillNotFocus(Disciple, skillChar.Id, 1);
                        return;
                    }

                    // Thái dương hạ sang
                    if (skillChar.Id == 6)
                    {
                        SkillHandler.SkillNotFocus(Disciple, skillChar.Id, 0);
                        return;
                    }

                    // Khiên năng lượng
                    if (skillChar.Id == 19)
                    {
                        SkillHandler.SkillNotFocus(Disciple, skillChar.Id, 9);
                        return;
                    }
                    //Đẻ trứng
                    if (skillChar.Id == 12)
                    {
                        SkillHandler.SkillNotFocus(Disciple, skillChar.Id, 8);
                        return;
                    }

                    //Biến khỉ
                    if (skillChar.Id == 13)
                    {
                        SkillHandler.SkillNotFocus(Disciple, skillChar.Id, 6);
                        return;
                    }

                    if (monster is {IsDie: false})
                    {
                        if (skillChar.Id == 0 || skillChar.Id == 2 || skillChar.Id == 4 || skillChar.Id == 9 || Disciple.Status == 2)
                        {
                            if (monster.X > Disciple.InfoChar.X)
                            {
                                Disciple.InfoChar.X = (short)(monster.X - dX);
                            }
                            else
                            {
                                Disciple.InfoChar.X = (short)(monster.X + dX);
                            }

                            Disciple.InfoChar.Y = monster.Y;
                            SendZoneMessage(Service.PlayerMove(Disciple.Id, Disciple.InfoChar.X, Disciple.InfoChar.Y));
                        }
                        //SendZoneMessage(Service.SendPos(Disciple, 0));
                        SkillHandler.AttackMonster(Disciple, skillChar, monster.IdMap);
                    }
                }
                catch (Exception)
                {
                    // Ignore
                    return;
                }
            }
            else
            {
                if (charId != -1)
                {

                }
                else if(modId != -1)
                {
                    
                }
            }
            
        }

        private void AutoMoveMap(long timeServer, bool isForce = false)
        {
            if ((Disciple.IsFire && Disciple.InfoDelayDisciple.AutoMove <= timeServer) || isForce)
            {
                Disciple.InfoChar.X = (short)ServerUtils.RandomNumber(Disciple.Character.InfoChar.X - 30,
                    Disciple.Character.InfoChar.X + 30);
                SendZoneMessage(Service.PlayerMove(Disciple.Id, Disciple.InfoChar.X, Disciple.InfoChar.Y));
                if (Disciple.InfoSkill.MeTroi.IsMeTroi &&
                    Disciple.InfoSkill.MeTroi.DelayStart <= timeServer)
                {
                    SkillHandler.RemoveTroi(Disciple);
                }
                Disciple.InfoDelayDisciple.AutoMove = timeServer + ServerUtils.RandomNumber(10000, 20000);
            }
        }


        private void AutoPlusPoint(long timeServer)
        {
            if (Disciple.InfoDelayDisciple.AutoPlusPoint <= timeServer && Disciple.PlusPoint.PointNext <= Disciple.InfoChar.Potential)
            {
                if (Disciple.PlusPoint.CheckPlusPoint(Disciple))
                {  
                    PlusPointOrignal(Disciple.PlusPoint.TypeNext);
                    SetUpInfo();
                    Disciple.InfoDelayDisciple.AutoPlusPoint = 1000 + timeServer;
                }
                Disciple.PlusPoint.RandomPoint(Disciple);
            }
        }

        private void PlusPointOrignal(int type)
        {
            lock (Disciple.InfoChar)
            {
                var infoChar = Disciple.InfoChar;
                switch (type)
                {
                    case 0:
                    {
                        var x10 = 10 * (2 * (infoChar.OriginalHp + 1000) + 180) / 2;

                        if (infoChar.Potential >= x10)
                        {
                            infoChar.OriginalHp += (infoChar.HpFrom1000*10);
                            infoChar.Potential -= x10;
                        }
                        else 
                        {
                            infoChar.OriginalHp += infoChar.HpFrom1000;
                            infoChar.Potential -= Disciple.PlusPoint.PointNext;
                        }
                        break;
                    }
                    case 1:
                    {
                        var x10 = 10 * (2 * (infoChar.OriginalMp + 1000) + 180) / 2;
                        if (infoChar.Potential >= x10)
                        {
                            infoChar.OriginalMp += (infoChar.MpFrom1000*10);
                            infoChar.Potential -= x10;
                        }
                        else 
                        {
                            infoChar.OriginalMp += infoChar.MpFrom1000;
                            infoChar.Potential -= Disciple.PlusPoint.PointNext;
                        }
                        break;
                    }
                    case 2:
                    {
                        var x10 = 10 * (2 * infoChar.OriginalDamage + 9) / 2 * 100;
                        if (infoChar.Potential >= x10)
                        {

                            infoChar.OriginalDamage += (infoChar.DamageFrom1000*10);
                            infoChar.Potential -= x10;
                        }
                        else 
                        {
                            infoChar.OriginalDamage += infoChar.DamageFrom1000;
                            infoChar.Potential -= Disciple.PlusPoint.PointNext;
                        }
                        
                        break; 
                    }
                    case 3:
                    {
                        infoChar.OriginalDefence += 1;
                        infoChar.Potential -= Disciple.PlusPoint.PointNext;
                        break; 
                    }
                    case 4:
                    {
                        infoChar.OriginalCrit += 1;
                        infoChar.Potential -= Disciple.PlusPoint.PointNext;
                        break; 
                    }
                }
            }
        }

        private void AutoAddSkill()
        {
            switch (Disciple.InfoChar.Power)
            {
                case >= 150000000 when Disciple.Skills.Count < 2:
                {
                    var randomSkill = DataCache.IdSkillDisciple2[ServerUtils.RandomNumber(DataCache.IdSkillDisciple2.Count)];
                    Disciple.Skills.Add(new SkillCharacter()
                    {
                        Id = randomSkill,
                        SkillId = Disciple.GetSkillId(randomSkill),
                        Point = 1,
                    });
                    break;
                }
                case >= 1500000000 when Disciple.Skills.Count < 3:
                {
                    var randomSkill = DataCache.IdSkillDisciple3[ServerUtils.RandomNumber(DataCache.IdSkillDisciple3.Count)];
                    Disciple.Skills.Add(new SkillCharacter()
                    {
                        Id = randomSkill,
                        SkillId = Disciple.GetSkillId(randomSkill),
                        Point = 1,
                    });
                    break;
                }
                case >= 20000000000 when Disciple.Skills.Count < 4:
                {
                    var randomSkill = DataCache.IdSkillDisciple4[ServerUtils.RandomNumber(DataCache.IdSkillDisciple4.Count)];
                    Disciple.Skills.Add(new SkillCharacter()
                    {
                        Id = randomSkill,
                        SkillId = Disciple.GetSkillId(randomSkill),
                        Point = 1,
                    });
                    break;
                }
                    
            }
        }

        private void SendChatForSp(string text)
        {
            if (Disciple.Status < 3)
            {
                Disciple.Character.CharacterHandler.SendMessage(Service.PublicChat(Disciple.Id, text));
            };
        }

        public void Close()
        {
            DiscipleDB.Update(Disciple);
            Clear();
        }

        public void Clear()
        {
            SuppressFinalize(this);
        }

        public void UpdateInfo()
        {
            SetUpInfo();
            SendZoneMessage(Service.UpdateBody(Disciple));
        }

        public void SetUpPosition(int mapOld = -1, int mapNew = -1, bool isRandom = false)
        {
            if (isRandom)
            {
                Disciple.InfoChar.X = (short) (Disciple.Character.InfoChar.X + 15);
            }
            else
            {
                Disciple.InfoChar.X = Disciple.Character.InfoChar.X;
            }
            Disciple.InfoChar.Y = Disciple.Character.InfoChar.Y;
            //Todo lỗi Touch không có map
            // if (Disciple.InfoChar.Y <= 10)
            // {
            //     Disciple.InfoChar.Y = Disciple.Zone.Map.TileMap.TouchY(Disciple.InfoChar.X, Disciple.InfoChar.Y);
            // }
        }

        public void SendInfo()
        {
            SetUpInfo();
        }

        public void SendDie()
        {
            lock (Disciple)
            {
                RemoveSkill(ServerUtils.CurrentTimeMillis(), true);
                Disciple.InfoChar.IsDie = true;
                Disciple.InfoSkill.Monkey.MonkeyId = 0;
                SetUpInfo();
                // SendZoneMessage(Service.UpdateBody(Disciple));
                SendZoneMessage(Service.PlayerDie(Disciple));
                Disciple.InfoDelayDisciple.LeaveDead = ServerUtils.CurrentTimeMillis() + 30000;
            }
        }

        public int GetParamItem(int id)
        {
            return Disciple.ItemBody.Where(item => item != null).Select(item => item.Options.Where(option => option.Id == id).ToList()).Select(option => option.Sum(optionItem => optionItem.Param)).Sum();
        }

        public List<int> GetListParamItem(int id)
        {
            var param = new List<int>();
            foreach (var item in Disciple.ItemBody.Where(item => item != null))
            {
                var option = item.Options.Where(option => option.Id == id).ToList();
                param.AddRange(option.Select(optionItem => optionItem.Param)); 
            }
            return param;
        }

        public void SetUpInfo()
        {
            SetInfoSet();
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
            SetEnhancedOption();
        }

        public void SetInfoSet()
        {
            Disciple.InfoSet.Reset();

            Disciple.InfoSet.IsFullSetThanLinh = true;
            for (int i = 0; i < 5; i++)
            {
                if (Disciple.ItemBody[i] == null || Disciple.ItemBody[i].Id > 567 || Disciple.ItemBody[i].Id < 555)
                {
                    Disciple.InfoSet.IsFullSetThanLinh = false;
                    break;
                }
            }

            switch (Disciple.InfoChar.Gender)
            {
                case 0:
                {
                    if (Disciple.ItemBody[0] != null)
                    {
                        // IsFullSetKirin = false;
                        // IsFullSetSongoku = false;
                        // IsFullSetThienXingHang = false;
                        var getSetTXH = Disciple.ItemBody[0].Options.FirstOrDefault(option => option.Id == 127);

                        if (getSetTXH != null)
                        {
                            Disciple.InfoSet.IsFullSetThienXinHang = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Disciple.ItemBody[i] == null || Disciple.ItemBody[i].Options.FirstOrDefault(option => option.Id == 127) == null)
                                {
                                    Disciple.InfoSet.IsFullSetThienXinHang = false;
                                    break;
                                }
                            }
                            return;
                        }

                        var getSetKirin = Disciple.ItemBody[0].Options.FirstOrDefault(option => option.Id == 128);

                        if (getSetKirin != null)
                        {
                            Disciple.InfoSet.IsFullSetKirin = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Disciple.ItemBody[i] == null || Disciple.ItemBody[i].Options.FirstOrDefault(option => option.Id == 128) == null)
                                {
                                    Disciple.InfoSet.IsFullSetKirin = false;
                                    break;
                                }
                            }
                            return;
                        }

                        var getSetSGK = Disciple.ItemBody[0].Options.FirstOrDefault(option => option.Id == 129);

                        if (getSetSGK != null)
                        {
                            Disciple.InfoSet.IsFullSetSongoku = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Disciple.ItemBody[i] == null || Disciple.ItemBody[i].Options.FirstOrDefault(option => option.Id == 129) == null)
                                {
                                    Disciple.InfoSet.IsFullSetSongoku = false;
                                    break;
                                }
                            }
                            return;
                        }

                    }
                    break;
                }
                case 1:
                {
                    if (Disciple.ItemBody[0] != null)
                    {
                        // IsFullSetKirin = false;
                        // IsFullSetSongoku = false;
                        // IsFullSetThienXingHang = false;
                        var getSetPicolo = Disciple.ItemBody[0].Options.FirstOrDefault(option => option.Id == 130);

                        if (getSetPicolo != null)
                        {
                            Disciple.InfoSet.IsFullSetPicolo = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Disciple.ItemBody[i] == null || Disciple.ItemBody[i].Options.FirstOrDefault(option => option.Id == 130) == null)
                                {
                                    Disciple.InfoSet.IsFullSetPicolo = false;
                                    break;
                                }
                            }
                            return;
                        }

                        var getSetOcTieu = Disciple.ItemBody[0].Options.FirstOrDefault(option => option.Id == 131);

                        if (getSetOcTieu != null)
                        {
                            Disciple.InfoSet.IsFullSetOcTieu = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Disciple.ItemBody[i] == null || Disciple.ItemBody[i].Options.FirstOrDefault(option => option.Id == 131) == null)
                                {
                                    Disciple.InfoSet.IsFullSetOcTieu = false;
                                    break;
                                }
                            }
                            return;
                        }

                        var getSetPikkoro = Disciple.ItemBody[0].Options.FirstOrDefault(option => option.Id == 132);

                        if (getSetPikkoro != null)
                        {
                            Disciple.InfoSet.IsFullSetPikkoro = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Disciple.ItemBody[i] == null || Disciple.ItemBody[i].Options.FirstOrDefault(option => option.Id == 132) == null)
                                {
                                    Disciple.InfoSet.IsFullSetPikkoro = false;
                                    break;
                                }
                            }
                            return;
                        }

                    }
                    break;
                }
                case 2:
                {
                    if (Disciple.ItemBody[0] != null)
                    {
                        // IsFullSetKirin = false;
                        // IsFullSetSongoku = false;
                        // IsFullSetThienXingHang = false;
                        var getSetKakarot = Disciple.ItemBody[0].Options.FirstOrDefault(option => option.Id == 133);

                        if (getSetKakarot != null)
                        {
                            Disciple.InfoSet.IsFullSetKakarot = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Disciple.ItemBody[i] == null || Disciple.ItemBody[i].Options.FirstOrDefault(option => option.Id == 133) == null)
                                {
                                    Disciple.InfoSet.IsFullSetKakarot = false;
                                    break;
                                }
                            }
                            return;
                        }

                        var getSetCadic = Disciple.ItemBody[0].Options.FirstOrDefault(option => option.Id == 134);

                        if (getSetCadic != null)
                        {
                            Disciple.InfoSet.IsFullSetCadic = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Disciple.ItemBody[i] == null || Disciple.ItemBody[i].Options.FirstOrDefault(option => option.Id == 134) == null)
                                {
                                    Disciple.InfoSet.IsFullSetCadic = false;
                                    break;
                                }
                            }
                            return;
                        }

                        var getSetNappa = Disciple.ItemBody[0].Options.FirstOrDefault(option => option.Id == 135);

                        if (getSetNappa != null)
                        {
                            Disciple.InfoSet.IsFullSetNappa = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Disciple.ItemBody[i] == null || Disciple.ItemBody[i].Options.FirstOrDefault(option => option.Id == 135) == null)
                                {
                                    Disciple.InfoSet.IsFullSetNappa = false;
                                    break;
                                }
                            }
                            return;
                        }

                    }
                    break;
                }
            }
        }

        public void SetEnhancedOption()
        {
            Disciple.InfoOption.Reset();

            Disciple.InfoOption.PhanPercentSatThuong += GetParamItem(97);

            Disciple.InfoOption.PhanTramXuyenGiapChuong += GetParamItem(98);

            Disciple.InfoOption.PhanTramXuyenGiapCanChien += GetParamItem(99);

            Disciple.InfoOption.PhanTramNeDon += GetParamItem(108);

            Disciple.InfoOption.PhanTramVangTuQuai += GetParamItem(100);

            Disciple.InfoOption.PhanTramTNSM += GetParamItem(101);
        }

        public void SetHpFull()
        {
            var hp = Disciple.InfoChar.OriginalHp;
            hp += GetParamItem(2) * 100;
            hp += GetParamItem(6);
            hp += GetParamItem(22) * 1000;
            hp += GetParamItem(48);
            GetListParamItem(77).ForEach(param => hp += hp*param/100);
            GetListParamItem(109).ForEach(param => hp -= hp*param/100);
            // Nappa
            if (Disciple.InfoSet.IsFullSetNappa)
            {
                hp += hp*80/100;
            }
            if (Disciple.InfoSkill.Monkey.MonkeyId != 0) hp += hp * Disciple.InfoSkill.Monkey.Hp / 100;
            if (Disciple.InfoSkill.HuytSao.IsHuytSao) hp += hp * Disciple.InfoSkill.HuytSao.Percent / 100;

            Disciple.HpFull = hp;
        }

        public void SetMpFull()
        {
            var mp = Disciple.InfoChar.OriginalMp;
            mp += GetParamItem(2) * 100;
            mp += GetParamItem(7);
            mp += GetParamItem(23) * 1000;
            mp += GetParamItem(48);
            GetListParamItem(103).ForEach(param => mp += mp*param/100);
            Disciple.MpFull = mp;
        }

        public void SetDamageFull()
        {
            var damage = Disciple.InfoChar.OriginalDamage;
            damage += GetParamItem(0);
            GetListParamItem(50).ForEach(param => damage += damage*param/100);
            GetListParamItem(147).ForEach(param => damage += damage*param/100);
            if (Disciple.InfoSkill.Monkey.MonkeyId != 0) damage += damage * Disciple.InfoSkill.Monkey.Damage / 100;
            Disciple.DamageFull = damage;
        }

        public void SetDefenceFull()
        {
            var defence = Disciple.InfoChar.OriginalDefence * 4;
            defence += GetParamItem(47);
            GetListParamItem(94).ForEach(param => defence += defence*param/100);
            Disciple.DefenceFull = Math.Abs(defence);
        }

        public void SetCritFull()
        {
            int crtCal;
            if (Disciple.InfoSkill.Monkey.MonkeyId != 0)
            {
                crtCal = 115;
            }
            else
            {
                crtCal = Disciple.InfoChar.OriginalCrit;
                crtCal += GetParamItem(14);
            }
            Disciple.CritFull = crtCal;
        }

        public void SetHpPlusFromDamage()
        {
            var hpPlus = GetParamItem(95);
            Disciple.HpPlusFromDamage = hpPlus;

            Disciple.HpPlusFromDamageMonster = GetParamItem(104);
        }

        public void SetMpPlusFromDamage()
        {
            var mpPlus = GetParamItem(96);
            Disciple.MpPlusFromDamage = mpPlus;
        }

        public void SetSpeed()
        {
            var speed = 5;
            if (Disciple.InfoSkill.Monkey.MonkeyId != 0) speed = 7;
            var plus = speed * (GetParamItem(148) + GetParamItem(114) + GetParamItem(16)) / 100;
            switch (plus)
            {
                case <= 1:
                    speed+=1;
                    break;
                case > 1 and <= 2:
                    speed += 2;
                    break;
                case > 2:
                    speed += plus;
                    break;
            }
            Disciple.InfoChar.Speed = (sbyte)speed;
        }

        public void SetBuffHp30s()
        {
            var hpPlus = GetParamItem(27);
            Disciple.Effect.BuffHp30S.Value = hpPlus;
            if (Disciple.Effect.BuffHp30S.Time == -1)
            {
                Disciple.Effect.BuffHp30S.Time = 30000 + ServerUtils.CurrentTimeMillis();
            }
            
        }

        public void SetBuffMp1s()
        {
            var mpPlus = (int)Disciple.MpFull * GetParamItem(162)/100;
            Disciple.Effect.BuffKi1s.Value = mpPlus;
            if (Disciple.Effect.BuffKi1s.Time == -1)
            {
                Disciple.Effect.BuffKi1s.Time = 1500 + ServerUtils.CurrentTimeMillis();
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
            if(Disciple.IsDontMove()) return;

            var compare = Math.Abs(Disciple.InfoChar.X - toX);
            if (compare >= 50)
            {
                Disciple.IsFire = false;
                Disciple.InfoDelayDisciple.Fire = timeServer + 1500;
                if (Disciple.InfoChar.X < toX)
                {
                    Disciple.InfoChar.X = compare switch
                    {
                        >= 150 => (short) (toX - 70),
                        _ => (short) (toX - 50)
                    };
                }
                else
                {
                    Disciple.InfoChar.X = compare switch
                    {
                        >= 150 => (short) (toX + 70),
                        _ => (short) (toX + 50)
                    };
                }

                if (toY != Disciple.InfoChar.Y)
                {
                    Disciple.InfoChar.Y = toY;
                }

                SendZoneMessage(Service.PlayerMove(Disciple.Id, Disciple.InfoChar.X, Disciple.InfoChar.Y));
                if (Disciple.InfoSkill.MeTroi.IsMeTroi && Disciple.InfoSkill.MeTroi.DelayStart <= timeServer)
                {
                    SkillHandler.RemoveTroi(Disciple);
                }
            }
        }

        public void PlusHp(int hp)
        {
            lock (Disciple.InfoChar)
            {
                if(Disciple.InfoChar.IsDie) return;
                Disciple.InfoChar.Hp += hp;
                if (Disciple.InfoChar.Hp >= Disciple.HpFull) Disciple.InfoChar.Hp = Disciple.HpFull;
            }
        }

        public void MineHp(long hp)
        {
            lock (Disciple.InfoChar)
            {
                if(Disciple.InfoChar.IsDie || hp <= 0) return;
                if (hp > Disciple.InfoChar.Hp)
                {
                    Disciple.InfoChar.Hp = 0;
                }
                else 
                {
                    Disciple.InfoChar.Hp -= hp;
                }

                if (Disciple.InfoChar.Hp <= 0)
                {
                    Disciple.InfoChar.IsDie = true;
                    Disciple.InfoChar.Hp = 0;
                }
            }
        }

        public void PlusMp(int mp)
        {
            lock (Disciple.InfoChar)
            {
                if(Disciple.InfoChar.IsDie) return;
                Disciple.InfoChar.Mp += mp;
                if (Disciple.InfoChar.Mp >= Disciple.MpFull) Disciple.InfoChar.Mp = Disciple.MpFull;
            }
        }

        public void MineMp(int mp)
        {
            lock (Disciple.InfoChar)
            {
                if(Disciple.InfoChar.IsDie || mp < 0) return;
                Disciple.InfoChar.Mp -= mp;
                if (Disciple.InfoChar.Mp <= 0) Disciple.InfoChar.Mp = 0;
            }
        }

        public void PlusStamina(int stamina)
        {
            lock (Disciple.InfoChar)
            {
                Disciple.InfoChar.Stamina += (short)stamina;
                if (Disciple.InfoChar.Stamina > 1250) Disciple.InfoChar.Stamina = 1250;
            }
        }

        public void MineStamina(int stamina)
        {
            lock (Disciple.InfoChar)
            {
                if (stamina < 0) return;
                Disciple.InfoChar.Stamina -= (short)stamina;
                if (Disciple.InfoChar.Stamina <= 0) Disciple.InfoChar.Stamina = 0;
            }
        }

        public void PlusPower(long power)
        {
            lock (Disciple.InfoChar)
            {
                Disciple.InfoChar.Power += power;
                Disciple.InfoChar.Level = (sbyte)(Cache.Gi().EXPS.Count(exp => exp < Disciple.InfoChar.Power) - 1);
                if (Cache.Gi().LIMIT_POWERS[Disciple.Character.InfoChar.LitmitPower].Power > Disciple.InfoChar.Power)
                {
                    Disciple.InfoChar.IsPower = true;
                }
                else 
                {
                    Disciple.InfoChar.IsPower = false;
                }
            }
        }

        public void PlusPotential(long potential)
        {
            lock (Disciple.InfoChar)
            {
                Disciple.InfoChar.Potential += potential;
            }
        }

        public Model.Item.Item RemoveItemBody(int index)
        {
            Model.Item.Item item;
            lock (Disciple.ItemBody)
            {
                item = Disciple.ItemBody[index];
                if (item == null) return null;
                Disciple.ItemBody[index] = null;
                DiscipleDB.SaveInventory(Disciple);
                UpdateInfo();
            }
            return item;
        }

        public void AddItemToBody(Model.Item.Item item, int index)
        {
            if (item == null) return;
            item.IndexUI = index;
            Disciple.ItemBody[index] = item;
            DiscipleDB.SaveInventory(Disciple);
        }

        public void RemoveMonsterMe()
        {
            var skillEgg = Disciple.InfoSkill.Egg;
            if (skillEgg.Monster is {IsDie: true})
            {
                SendZoneMessage(Service.UpdateMonsterMe7(skillEgg.Monster.Id));
                Disciple.Zone.ZoneHandler.RemoveMonsterMe(skillEgg.Monster.Id);
                SkillHandler.RemoveMonsterPet(Disciple);
            }
        }

        public void PlusPoint(IMonster monster, int damage)
        {
            if(monster.IsMobMe && monster.Character != null) return;
            if(damage <= 0) return;

            long fixDmg = (long) ((damage) + (monster.OriginalHp * 0.00125));
            long damagePlusPoint = fixDmg;

            if (damagePlusPoint <= 0)
            {
                damagePlusPoint = 2;
            }
            
            if (monster.Id != 0)
            {
                var levelChar = Disciple.InfoChar.Level;
                var levelMonster = monster.Level;
                var checkLevel = Math.Abs(levelChar - levelMonster);
                if (checkLevel > 5 && levelChar > levelMonster)
                {
                    damagePlusPoint = 1;
                } 
                else if (levelChar < levelMonster && checkLevel <= 5)
                {
                    damagePlusPoint *= (monster.LvBoss * 2 + checkLevel);
                }
                else if (levelChar < levelMonster)
                {
                    damagePlusPoint *= monster.LvBoss * 2 + 3;
                }
                else
                {
                    damagePlusPoint *= monster.LvBoss * 2 + 1;
                }
                
            }

            switch (Disciple.Flag)
            {
                case 0: break;
                case 8:
                    damagePlusPoint += damagePlusPoint*10/100;
                    break;
                default:
                    damagePlusPoint += damagePlusPoint*5/100;
                    break;
            }

            if (DatabaseManager.Manager.gI().ExpUp > 1)
            {
                if (Disciple.InfoChar.Power < DatabaseManager.Manager.gI().LimitPowerExpUp)
                {
                    damagePlusPoint *= DatabaseManager.Manager.gI().ExpUp;
                }
                else 
                {
                    damagePlusPoint *= (DatabaseManager.Manager.gI().ExpUp/2);
                }
            }

            // Option Sao pha lê
            var OptionPhanTramTNSM = Disciple.InfoOption.PhanTramTNSM;
            if (OptionPhanTramTNSM > 0)
            {
                damagePlusPoint += damagePlusPoint*OptionPhanTramTNSM/100;
            }

            // Đệ ma bư up sức mạnh lâu hơn đệ tử thường
            // if (Disciple.Type == 2)
            // {
            //     damagePlusPoint /= 2;
            // }
            // 
            // if (damagePlusPoint >= 100000)
            // {
            //     damagePlusPoint = 100000;
            // }
            
            if (Disciple.InfoChar.IsPower)
            {
                PlusPoint(damagePlusPoint, damagePlusPoint, true);
            }
            else
            {
                PlusPoint(0, damagePlusPoint, false);
            }

            // Đệ ma bư cộng chỉ số khi up cho sư phụ cao hơn
            // if (Disciple.Type == 2)
            // {
            //     Disciple.Character.CharacterHandler.PlusPoint(damagePlusPoint, damagePlusPoint, true);
            // }
            // else
            // {
            //     Disciple.Character.CharacterHandler.PlusPoint(damagePlusPoint/2, damagePlusPoint/2, true);
            // }
            if (Disciple.Character.InfoChar.Power > DatabaseManager.Manager.gI().LimitPowerExpUp)
            {
                if (Disciple.Character.InfoChar.IsPower)
                {
                    Disciple.Character.CharacterHandler.PlusPoint((long)(damagePlusPoint/5), (long)(damagePlusPoint/2), true);
                }
                else 
                {
                    Disciple.Character.CharacterHandler.PlusPoint(0, (long)(damagePlusPoint/2), false);
                }
            }
            else 
            {
                if (Disciple.Character.InfoChar.IsPower)
                {
                    Disciple.Character.CharacterHandler.PlusPoint((long)(damagePlusPoint/1.5), (long)(damagePlusPoint/1.5), true);
                }
                else 
                {
                    Disciple.Character.CharacterHandler.PlusPoint(0, (long)(damagePlusPoint/1.5), false);
                }
            }
        }

        public void PlusPoint(long power, long potential, bool isAll)
        {
            // if (!Disciple.Character.InfoChar.IsPremium && Disciple.Character.InfoChar.Power >= DataCache.PREMIUM_LIMIT_UP_POWER)
            // {
            //     SendChatForSp(TextServer.gI().DISCIPLE_NOT_PREMIUM_LIMIT_POWER);
            //     return;
            // }

            if (isAll && power > 0 && potential > 0)
            {
                PlusPower(power);
                PlusPotential(potential);
            }
            else
            {
                if (power > 0)
                {
                    PlusPower(power);
                }

                if (potential > 0)
                {
                    PlusPotential(potential);
                }
            }
        }

        public void LeaveFromDead(bool isHeal = false)
        {
            lock (Disciple)
            {
                UpdateInfo();
                Disciple.InfoChar.IsDie = false;
                Disciple.InfoChar.Hp = Disciple.HpFull;
                Disciple.InfoChar.Mp = Disciple.MpFull;
                SendZoneMessage(Service.ReturnPointMap(Disciple));
                SendZoneMessage(Service.PlayerLoadLive(Disciple));
            }
        }

        public void RemoveSkill(long timeServer, bool globalReset = false)
        {
            var infoSkill = Disciple.InfoSkill;
            if ((infoSkill.TaiTaoNangLuong.IsTTNL &&
                 infoSkill.TaiTaoNangLuong.DelayTTNL <= timeServer) || globalReset)
            {
                SkillHandler.RemoveTTNL(Disciple);
            }

            if (infoSkill.Monkey.MonkeyId == 1 && infoSkill.Monkey.TimeMonkey <= timeServer || globalReset)
            {
                SkillHandler.HandleMonkey(Disciple,false);
            }

            if (infoSkill.Protect.IsProtect && infoSkill.Protect.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveProtect(Disciple);
            }

            if (infoSkill.HuytSao.IsHuytSao && infoSkill.HuytSao.Time <= timeServer)
            {
                SkillHandler.RemoveHuytSao(Disciple);
            }

            if (infoSkill.PlayerTroi.IsPlayerTroi || globalReset)
            {
                if (globalReset && infoSkill.PlayerTroi.IsPlayerTroi)
                {
                    infoSkill.PlayerTroi.PlayerId.ForEach(i => SkillHandler.RemoveTroi(Disciple.Zone.ZoneHandler.GetCharacter(i)));
                }
            }

            if (infoSkill.DichChuyen.IsStun && infoSkill.DichChuyen.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveDichChuyen(Disciple);
            }

            if (infoSkill.ThaiDuongHanSan.IsStun && infoSkill.ThaiDuongHanSan.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveThaiDuongHanSan(Disciple);
            }

            if (infoSkill.ThoiMien.IsThoiMien && infoSkill.ThoiMien.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveThoiMien(Disciple);
            }

            if (infoSkill.Socola.IsSocola && infoSkill.Socola.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveSocola(Disciple);
            }
        }

        public void UpdateEffect(long timeServer)
        {
            var effect = Disciple.Effect;
            if (effect.BuffHp30S.Value > 0 && effect.BuffHp30S.Time <= timeServer && Disciple.InfoChar.Hp < Disciple.HpFull)
            {
                PlusHp(effect.BuffHp30S.Value);
                SendZoneMessage(Service.PlayerLevel(Disciple));
                effect.BuffHp30S.Time = 30000 + timeServer;
            }

            if (effect.BuffKi1s.Value > 0 && effect.BuffKi1s.Time <= timeServer && Disciple.InfoChar.Mp < Disciple.MpFull)
            {
                PlusMp(effect.BuffKi1s.Value);
                effect.BuffKi1s.Time = 1500 + timeServer;
            }
        }

        public void UpdateMask(long timeServer)
        {
            var item = Disciple.ItemBody[5];
            if(item == null) return;
            /*switch (item.Id)
            {
                //TODO handle logic for mask
            }*/
        }

        public void UpdateAutoPlay(long timeServer)
        {
            
        }

        public void UpdateLuyenTap()
        {
            
        }

        public void RemoveTroi(int charId)
        {
            var infoSkill = Disciple.InfoSkill.PlayerTroi;
            if (infoSkill.IsPlayerTroi)
            {
                infoSkill.PlayerId.RemoveAll(i => i == charId);
                if (infoSkill.PlayerId.Count <= 0)
                {
                    infoSkill.IsPlayerTroi = false;
                    infoSkill.TimeTroi = -1;
                    infoSkill.PlayerId.Clear();
                    SendZoneMessage(Service.SkillEffectPlayer(charId, Disciple.Id, 2, 32));
                }
            }
        }

        #region Ignored Function
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
            //Disciple join map
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

        public void LeaveItem(ICharacter character)
        {
            // Ignore
        }

        #endregion
    }
}
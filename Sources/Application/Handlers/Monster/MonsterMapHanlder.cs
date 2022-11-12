using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Handlers.Item;
using NRO_Server.Application.Handlers.Skill;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Monster;
using NRO_Server.Application.IO;
using NRO_Server.Application.Main;
using NRO_Server.Model.Monster;

namespace NRO_Server.Application.Handlers.Monster
{
    public class MonsterMapHanlder : IMonsterHandler
    {
        public IMonster Monster { get; set; }
        
        public void SetUpMonster()
        {
            Monster.Status = 5;
            Monster.IsDie = false;
            SetClassNewMonster();
            SetHpNewMonster();
            SetLevelBossNewMonster();
            SetDamageBossNewMonster();
        }

        public void SetClassNewMonster()
        {
            Monster.Sys = (sbyte)ServerUtils.RandomNumber(1, 3);
        }

        public void SetHpNewMonster()
        {
            Monster.Hp = Monster.HpMax = GetMonsterHP();
            Monster.MaxExp = (int)Monster.OriginalHp;
        }

        private long GetMonsterHP()
        {
            if (DataCache.IdMapThanhDia.Contains(Monster.Zone.Map.Id))
            {
                switch (Monster.Id)
                {
                    case 39:
                    {
                        return 7250000;
                    }
                    case 40:
                    {
                        return 7000000;
                    }
                    case 43:
                    {
                        return 10200000;
                    }
                    case 49:
                    {
                        return 5800000;
                    }
                    case 50:
                    {
                        return 9000000;
                    }
                    case 66:
                    {
                        return 11800000;
                    }
                    case 67:
                    {
                        return 13100000;
                    }
                    case 68:
                    {
                        return 12500000;
                    }
                    case 69:
                    {
                        return 10350000;
                    }

                }
            }
            return Monster.OriginalHp;
        }

        public void SetLevelBossNewMonster()
        {
            if (ServerUtils.RandomNumber(50) < 2 && Monster.Level >= 7 && !Monster.IsBoss && !Monster.IsMobMe && Monster.Zone.MonsterMaps.FirstOrDefault(x => !x.IsDie && x.LvBoss == 1) == null)
            {
                Monster.LvBoss = 0;//tat sieu quai 1;
            }
            else
            {
                Monster.LvBoss = 0;
            }
        }

        public void SetDamageBossNewMonster()
        {
            if (Monster.IsBoss)
            {
                Monster.Damage = 7000;
            }
            else
            {
                var IsMapThanhDia = DataCache.IdMapThanhDia.Contains(Monster.Zone.Map.Id);
                if (IsMapThanhDia)
                {
                    Monster.Damage = (int)Monster.HpMax * 3 / 100;
                }
                else 
                {
                    Monster.Damage = (int)Monster.HpMax * 5 / 100;
                }
            }
        }

        public void Recovery()
        {
            RemoveEffect(ServerUtils.CurrentTimeMillis(), globalReset:true);
            Monster.IsDie = false;
            SetClassNewMonster();
            SetHpNewMonster();
            SetLevelBossNewMonster();
            SetDamageBossNewMonster();
            Monster.Status = 5;
            Monster.IsDontMove = false;
            Monster.Zone.ZoneHandler.SendMessage(Service.MobLive(Monster));
            Monster.CharacterAttack.Clear();
            Monster.SessionAttack.Clear();
        }

        public int UpdateHp(long damage, int charId, bool isMaxHp = false)
        {
            if (damage < 0) damage = Math.Abs(damage);
            
            if (Monster.Id == 0)
            {
                damage = 10;
            }
            else
            {
                if (Monster.Hp == Monster.HpMax && damage >= Monster.HpMax && !isMaxHp)
                {
                    damage = Monster.HpMax - 1;
                }
                else if(damage >= Monster.Hp)
                {
                    damage = Monster.Hp;
                }
            }

            Monster.Hp -= damage;
            if(Monster.Hp <= 0) StartDie();
            return (int)damage;
        }

        private void StartDie()
        {
            Monster.Zone.ZoneHandler.SendMessage(Service.MonsterDie(Monster.IdMap));
            Monster.Hp = 0;
            Monster.IsDie = true;
            Monster.Status = 0;
            RemoveEffect(ServerUtils.CurrentTimeMillis(), globalReset:true);
            if (Monster.IsRefresh) Monster.TimeRefresh = ServerUtils.CurrentTimeMillis() + 7000;
        }
        public void LeaveItem(ICharacter character)
        {
            if(Monster.Id == 0 || !Monster.IsDie) return;
            // var quantity = 1;
            // if (character.InfoChar.Level - Monster.Level < 6)
            // {
            //     quantity = ServerUtils.RandomNumber(100 * Monster.Level, 200 * Monster.Level);
            // }

            // if (Monster.LvBoss == 1)
            // {
            //     quantity *= 5;
            // }
            // Console.WriteLine("LeaveItemType: " + Monster.LeaveItemType);
            // var itemMap = LeaveItemHandler.LeaveGold(Math.Abs(character.Id), quantity);
            var plusGoldPercent = 0;
            Model.Character.Character charRel = null;
            if (character.Id > 0)
            {
                charRel = (Model.Character.Character)character;
                var specialId = charRel.SpecialSkill.Id;
                if (specialId != -1 && (specialId == 7 || specialId == 18 || specialId == 28)) //Đã có nội tại rơi vàng cộng thêm từ quái
                {
                    plusGoldPercent += charRel.SpecialSkill.Value;
                }
            }

            plusGoldPercent += character.InfoOption.PhanTramVangTuQuai;

            var itemMap = LeaveItemHandler.LeaveMonsterItem(character, Monster.LeaveItemType, plusGoldPercent, character.Zone.Map.Id, Monster.Id);
            if(itemMap == null) return;

            // kiểm tra
            if (character.Id > 0)
            {
                if (itemMap.Item.Id == 933)
                {
                    if (character.InfoChar.TrainManhVo >= DataCache.LIMIT_TRAIN_MANH_VO_BONG_TAI_NGAY)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().LIMIT_TRAIN_MANH_VO));
                        return;
                    }
                    character.InfoChar.TrainManhVo++;
                }
                if (itemMap.Item.Id == 934)
                {
                    if (character.InfoChar.TrainManhHon >= DataCache.LIMIT_TRAIN_MANH_HON_BONG_TAI_NGAY)
                    {
                        character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().LIMIT_TRAIN_MANH_HON));
                        return;
                    }
                    character.InfoChar.TrainManhHon++;
                }
            }
            else 
            {
                var disciple = (Model.Character.Disciple) character;
                if (itemMap.Item.Id == 933)
                {
                    if (disciple.Character.InfoChar.TrainManhVo >= DataCache.LIMIT_TRAIN_MANH_VO_BONG_TAI_NGAY)
                    {
                        disciple.Character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().LIMIT_TRAIN_MANH_VO));
                        return;
                    }
                    disciple.Character.InfoChar.TrainManhVo++;
                }
                if (itemMap.Item.Id == 934)
                {
                    if (disciple.Character.InfoChar.TrainManhHon >= DataCache.LIMIT_TRAIN_MANH_HON_BONG_TAI_NGAY)
                    {
                        disciple.Character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().LIMIT_TRAIN_MANH_HON));
                        return;
                    }
                    disciple.Character.InfoChar.TrainManhHon++;
                }
            }

            itemMap.X = Monster.X;
            itemMap.Y = Monster.Y;
            Monster.Zone.ZoneHandler.LeaveItemMap(itemMap, (MonsterMap)Monster);

            // Kiểm tra có bùa thu hút ko
            if (character.Id > 0)
            {
                if (charRel.InfoMore.BuaThuHut)
                {
                    if (charRel.InfoMore.BuaThuHutTime > ServerUtils.CurrentTimeMillis())
                    {
                        charRel.CharacterHandler.PickItemMap((short)itemMap.Id);
                    }
                    else 
                    {
                        charRel.InfoMore.BuaThuHut = false;
                    }
                }
            }
            else //Đệ 
            {
                var disciple = (Model.Character.Disciple) character;
                if (disciple.Character.InfoMore.BuaThuHut)
                {
                    if (disciple.Character.InfoMore.BuaThuHutTime > ServerUtils.CurrentTimeMillis())
                    {
                        disciple.Character.CharacterHandler.PickItemMap((short)itemMap.Id);
                    }
                    else 
                    {
                        disciple.Character.InfoMore.BuaThuHut = false;
                    }
                }
            }
        }

        public void MonsterAttack(long timeServer)
        {
            if (Monster.CharacterAttack.Count > 0)
            {
                foreach (var id in Monster.CharacterAttack.ToList())
                {
                    ICharacter character;
                    // bool IsCharacter = false;
                    if (id > 0)
                    {
                        character = Monster.Zone.ZoneHandler.GetCharacter(id);
                        // IsCharacter = true;
                    }
                    else
                    {
                        character = Monster.Zone.ZoneHandler.GetDisciple(id);
                    }
                    if (character == null)
                    {
                        Monster.CharacterAttack.RemoveAll(i => i == id);
                        continue;
                    }
                    if (character.InfoChar.IsDie)
                    {
                        Monster.CharacterAttack.RemoveAll(i => i == id);
                        continue;
                    }

                    // if (IsCharacter)
                    // {
                    //     var charRel = (Model.Character.Character)character;
                    //     if (charRel.InfoMore.BuaBatTu && charRel.InfoChar.Hp <= 1)
                    //     {
                    //         Monster.CharacterAttack.RemoveAll(i => i == id);
                    //         continue;
                    //     }
                    //     // có bùa bất tử thì bỏ qua
                    // }

                    var distance = Math.Abs(character.InfoChar.X - Monster.X);
                    if (!character.InfoChar.IsDie && distance <= 220 && Math.Abs(character.InfoChar.Y - Monster.Y) <= 120)
                    {
                        HandlerAttackCharacter(character, timeServer);
                        if (distance <= 150)
                        {
                            Monster.DelayFight = 1500 + timeServer;
                        }
                        else
                        {
                            Monster.DelayFight = 1000 + timeServer;
                        }
                        break;
                    }
                    Monster.CharacterAttack.RemoveAll(i => i == id);
                }
            }
            else if (Monster.Level >= 7 && ServerUtils.RandomNumber(3) < 1)
            {
                foreach (var character in Monster.Zone.Characters.Values.ToList())
                {
                    // Quái không tự đánh
                    if(character.IsInvisible() || character.InfoChar.IsDie || character.InfoMore.IsNearAuraPhongThuItem) continue;
                    if (!character.InfoChar.IsDie && Math.Abs(character.InfoChar.X - Monster.X) <= 70 &&
                        Math.Abs(character.InfoChar.Y - Monster.Y) <= 40 )
                    {
                        HandlerAttackCharacter(character, timeServer);
                        Monster.DelayFight = 1500 + timeServer;
                        return;
                    }
                }
            }
        }

        public void HandlerAttackCharacter(ICharacter character, long timeServer)
        {
            Monster.TimeAttack = 10000 + timeServer;
            var damage = ServerUtils.RandomNumber(Monster.Damage*9/10, Monster.Damage*11/10);
            if (Monster.LvBoss == 1 && !Monster.IsBoss)
            {
                if (damage < character.HpFull)
                {
                    damage = (int)character.HpFull / 10;
                }
            }

            damage -= character.DefenceFull;

            if (Monster.InfoSkill.ThoiMien.TimePercent > 0 && Monster.InfoSkill.ThoiMien.TimePercent > timeServer)
            {
                damage -= damage * Monster.InfoSkill.ThoiMien.Percent / 100;
            }

            if (Monster.InfoSkill.Socola.IsSocola && Monster.LvBoss == 0 && !Monster.IsBoss && Monster.InfoSkill.Socola.CharacterId == character.Id)
            {
                damage = 1;
            }
            else
            {
                if (Monster.InfoSkill.Socola.IsSocola)
                {
                    damage -= damage * Monster.InfoSkill.Socola.Percent / 100;
                }

                if (ServerUtils.RandomNumber(100) < 50 && damage <= 0)
                {
                    damage = 1;
                } 
                else if (character.InfoSkill.Protect.IsProtect)
                {
                    if (character.HpFull <= damage)
                    {
                        //HANDLE REMOVE SKILL PROTECT
                        if (character.Id > 0)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().DROP_PROTECT));
                            character.CharacterHandler.SendMessage(Service.ItemTime(DataCache.TimeProtect[0], 0));
                        }
                        SkillHandler.RemoveProtect(character);
                    }
                    damage = 1;
                }
            }

            // Xử lý phản phần trăm sát thương
            int phanTramPhanSatThuong = character.InfoOption.PhanPercentSatThuong;
            
            if (damage > 0 && phanTramPhanSatThuong > 0)
            {
                int phanDamage = damage*phanTramPhanSatThuong/100;
                UpdateHp(phanDamage, character.Id);
                character.CharacterHandler.SendZoneMessage(Service.MonsterHp(Monster, false, phanDamage, -1));
            }

            // Kiểm tra phải đệ tử nhận damage không
            // Kiểm tra xem có bùa đệ tử không
            if (character.Id < 0 && damage > 0)
            {
                var discipleReal = (Model.Character.Disciple)character;
                if (discipleReal.Character.InfoMore.BuaDeTu)
                {
                    if (discipleReal.Character.InfoMore.BuaDeTuTime > timeServer)
                    {
                        damage /= 2;
                    }
                    else 
                    {
                        discipleReal.Character.InfoMore.BuaDeTu = false;
                    }
                }
            }

            if (character.Id > 0)
            {
                var charRel = (Model.Character.Character)character;

                // Kiểm tra xem có bùa da trâu không
                if (charRel.InfoMore.BuaDaTrau)
                {
                    if (charRel.InfoMore.BuaDaTrauTime > ServerUtils.CurrentTimeMillis())
                    {
                        damage /= 2;
                    }
                    else 
                    {
                        charRel.InfoMore.BuaDaTrau = false;
                    }
                }

                if (charRel.InfoMore.BuaBatTu)
                {
                    if (charRel.InfoMore.BuaBatTuTime > timeServer)
                    {
                        // Neus damage lớn hơn máu thì set máu bằng 1
                        if (character.InfoChar.Hp - damage <= 1)
                        {
                            character.InfoChar.Hp = 1;
                            character.CharacterHandler.SendMessage(Service.SendHp((int)character.InfoChar.Hp));
                            damage = 0;
                        }
                    }
                    else 
                    {
                        charRel.InfoMore.BuaBatTu = false;
                    }
                }

                if (charRel.InfoMore.IsNearAuraPhongThuItem)
                {
                    if (charRel.InfoMore.AuraPhongThuTime > timeServer)
                    {
                        damage -= damage*20/100;
                    }
                    else 
                    {
                        charRel.InfoMore.IsNearAuraPhongThuItem = false;
                    }
                }

                // Giáp xên
                if (damage > 0 && charRel.InfoBuff.GiapXen)
                {
                    damage -= (damage*50/100);
                }
            }

            if (damage < 0)
            {
                damage = 1;
            }

            character.CharacterHandler.MineHp(damage);
            Monster.Zone.ZoneHandler.SendMessage(Service.MonsterAttackPlayer(Monster.IdMap, character));

            if (character.Id > 0)
            {
                character.CharacterHandler.SendMessage(Service.MonsterAttackMe(Monster.IdMap, damage, 0));
            }

            if (character.InfoChar.IsDie)
            {
                character.CharacterHandler.SendDie();
            }
        }

        public int PetAttackMonster(IMonster monster)
        {
            //Ignored
            return 0;
        }

        public void PetAttackPlayer(ICharacter character)
        {
            //Ignored
        }

        public void MonsterAttack(ICharacter temp, ICharacter character)
        {
            //Ignored
        }

        public void Update()
        {
            var timeServer = ServerUtils.CurrentTimeMillis();
            if (Monster.IsDie && Monster.IsRefresh && Monster.TimeRefresh > 0 && Monster.TimeRefresh <= timeServer) {
                Recovery();
                return;
            }

            if (!Monster.IsDie)
            {
                RemoveEffect(timeServer);
            }

            if(Monster.InfoSkill.PlayerTroi.IsPlayerTroi || Monster.InfoSkill.DichChuyen.IsStun) return;

            if (!Monster.IsDie && Monster.Id != 0)
            {
                if(Monster.TimeHp <= timeServer && Monster.Hp < Monster.HpMax && Monster.TimeAttack <= timeServer)
                {
                    Monster.Hp += Monster.HpMax / 10;
                    Monster.TimeHp = 3000 + timeServer;
                    Monster.Zone.ZoneHandler.SendMessage(Service.MonsterHp(Monster));
                }
                if(Monster.DelayFight <= timeServer) MonsterAttack(timeServer);
            }
        }

        public void AddPlayerAttack(ICharacter character, int damage)
        {
            if (!Monster.CharacterAttack.Contains(character.Id))
            {
                Monster.CharacterAttack.Add(character.Id);
            }

            var sessId = character.Player.Session.Id;
            
            if (!Monster.SessionAttack.TryAdd(sessId, damage))
            {
                var dmg = Monster.SessionAttack[sessId];
                dmg += damage;
                Monster.SessionAttack[sessId] = dmg;
            };
        }

        public void RemoveTroi(int charId)
        {
            lock (Monster.InfoSkill.PlayerTroi)
            {
                var infoSkill = Monster.InfoSkill.PlayerTroi;
                if (infoSkill.IsPlayerTroi)
                {
                    infoSkill.PlayerId.RemoveAll(i => i == charId);
                    if (infoSkill.PlayerId.Count <= 0)
                    {
                        infoSkill.IsPlayerTroi = false;
                        infoSkill.TimeTroi = -1;
                        infoSkill.PlayerId.Clear();
                        Monster.Zone.ZoneHandler.SendMessage(Service.SkillEffectMonster(-1, Monster.IdMap, 0, 32));
                        if (Monster.IsDontMove)
                        {
                            Monster.IsDontMove = false;
                            Monster.Zone.ZoneHandler.SendMessage(Service.MonsterDontMove(Monster.IdMap, false));
                        }
                    }
                }
            }
        }

        public void RemoveDichChuyen(long timeServer, bool globalReset)
        {
            lock (Monster.InfoSkill.DichChuyen)
            {
                if (Monster.InfoSkill.DichChuyen.IsStun && Monster.InfoSkill.DichChuyen.Time <= timeServer || globalReset)
                {
                    Monster.InfoSkill.DichChuyen.IsStun = false;
                    Monster.InfoSkill.DichChuyen.Time = -1;
                    Monster.Zone.ZoneHandler.SendMessage(Service.SkillEffectMonster(-1, Monster.IdMap, 0, 40));
                    if (Monster.IsDontMove)
                    {
                        Monster.IsDontMove = false;
                        Monster.Zone.ZoneHandler.SendMessage(Service.MonsterDontMove(Monster.IdMap, false));
                    }
                }
            }
        }

        public void RemoveThoiMien(long timeServer, bool globalReset)
        {
            lock (Monster.InfoSkill.ThoiMien)
            {
                if (Monster.InfoSkill.ThoiMien.IsThoiMien && Monster.InfoSkill.ThoiMien.Time <= timeServer || globalReset)
                {
                    Monster.InfoSkill.ThoiMien.IsThoiMien = false;
                    Monster.InfoSkill.ThoiMien.Time = -1;
                    Monster.InfoSkill.ThoiMien.TimePercent = 10000 + timeServer;
                    Monster.Zone.ZoneHandler.SendMessage(Service.SkillEffectMonster(-1, Monster.IdMap, 0, 41));
                    if (Monster.IsDontMove)
                    {
                        Monster.IsDontMove = false;
                        Monster.Zone.ZoneHandler.SendMessage(Service.MonsterDontMove(Monster.IdMap, false));
                    }
                }
            }
        }

        public void RemoveThoiMien2(long timeServer, bool globalReset)
        {
            lock (Monster.InfoSkill.ThoiMien)
            {
                if (Monster.InfoSkill.ThoiMien.TimePercent > 0 && Monster.InfoSkill.ThoiMien.TimePercent <= timeServer || globalReset)
                {
                    Monster.InfoSkill.ThoiMien.TimePercent = -1;
                    Monster.InfoSkill.ThoiMien.Percent = 0;
                }
            }
        }

        public void RemoveThaiDuongHanSan(long timeServer, bool globalReset)
        {
            lock (Monster.InfoSkill.ThaiDuongHanSan)
            {
                if (Monster.InfoSkill.ThaiDuongHanSan.IsStun && Monster.InfoSkill.ThaiDuongHanSan.Time <= timeServer || globalReset)
                {
                    Monster.InfoSkill.ThaiDuongHanSan.IsStun = false;
                    Monster.InfoSkill.ThaiDuongHanSan.Time = -1;
                    Monster.InfoSkill.ThaiDuongHanSan.TimeReal = 0;
                    Monster.Zone.ZoneHandler.SendMessage(Service.SkillEffectMonster(-1, Monster.IdMap, 0, 40));
                    if (Monster.IsDontMove)
                    {
                        Monster.IsDontMove = false;
                        Monster.Zone.ZoneHandler.SendMessage(Service.MonsterDontMove(Monster.IdMap, false));
                    }
                }
            }
        }

        public void RemoveTroi(long timeServer, bool globalReset)
        {
            lock (Monster.InfoSkill.PlayerTroi)
            {
                var infoSkill = Monster.InfoSkill.PlayerTroi; 
                if (globalReset)
                {
                    infoSkill.IsPlayerTroi = false;
                    infoSkill.PlayerId.ForEach(i => SkillHandler.RemoveTroi(Monster.Zone.ZoneHandler.GetCharacter(i)));
                }
            }
        }

        public void RemoveSocola(long timeServer, bool globalReset)
        {
            lock (Monster.InfoSkill.Socola)
            {
                var infoSkill = Monster.InfoSkill.Socola; 
                if (infoSkill.IsSocola && infoSkill.Time <= timeServer || globalReset)
                {
                    infoSkill.IsSocola = false;
                    infoSkill.Time = -1;
                    infoSkill.CharacterId = -1;
                    infoSkill.Fight = 0;
                    infoSkill.Percent = 0;
                    Monster.Zone.ZoneHandler.SendMessage(Service.ChangeMonsterBody(0, Monster.IdMap, -1));
                }
            }
        }

        public void RemoveEffect(long timeServer, bool globalReset = false)
        {
            RemoveDichChuyen(timeServer, globalReset);
            RemoveThoiMien(timeServer, globalReset);
            RemoveThoiMien2(timeServer, globalReset);
            RemoveThaiDuongHanSan(timeServer, globalReset);
            RemoveTroi(timeServer, globalReset);
            RemoveSocola(timeServer, globalReset);
        }

        public MonsterMapHanlder(IMonster monster)
        {
            Monster = monster;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.Interfaces.Map;
using NRO_Server.Application.Interfaces.Monster;
using NRO_Server.Application.IO;
using NRO_Server.Application.Manager;
using NRO_Server.Application.Threading;
using NRO_Server.DatabaseManager;
using NRO_Server.Model.Character;
using NRO_Server.Model.Clan;
using NRO_Server.Model.Info;
using NRO_Server.Model.Info.Radar;
using NRO_Server.Model.Item;
using NRO_Server.Model.Map;
using NRO_Server.Model.Monster;
using NRO_Server.Model.Template;
using NRO_Server.Application.Helper;
using NRO_Server.Model;

namespace NRO_Server.Application.Main
{
    public class Service
    {
        private static Message MessageSubCommand(sbyte cmd)
        {
            var message = new Message(-30);
            message.Writer.WriteByte(cmd);
            return message;
        }
        
        private static Message MessageNotLogin(sbyte cmd)
        {
            var message = new Message(-29);
            message.Writer.WriteByte(cmd);
            return message;
        }
        
        private static Message MessageNotMap(sbyte cmd)
        {
            var message = new Message(-28);
            message.Writer.WriteByte(cmd);
            return message;
        }      

        //-127
        public static Message LuckRoll0()
        {
            try
            {
                var message = new Message(-127);
                message.Writer.WriteByte(0);
                message.Writer.WriteByte(7);
                DataCache.IdCrackBall.ForEach(i => message.Writer.WriteShort(i));
                message.Writer.WriteByte(0);//0 = vang , 1 = ngoc
                message.Writer.WriteInt(DataCache.CRACK_BALL_PRICE);
                message.Writer.WriteShort(821);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error LuckRoll0 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        public static Message LuckRoll1(List<short> list)
        {
            try
            {
                var message = new Message(-127);
                message.Writer.WriteByte(1);
                message.Writer.WriteByte(list.Count);
                list.ForEach(i => message.Writer.WriteShort(i));
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error LuckRoll1 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-125
        public static Message ShowInput(string title, List<InputBox> list)
        {
            try
            {
                var message = new Message(-125);
                message.Writer.WriteUTF(title);
                message.Writer.WriteByte(list.Count);
                list.ForEach(i =>
                {
                    message.Writer.WriteUTF(i.Name);
                    message.Writer.WriteByte(i.Type);
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ShowInput in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-124 _ ZONE
        //action 0: player
        //action 1: mob
        //type 1: active eff
        //type 2: remove eff
        //eff
        // - 32: trói
        // - 33: khiên
        // - 39: huýt sáo
        // - 40: mù
        // - 41: ngủ
        // - 42: stone
        public static Message SkillEffectPlayer(int charIdUseSkill, int charId2, int type, int eff) {
            try
            {
                var message = new Message(-124);
                message.Writer.WriteByte(type);
                message.Writer.WriteByte(0);
                if (type == 2)
                {
                    message.Writer.WriteInt(charIdUseSkill);
                }
                message.Writer.WriteByte(eff);
                message.Writer.WriteInt(charId2);
                if (eff == 32 && type == 1)
                {
                    message.Writer.WriteInt(charIdUseSkill);
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SkillEffectPlayer in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        public static Message SkillEffectMonster(int charIdUseSkill, int monsterId, int type, int eff) {
            try
            {
                var message = new Message(-124);
                message.Writer.WriteByte(type);
                message.Writer.WriteByte(1);
                message.Writer.WriteByte(eff);
                message.Writer.WriteByte(monsterId);
                if (eff == 32 && type == 1)
                {
                    message.Writer.WriteInt(charIdUseSkill);
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SkillEffectPlayer in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-116 _ ME
        public static Message AutoPlay(bool isAuto)
        {
            try
            {
                var message = new Message(-116);
                message.Writer.WriteByte(isAuto ? 1 : 0);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error AutoPlay in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-113 _ ME
        public static Message ChangeOnSkill(List<sbyte> skills)
        {
            try
            {
                var message = new Message(-113);
                skills.ForEach(skill => message.Writer.WriteByte(skill));
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ChangeOnSkill in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-112 _ Zone
        public static Message ChangeMonsterBody(int type, int idMap, int body)
        {
            try
            {
                var message = new Message(-112);
                message.Writer.WriteByte(type);
                message.Writer.WriteByte(idMap);
                if (type == 1)
                {
                    message.Writer.WriteShort(body);
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ChangeMonsterBody in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-111
        public static Message GetImageResource2()
        {
            try
            {
                var message = new Message(-111);
                message.Writer.WriteShort(0);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error GetImageResource2 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-107 _ Me
        public static Message Disciple(int type, Disciple disciple)
        {
            try
            {
                var message = new Message(-107);
                message.Writer.WriteByte(type);
                if (type != 2 || disciple == null) return message;
                message.Writer.WriteShort(disciple.GetHead());
                message.Writer.WriteByte(disciple.ItemBody.Count);
                disciple.ItemBody.ForEach(item =>
                {
                    if(item == null) 
                        message.Writer.WriteShort(-1);
                    else
                    {
                        var itemTemplate = ItemCache.ItemTemplate(item.Id);
                        message.Writer.WriteShort(itemTemplate.Id);
                        message.Writer.WriteInt(item.Quantity);
                        message.Writer.WriteUTF(itemTemplate.Name);
                        message.Writer.WriteUTF(itemTemplate.Description);
                        message.Writer.WriteByte(item.Options.Count);
                        item.Options.ForEach(op =>
                        {
                            message.Writer.WriteByte(op.Id);
                            message.Writer.WriteShort(op.Param); 
                        });
                    }
                });
                message.Writer.WriteInt((int)disciple.InfoChar.Hp);
                message.Writer.WriteInt((int)disciple.HpFull);
                message.Writer.WriteInt((int)disciple.InfoChar.Mp);
                message.Writer.WriteInt((int)disciple.MpFull);
                message.Writer.WriteInt((int)disciple.DamageFull);
                message.Writer.WriteUTF(disciple.Name);
                message.Writer.WriteUTF(disciple.CurrStrLevel());
                message.Writer.WriteLong(disciple.InfoChar.Power);
                message.Writer.WriteLong(disciple.InfoChar.Potential);
                message.Writer.WriteByte(disciple.Status);
                message.Writer.WriteShort(disciple.InfoChar.Stamina);
                message.Writer.WriteShort(disciple.InfoChar.MaxStamina);
                message.Writer.WriteByte(disciple.CritFull);
                message.Writer.WriteShort(disciple.DefenceFull);
                message.Writer.WriteByte(4); //skill
                for (var i = 0; i < 4; i++)
                {
                    try
                    {
                        var skill = disciple.Skills[i];
                        if (skill != null)
                        {
                            message.Writer.WriteShort(skill.SkillId);
                        }
                        else
                        {
                            message.Writer.WriteShort(-1);
                            message.Writer.WriteUTF(i switch
                            {
                                2 => "Mở khoá khi đạt 1 Tỷ 5 sức mạnh",
                                3 => "Mở khoá khi đạt 20 Tỷ sức mạnh",
                                _ => "Mở khoá khi đạt 150 Tr sức mạnh"
                            });
                        }
                    }
                    catch (Exception)
                    {
                        message.Writer.WriteShort(-1);
                        message.Writer.WriteUTF(i switch
                        {
                            2 => "Mở khoá khi đạt 1 Tỷ 5 sức mạnh",
                            3 => "Mở khoá khi đạt 20 Tỷ sức mạnh",
                            _ => "Mở khoá khi đạt 150 Tr sức mạnh"
                        });
                    }
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ItemTime in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-106 _ Me
        public static Message ItemTime(int itemId, int time)
        {
            try
            {
                var message = new Message(-106);
                message.Writer.WriteShort(itemId);
                message.Writer.WriteShort(time);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ItemTime in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-103 _ ME 0
        public static Message ChangeFlag0()
        {
            try
            {
                var list = DataCache.IdFlag;
                var message = new Message(-103);
                message.Writer.WriteByte(0);
                message.Writer.WriteByte(list.Count);
                list.ForEach(id =>
                {
                    var item = ItemCache.GetItemDefault(id);
                    message.Writer.WriteShort(id);
                    message.Writer.WriteByte(item.Options.Count);
                    item.Options.ForEach(op =>
                    {
                        message.Writer.WriteByte(op.Id);
                        message.Writer.WriteShort(op.Param);
                    });
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ChangeFlag0 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-103 _ 1 _ ZONE
        public static Message ChangeFlag1(int charId, int flag)
        {
            try
            {
                var message = new Message(-103);
                message.Writer.WriteByte(1);
                message.Writer.WriteInt(charId);
                message.Writer.WriteByte(flag);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ChangeFlag1 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-103_ 2 _ ZONE
        public static Message ChangeFlag2(int flag)
        {
            try
            {
                var message = new Message(-103);
                message.Writer.WriteByte(2);
                message.Writer.WriteByte(flag);
                message.Writer.WriteShort(Cache.Gi().ITEM_TEMPLATES.ElementAt(DataCache.IdFlag[flag]).Value.IconId);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ChangeFlag2 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-99 _ Me
        public static Message ListEmeny(IEnumerable<InfoFriend> list)
        {
            try
            {
                var newList = list.ToList().Select((x, i) => new KeyValuePair<int, InfoFriend>(i, x)).OrderByDescending(x => x.Key).ToList();
                var message = new Message(-99);
                message.Writer.WriteByte(0);
                message.Writer.WriteByte(newList.ToList().Count);
                newList.ForEach(emeny =>
                {
                    message.Writer.WriteInt(emeny.Value.Id);
                    message.Writer.WriteShort(emeny.Value.Head);
                    message.Writer.WriteShort(emeny.Value.Body);
                    message.Writer.WriteShort(emeny.Value.Leg);
                    message.Writer.WriteShort(emeny.Value.Bag);
                    message.Writer.WriteUTF(emeny.Value.Name);
                    message.Writer.WriteUTF(ServerUtils.GetPower(emeny.Value.Power));
                    message.Writer.WriteBoolean(ClientManager.Gi().GetCharacter(emeny.Value.Id) != null);
                    
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendNangDong in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-97 _ Me
        public static Message SendNangDong(int nangdong)
        {
            try
            {
                var message = new Message(-97);
                message.Writer.WriteInt(nangdong);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendNangDong in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-96 _ Me
        public static Message ListRank()
        {
            try
            {
                var message = new Message(-96);
                message.Writer.WriteInt(1);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ListRank in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-95 _ 0 _ Zone
        public static Message UpdateMonsterMe0(MonsterPet monsterPet)
        {
            try
            {
                var message = new Message(-95);
                message.Writer.WriteByte(0);
                message.Writer.WriteInt(monsterPet.IdMap);
                message.Writer.WriteShort(monsterPet.Id);
                message.Writer.WriteInt((int)monsterPet.HpMax);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ListRank in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-95 _ 1 _ Zone  Pet attack Monster map
        public static Message UpdateMonsterMe1(int idPet, int idMonstermap)
        {
            try
            {
                var message = new Message(-95);
                message.Writer.WriteByte(1);
                message.Writer.WriteInt(idPet);
                message.Writer.WriteByte(idMonstermap);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ListRank in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-95 _ 2 _ Zone  Pet attack Player
        public static Message UpdateMonsterMe2(int monsterId, int charAttackId, int damage, int hpNew)
        {
            try
            {
                var message = new Message(-95);
                message.Writer.WriteByte(2);
                message.Writer.WriteInt(monsterId);
                message.Writer.WriteInt(charAttackId);
                message.Writer.WriteInt(damage);
                message.Writer.WriteInt(hpNew);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ListRank in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-95 _ 3 _ Zone  Pet damage Monster map
        public static Message UpdateMonsterMe3(int idPet, int idMonstermap, int newHpMonsterMap, int damage)
        {
            try
            {
                var message = new Message(-95);
                message.Writer.WriteByte(3);
                message.Writer.WriteInt(idPet);
                message.Writer.WriteByte(idMonstermap);
                message.Writer.WriteInt(newHpMonsterMap);
                message.Writer.WriteInt(damage);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ListRank in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }       
        //-95 _ 5 _ Player attack pet
        public static Message UpdateMonsterMe5(int charId, int monsterId, int skillId, int damage, int hpNew)
        {
            try
            {
                var message = new Message(-95);
                message.Writer.WriteByte(5);
                message.Writer.WriteInt(charId);
                message.Writer.WriteByte(skillId);
                message.Writer.WriteInt(monsterId);
                message.Writer.WriteInt(damage);
                message.Writer.WriteInt(hpNew);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ListRank in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-95 _ 6 _ Zone Start Die
        public static Message UpdateMonsterMe6(int monsterId)
        {
            try
            {
                var message = new Message(-95);
                message.Writer.WriteByte(6);
                message.Writer.WriteInt(monsterId);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ListRank in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-95 _ 7 _ Zone Remove
        public static Message UpdateMonsterMe7(int monsterId)
        {
            try
            {
                var message = new Message(-95);
                message.Writer.WriteByte(7);
                message.Writer.WriteInt(monsterId);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ListRank in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }


        //-94  _ Me
        public static Message UpdateCooldown(ICharacter character)
        {
            try
            {
                var message = new Message(-94);
                character.Skills.ForEach(skill =>
                {
                    message.Writer.WriteShort(skill.SkillId);
                    var timeLast = (int)(skill.CoolDown- ServerUtils.CurrentTimeMillis() );
                    if (timeLast <= 0)
                    {
                        timeLast = 0;
                    }
                    message.Writer.WriteInt(timeLast);
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateBody in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-93
        public static Message SendNewBackground()
        {
            try
            {
                var bytes = ServerUtils.ReadFileBytes(ServerUtils.ProjectDir(DatabaseManager.Manager.gI().NewBG));
                var message = new Message(-93);
                message.Writer.WriteShort(bytes.Length);
                message.Writer.Write(bytes);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendNewBackground in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-91 _ ME
        public static Message Transport(short maxTime = 10, byte type = 0)
        {
            try
            {
                var message = new Message(-105);
                message.Writer.WriteShort(maxTime);
                message.Writer.WriteByte(type);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Transport in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-91 _ ME
        public static Message MapTranspot(List<MapTranspot> list)
        {
            try
            {
                var message = new Message(-91);
                message.Writer.WriteByte(list.Count);
                list.ForEach(map =>
                {
                    message.Writer.WriteUTF(map.Info);
                    message.Writer.WriteUTF(map.Name);
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MapTranspot in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }


        //-90  _ Zone
        public static Message UpdateBody(ICharacter character)
        {
            try
            {
                var message = new Message(-90);
                message.Writer.WriteByte(0);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteShort(character.GetHead());
                message.Writer.WriteShort(character.GetBody());
                message.Writer.WriteShort(character.GetLeg());
                message.Writer.WriteByte(character.InfoSkill.Monkey.MonkeyId);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateBody in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-87
        public static Message UpdateData()
        {
            try
            {
                var message = new Message(-87);
                message.Writer.WriteByte(DatabaseManager.Manager.gI().DataVersion);
                
                if (!DatabaseManager.Manager.gI().IsDir)
                {
                    message.Writer.WriteInt(Cache.Gi().NR_DART.Count);
                    message.Writer.Write(Cache.Gi().NR_DART);
                    message.Writer.WriteInt(Cache.Gi().NR_ARROW.Count);
                    message.Writer.Write(Cache.Gi().NR_ARROW);
                    message.Writer.WriteInt(Cache.Gi().NR_EFFECT.Count);
                    message.Writer.Write(Cache.Gi().NR_EFFECT);
                    message.Writer.WriteInt(Cache.Gi().NR_IMAGE.Count);
                    message.Writer.Write(Cache.Gi().NR_IMAGE);
                    message.Writer.WriteInt(Cache.Gi().NR_PART.Count);
                    message.Writer.Write(Cache.Gi().NR_PART);
                    message.Writer.WriteInt(Cache.Gi().NR_SKILL.Count);
                    message.Writer.Write(Cache.Gi().NR_SKILL);
                }
                else
                {
                    var bytes = ServerUtils.ReadFileBytes(ServerUtils.ProjectDir(DatabaseManager.Manager.gI().NR_Dart));
                    message.Writer.WriteInt(bytes.Length);
                    message.Writer.Write(bytes);
                
                    bytes = ServerUtils.ReadFileBytes(ServerUtils.ProjectDir(DatabaseManager.Manager.gI().NR_Arrow));
                    message.Writer.WriteInt(bytes.Length);
                    message.Writer.Write(bytes);
                
                    bytes = ServerUtils.ReadFileBytes(ServerUtils.ProjectDir(DatabaseManager.Manager.gI().NR_Effect));
                    message.Writer.WriteInt(bytes.Length);
                    message.Writer.Write(bytes);
                
                    bytes = ServerUtils.ReadFileBytes(ServerUtils.ProjectDir(DatabaseManager.Manager.gI().NR_Image));
                    message.Writer.WriteInt(bytes.Length);
                    message.Writer.Write(bytes);
                
                    bytes = ServerUtils.ReadFileBytes(ServerUtils.ProjectDir(DatabaseManager.Manager.gI().NR_Part));
                    message.Writer.WriteInt(bytes.Length);
                    message.Writer.Write(bytes);
                
                    bytes = ServerUtils.ReadFileBytes(ServerUtils.ProjectDir(DatabaseManager.Manager.gI().NR_Skill));
                    message.Writer.WriteInt(bytes.Length);
                    message.Writer.Write(bytes);
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateData in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-86 _ 0-1 _ ME
        public static Message Trade01(int action, int charId)
        {
            try
            {
                var message = new Message(-86);
                message.Writer.WriteByte(action);
                message.Writer.WriteInt(charId);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Trade01 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-86 _ 2 _ ME
        public static Message Trade2(int index)
        {
            try
            {
                var message = new Message(-86);
                message.Writer.WriteByte(2);
                message.Writer.WriteByte(index);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Trade2 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-86 _ 6 _ ME
        public static Message Trade6(List<Item> items)
        {
            try
            {
                var message = new Message(-86);
                message.Writer.WriteByte(6);
                var gold = items.FirstOrDefault(item => item.Id == 76);
                if (gold == null)
                {
                    message.Writer.WriteInt(0);
                    message.Writer.WriteByte(items.Count);
                }
                else
                {
                    message.Writer.WriteInt(gold.Quantity);
                    message.Writer.WriteByte(items.Count-1);
                }
                items.Where(item => item.Id != 76).ToList().ForEach(item =>
                {
                    message.Writer.WriteShort(item.Id);
                    message.Writer.WriteByte(item.Quantity);
                    message.Writer.WriteByte(item.Options.Count);
                    item.Options.ForEach(option =>
                    {
                        message.Writer.WriteByte(option.Id);
                        message.Writer.WriteShort(option.Param);
                    });
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Trade6 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-86_7
        public static Message ClosePanel()
        {
            try
            {
                var message = new Message(-86);
                message.Writer.WriteByte(7);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ClosePanel in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-82
        public static Message SendTileSet()
        {
            try
            {
                var fileName = ServerUtils.ProjectDir(DatabaseManager.Manager.gI().TileMap);
                var bytes = ServerUtils.ReadFileBytes(fileName);
                var message = new Message(-82);
                message.Writer.Write(bytes);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendTileSet in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-81 _ ME
        public static Message SendCombinne0(List<string> info)
        {
            try
            {
                var message = new Message(-81);
                message.Writer.WriteByte(0);
                message.Writer.WriteUTF(info[0]);
                message.Writer.WriteUTF(info[1]);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendCombinne0 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        public static Message SendCombinne1(List<int> indexs)
        {
            try
            {
                var message = new Message(-81);
                message.Writer.WriteByte(1);
                message.Writer.WriteByte(indexs.Count);
                indexs.ForEach(ind => message.Writer.WriteByte(ind));
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendCombinne1 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        public static Message SendCombinne2()
        {
            try
            {
                var message = new Message(-81);
                message.Writer.WriteByte(2);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendCombinne2 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        public static Message SendCombinne3()
        {
            try
            {
                var message = new Message(-81);
                message.Writer.WriteByte(3);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendCombinne3 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        public static Message SendCombinne4(short iconId)
        {
            try
            {
                var message = new Message(-81);
                message.Writer.WriteByte(4);
                message.Writer.WriteShort(iconId);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendCombinne4 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        public static Message SendCombinne5(short iconId)
        {
            try
            {
                var message = new Message(-81);
                message.Writer.WriteByte(5);
                message.Writer.WriteShort(iconId);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendCombinne5 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        public static Message SendCombinne6(short iconId1, short iconId2)
        {
            try
            {
                var message = new Message(-81);
                message.Writer.WriteByte(6);
                message.Writer.WriteShort(iconId1);
                message.Writer.WriteShort(iconId2);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendCombinne6 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-80 _ ME
        public static Message ListFriend0(List<InfoFriend> list)
        {
            try
            {
                var message = new Message(-80);
                message.Writer.WriteByte(0);
                message.Writer.WriteByte(list.Count);
                list.ForEach(friend =>
                {
                    var isOnline = ClientManager.Gi().GetCharacter(friend.Id) != null;
                    message.Writer.WriteInt(friend.Id);
                    message.Writer.WriteShort(friend.Head);
                    message.Writer.WriteShort(friend.Body);
                    message.Writer.WriteShort(friend.Leg);
                    message.Writer.WriteByte(friend.Bag);
                    message.Writer.WriteUTF((friend.Name != null ? friend.Name : "superhero"));
                    message.Writer.WriteBoolean(isOnline);
                    message.Writer.WriteUTF(ServerUtils.GetPower(friend.Power));
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ListFriend0 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-80 _ ME
        public static Message ListFriend2(int charId)
        {
            try
            {
                var message = new Message(-80);
                message.Writer.WriteByte(2);
                message.Writer.WriteInt(charId);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ListFriend2 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-80 _ ME
        public static Message ListFriend3(int charId, bool isOnline)
        {
            try
            {
                var message = new Message(-80);
                message.Writer.WriteByte(3);
                message.Writer.WriteInt(charId);
                message.Writer.WriteBool(isOnline);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ListFriend3 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-80 _ ME
        public static Message MenuPlayer(int charId, long power, string info)
        {
            try
            {
                var message = new Message(-79);
                message.Writer.WriteInt(charId);
                message.Writer.WriteLong(power);
                message.Writer.WriteUTF(info);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MenuPlayer in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-77 _ ME
        public static Message SendNewImage(int zoomLevel)
        {
            if (zoomLevel is < 1 or > 4) zoomLevel = 2;
            try
            {
                var message = new Message(-77);
                switch(zoomLevel)
                {
                    case 1:
                    {
                        message.Writer.WriteShort(Cache.Gi().VersionIconX1.Count);
                        message.Writer.Write(Cache.Gi().VersionIconX1);
                        break;
                    }
                    case 2:
                    {
                        message.Writer.WriteShort(Cache.Gi().VersionIconX2.Count);
                        message.Writer.Write(Cache.Gi().VersionIconX2);
                        break;
                    }
                    case 3:
                    {
                        message.Writer.WriteShort(Cache.Gi().VersionIconX3.Count);
                        message.Writer.Write(Cache.Gi().VersionIconX3);
                        break;
                    }
                    case 4:
                    {
                        message.Writer.WriteShort(Cache.Gi().VersionIconX4.Count);
                        message.Writer.Write(Cache.Gi().VersionIconX4);
                        break;
                    }
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendNewImage in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-74 _ ME
        public static Message GetImageResource()
        {
            try
            {
                var message = new Message(-74);
                message.Writer.WriteByte(0);
                message.Writer.WriteInt(DatabaseManager.Manager.gI().DownloadVersion);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error GetImageResource in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-74_1
        public static Message SendImageResource1(int zoomLevel)
        {
            if (zoomLevel is < 1 or > 4) zoomLevel = 2;
            try
            {
                var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().Download, zoomLevel));
                var count = new DirectoryInfo(path).GetFiles().Length;
                var message = new Message(-74);
                message.Writer.WriteByte(1);
                message.Writer.WriteShort(count);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendImageResource1 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-74_2
        public static bool SendImageResource2(ISession_ME session)
        {
            if (session.ZoomLevel is < 1 or > 4) session.ZoomLevel = 2;
            var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().Download, session.ZoomLevel));
            var dir = new DirectoryInfo(path).GetFiles();
            var count = 0;
            var pathName = "bg";
            
            if (session.ZoomLevel == 4) 
            {
                pathName = "background";
            }
            
            foreach (var fileInfo in dir)
            {
                var fileName = $"/{pathName}/{fileInfo.Name.Replace($"x{session.ZoomLevel}", "")}";
                var bytes = ServerUtils.ReadFileBytes($"{fileInfo.DirectoryName}/{fileInfo.Name}");
                if (bytes == null) continue;
                try
                {
                    var message = new Message(-74);
                    message.Writer.WriteByte(2);
                    message.Writer.WriteUTF(fileName);
                    message.Writer.WriteInt(bytes.Length);
                    message.Writer.Write(bytes);
                    session.SendMessage(message);
                    count++;
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error SendImageResource2 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                    return false;
                }
            }
            return count == dir.Length;
        }

        public static async Task<bool> SendImageResource2Async(ISession_ME session)
        {
            if (session.ZoomLevel is < 1 or > 4) session.ZoomLevel = 2;
            var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().Download, session.ZoomLevel));
            var dir = new DirectoryInfo(path).GetFiles();
            var count = 0;
            var pathName = "bg";
            
            if (session.ZoomLevel == 4) 
            {
                pathName = "background";
            }

            foreach (var fileInfo in dir)
            {
                var fileName = $"/{pathName}/{fileInfo.Name.Replace($"x{session.ZoomLevel}", "")}";

                var bytes = ServerUtils.ReadFileBytes($"{fileInfo.DirectoryName}/{fileInfo.Name}");
                if (bytes == null) continue;
                try
                {
                    var message = new Message(-74);
                    message.Writer.WriteByte(2);
                    message.Writer.WriteUTF(fileName);
                    message.Writer.WriteInt(bytes.Length);
                    message.Writer.Write(bytes);
                    session.SendMessage(message);
                    count++;
                    await Task.Delay(0);
                }
                catch (Exception e)
                {
                    Server.Gi().Logger.Error($"Error SendImageResource2 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                    return false;
                }
            }
            return count == dir.Length;
        }


        //-74_3
        public static void SendImageResource3(ISession_ME session)
        {
            try
            {
                var message = new Message(-74);
                message.Writer.WriteByte(3);
                message.Writer.WriteInt(DatabaseManager.Manager.gI().DownloadVersion);
                session.SendMessage(message);
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendImageResource3 in Service.cs: {e.Message} \n {e.StackTrace}", e);
            }
        }
        
        //-70
        public static Message BigMessage()
        {
            try
            {
                var message = new Message(-70);
                message.Writer.WriteShort(1139);
                message.Writer.WriteUTF(Cache.Gi().GAME_INFO_TEMPLATES[0].Content);
                message.Writer.WriteByte(0);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error BigMessage in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-69
        public static Message SendMaxStamina(short maxStamina)
        {
            Message message;
            try
            {
                message = new Message(-69);
                message.Writer.WriteShort(maxStamina);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendMaxStamina in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-68
        public static Message SendStamina(short stamina)
        {
            try
            {
                var message = new Message(-68);
                message.Writer.WriteShort(stamina);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendStamina in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-67
        public static Message SendIcon(int zoomLevel, int id)
        {
            if (zoomLevel is < 1 or > 4) zoomLevel = 2;
            try
            {
                var bytes = new byte[]{};
                switch (zoomLevel)
                {
                    case 1:
                    {
                        if (Cache.Gi().DATA_ICON_X1.ContainsKey(id))
                        {
                            bytes = Cache.Gi().DATA_ICON_X1[id].ToArray();
                        }
                        else
                        {
                            var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().IconImg, zoomLevel, id));
                            bytes = ServerUtils.ReadFileBytes(path);
                            Cache.Gi().DATA_ICON_X1.TryAdd(id, bytes.ToList());
                        }
                        break;
                    }
                    case 2:
                    {
                        if (Cache.Gi().DATA_ICON_X2.ContainsKey(id))
                        {
                            bytes = Cache.Gi().DATA_ICON_X2[id].ToArray();
                        }
                        else
                        {
                            var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().IconImg, zoomLevel, id));
                            bytes = ServerUtils.ReadFileBytes(path);
                            Cache.Gi().DATA_ICON_X2.TryAdd(id, bytes.ToList());
                        }
                        break;
                    }
                    case 3:
                    {
                        if (Cache.Gi().DATA_ICON_X3.ContainsKey(id))
                        {
                            bytes = Cache.Gi().DATA_ICON_X3[id].ToArray();
                        }
                        else
                        {
                            var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().IconImg, zoomLevel, id));
                            bytes = ServerUtils.ReadFileBytes(path);
                            Cache.Gi().DATA_ICON_X3.TryAdd(id, bytes.ToList());
                        }
                        break;
                    }
                    case 4:
                    {
                        if (Cache.Gi().DATA_ICON_X4.ContainsKey(id))
                        {
                            bytes = Cache.Gi().DATA_ICON_X4[id].ToArray();
                        }
                        else
                        {
                            var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().IconImg, zoomLevel, id));
                            bytes = ServerUtils.ReadFileBytes(path);
                            Cache.Gi().DATA_ICON_X4.TryAdd(id, bytes.ToList());
                        }
                        break;
                    }
                }
                var message = new Message(-67);
                message.Writer.WriteInt(id);
                message.Writer.WriteInt(bytes.Length);
                message.Writer.Write(bytes);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendIcon in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-66
        public static Message SendEffect(int zoomLevel, short id)
        {
            if (zoomLevel < 1 || zoomLevel > 4) zoomLevel = 2;
            try
            {
                var bytes = ServerUtils.ReadFileBytes(ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().EffectData, zoomLevel, id)));
                var bytes2 = ServerUtils.ReadFileBytes(ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().EffectImg, zoomLevel, id)));
                var message = new Message(-66);
                message.Writer.WriteShort(id);
                message.Writer.WriteInt(bytes.Length);
                message.Writer.Write(bytes);
                message.Writer.WriteInt(bytes2.Length);
                message.Writer.Write(bytes2);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendEffect in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-65 _ Zone
        public static Message SendTeleport(int charId, int teleport)
        {
            try
            {
                var message = new Message(-65);
                message.Writer.WriteInt(charId);
                message.Writer.WriteByte(teleport); //0, 1, 2, 3
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendTeleport in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-64 _ Zone
        public static Message SendImageBag(int charId, int id)
        {
            try
            {
                var message = new Message(-64);
                message.Writer.WriteInt(charId);
                message.Writer.WriteByte(id);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendImageBag in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-63 _ Me
        public static Message GetImageBag(ClanImage clanImage)
        {
            try
            {
                var message = new Message(-63);
                if (clanImage == null)
                {
                    message.Writer.WriteByte(-1);
                    message.Writer.WriteByte(0);
                    return message;
                }
                message.Writer.WriteByte(clanImage.Id);
                message.Writer.WriteByte(clanImage.Data.Count-1);
                var count = 0;
                foreach (var s in clanImage.Data)
                {
                    if (count == 0)
                    {
                        count++;
                        continue;
                    }
                    message.Writer.WriteShort(s);
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error GetImageBag in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-62 _ ME
        public static Message SendClanImage(ClanImage clanImage)
        {
            try
            {
                var message = new Message(-62);
                message.Writer.WriteByte(clanImage.Id);
                message.Writer.WriteByte(clanImage.Data.Count);
                clanImage.Data.ForEach(icon => message.Writer.WriteShort(icon));
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendClanImage in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-61 _ Zone
        public static Message UpdateClanId(int charId, int id)
        {
            try
            {
                var message = new Message(-61);
                message.Writer.WriteInt(charId);
                message.Writer.WriteInt(id);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateClanId in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-60 _ Zone
        public static Message PlayerAttackPlayer(int charId, List<ICharacter> characters, int skillId)
        {
            try
            {
                var message = new Message(-60);
                message.Writer.WriteInt(charId);
                message.Writer.WriteByte(skillId);
                message.Writer.WriteByte(characters.Count);
                characters.ForEach(c => message.Writer.WriteInt(c.Id));
                message.Writer.WriteByte(0); //continue
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerAttackPlayer in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-59
        public static Message PlayerVsPLayer(int typePk, int charId, int gold, string text)
        {
            try
            {
                var message = new Message(-59);
                message.Writer.WriteByte(typePk);
                message.Writer.WriteInt(charId);
                message.Writer.WriteInt(gold);
                message.Writer.WriteUTF(text);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerVsPLayer in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-57 invite clan
        public static Message InviteClan(string text, int clanId, int code)
        {
            try
            {
                var message = new Message(-57);
                message.Writer.WriteUTF(text);
                message.Writer.WriteInt(clanId);
                message.Writer.WriteInt(code);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error InviteClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-53 CLAN_INFO
        public static Message MyClanInfo()
        {
            try
            {
                var message = new Message(-53);
                message.Writer.WriteInt(-1);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MyClanInfo in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;                                                                    
            }
        }

        public static Message MyClanInfo(Character character)
        {
            try
            {
                var message = new Message(-53);
                message.Writer.WriteInt(character.ClanId);
                if (character.ClanId != -1)
                {
                    var clan = ClanManager.Get(character.ClanId);
                    if (clan == null) return null;
                    var memberChar = clan.ClanHandler.GetMember(character.Id);
                    if (memberChar == null) return MyClanInfo();
                    message.Writer.WriteUTF(clan.Name);
                    message.Writer.WriteUTF(clan.Slogan);
                    message.Writer.WriteByte(clan.ImgId);
                    message.Writer.WriteUTF(ServerUtils.GetPower(clan.Power));
                    message.Writer.WriteUTF(clan.LeaderName);
                    message.Writer.WriteByte(clan.Members.Count);
                    message.Writer.WriteByte(clan.MaxMember);
                    message.Writer.WriteByte(memberChar.Role);//role
                    message.Writer.WriteInt(clan.Point);
                    message.Writer.WriteByte(clan.Level);
                    clan.Members.ToList().ForEach(member =>
                    {
                        message.Writer.WriteInt(member.Id);
                        message.Writer.WriteShort(member.Head);
                        message.Writer.WriteShort(member.Leg);
                        message.Writer.WriteShort(member.Body);
                        message.Writer.WriteUTF(member.Name);
                        message.Writer.WriteByte(member.Role);
                        message.Writer.WriteUTF(ServerUtils.GetPower(member.Power));//role
                        message.Writer.WriteInt(member.Donate);
                        message.Writer.WriteInt(member.ReceiveDonate);
                        message.Writer.WriteInt(member.ClanPoint);
                        message.Writer.WriteInt(member.CurClanPoint);
                        message.Writer.WriteInt(member.JoinTime);
                    });
                    message.Writer.WriteByte(clan.Messages.Count);
                    clan.Messages.OrderByDescending (msg => msg.Id).ToList().ForEach(msg =>
                    {
                        message.Writer.WriteByte(msg.Type);
                        message.Writer.WriteInt(msg.Id);
                        message.Writer.WriteInt(msg.PlayerId);
                        message.Writer.WriteUTF(msg.PlayerName);
                        message.Writer.WriteByte(msg.Role);
                        message.Writer.WriteInt(msg.Time);
                        if (msg.Type == 0)
                        {
                            message.Writer.WriteUTF(msg.Text);
                            message.Writer.WriteByte(msg.Color);
                        }
                        else if (msg.Type == 1)
                        {
                            message.Writer.WriteByte(msg.Recieve);
                            message.Writer.WriteByte(msg.MaxCap);
                            message.Writer.WriteByte(msg.NewMessage ? 1 : 0);
                        }
                    });
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MyClanInfo in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-52 _ Add Member
        public static Message AddMemberClan(ClanMember member)
        {
            try
            {
                var message = new Message(-52);
                message.Writer.WriteByte(0);
                message.Writer.WriteInt(member.Id);
                message.Writer.WriteShort(member.Head);
                message.Writer.WriteShort(member.Leg);
                message.Writer.WriteShort(member.Body);
                message.Writer.WriteUTF(member.Name);
                message.Writer.WriteByte(member.Role);
                message.Writer.WriteUTF(ServerUtils.GetPower(member.Power));//role
                message.Writer.WriteInt(member.Donate);
                message.Writer.WriteInt(member.ReceiveDonate);
                message.Writer.WriteInt(member.ClanPoint);
                message.Writer.WriteInt(member.JoinTime);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error AddMemberClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        public static Message RemoveMemberClan(int index)
        {
            try
            {
                var message = new Message(-52);
                message.Writer.WriteByte(1);
                message.Writer.WriteByte(index);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error RemoveMemberClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        public static Message ChangeMemberClan(ClanMember member)
        {
            try
            {
                var message = new Message(-52);
                message.Writer.WriteByte(2);
                message.Writer.WriteInt(member.Id);
                message.Writer.WriteShort(member.Head);
                message.Writer.WriteShort(member.Leg);
                message.Writer.WriteShort(member.Body);
                message.Writer.WriteUTF(member.Name);
                message.Writer.WriteByte(member.Role);
                message.Writer.WriteUTF(ServerUtils.GetPower(member.Power));//role
                message.Writer.WriteInt(member.Donate);
                message.Writer.WriteInt(member.ReceiveDonate);
                message.Writer.WriteInt(member.ClanPoint);
                message.Writer.WriteInt(member.JoinTime);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ChangeMemberClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-51 CLAN_MESSAGE
        public static Message ClanMessage(ClanMessage msg)
        {
            try
            {
                var message = new Message(-51);
                message.Writer.WriteByte(msg.Type);
                message.Writer.WriteInt(msg.Id);
                message.Writer.WriteInt(msg.PlayerId);
                message.Writer.WriteUTF(msg.PlayerName);
                message.Writer.WriteByte(msg.Role);
                message.Writer.WriteInt(msg.Time);
                switch (msg.Type)
                {
                    case 0:
                        message.Writer.WriteUTF(msg.Text);
                        message.Writer.WriteByte(msg.Color);
                        break;
                    case 1:
                        message.Writer.WriteByte(msg.Recieve);
                        message.Writer.WriteByte(msg.MaxCap);
                        message.Writer.WriteByte(msg.NewMessage ? 1 : 0);
                        break;
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ClanMessage in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-50 _ ME
        public static Message ClanMember(List<ClanMember> members)
        {
            try
            {
                var message = new Message(-50);
                message.Writer.WriteByte(members.Count);
                members.ToList().ForEach(member =>
                {
                    message.Writer.WriteInt(member.Id);
                    message.Writer.WriteShort(member.Head);
                    message.Writer.WriteShort(member.Leg);
                    message.Writer.WriteShort(member.Body);
                    message.Writer.WriteUTF(member.Name);
                    message.Writer.WriteByte(member.Role);
                    message.Writer.WriteUTF(ServerUtils.GetPower(member.Power));
                    message.Writer.WriteInt(member.Donate);
                    message.Writer.WriteInt(member.ReceiveDonate);
                    message.Writer.WriteInt(member.ClanPoint);
                    message.Writer.WriteInt(member.JoinTime);
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ClanMember in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-47 _ ME
        public static Message ClanSearch(List<Clan> clans)
        {
            try
            {
                var message = new Message(-47);
                message.Writer.WriteByte(clans.Count);
                clans.ForEach(clan =>
                {
                    message.Writer.WriteInt(clan.Id);
                    message.Writer.WriteUTF(clan.Name);
                    message.Writer.WriteUTF(clan.Slogan);
                    message.Writer.WriteByte(clan.ImgId);
                    message.Writer.WriteUTF(ServerUtils.GetPower(clan.Power));
                    message.Writer.WriteUTF(clan.LeaderName);
                    message.Writer.WriteByte(clan.CurrMember);
                    message.Writer.WriteByte(clan.MaxMember);
                    message.Writer.WriteInt(clan.Date);
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ClanSearch in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-46 _ ME
        public static Message CreateClan(int type)
        {
            Message message;
            try
            {
                message = new Message(-46);
                message.Writer.WriteByte(type);
                switch (type)
                {
                    case 1:
                    case 3:
                    {
                        message.Writer.WriteByte(29);
                        Cache.Gi().CLAN_IMAGES.ForEach(image =>
                        {
                            if (image.Id >= 30) return;
                            message.Writer.WriteByte(image.Id);
                            message.Writer.WriteUTF(image.Name);
                            message.Writer.WriteInt(image.Gold);
                            message.Writer.WriteInt(image.Diamond);
                        });
                        break;
                    }
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error CreateClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-46 _ ME
        public static Message UpdateClan(int id, string slogan)
        {
            try
            {
                var message = new Message(-46);
                message.Writer.WriteByte(4);
                message.Writer.WriteByte(id);
                message.Writer.WriteUTF(slogan);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-45 _ 0 _ Zone
        public static Message SkillNotFocus0(int charId, int id, List<ICharacter> characters, List<IMonster> monsters)
        {
            try
            {
                var message = new Message(-45);
                message.Writer.WriteByte(0);
                message.Writer.WriteInt(charId);
                message.Writer.WriteShort(id);
                message.Writer.WriteByte(monsters.Count);
                monsters.ForEach(m =>
                {
                    message.Writer.WriteByte(m.IdMap);
                    message.Writer.WriteByte(m.InfoSkill.ThaiDuongHanSan.TimeReal);
                });
                message.Writer.WriteByte(characters.Count);
                characters.ForEach(c =>
                {
                    message.Writer.WriteInt(c.Id);
                    message.Writer.WriteByte(c.InfoSkill.ThaiDuongHanSan.TimeReal);
                });
                characters.Clear();
                monsters.Clear();
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-45 _ 1 _ Zone
        public static Message SkillNotFocus1(int charId, int id)
        {
            try
            {
                var message = new Message(-45);
                message.Writer.WriteByte(1);
                message.Writer.WriteInt(charId);
                message.Writer.WriteShort(id);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-45 _ 3 _ Zone
        public static Message SkillNotFocus3(int charId, int id)
        {
            try
            {
                var message = new Message(-45);
                message.Writer.WriteByte(3);
                message.Writer.WriteInt(charId);
                message.Writer.WriteShort(id);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-45 _ 4 _ Zone
        public static Message SkillNotFocus4(int charId, int id, short time)
        {
            try
            {
                var message = new Message(-45);
                message.Writer.WriteByte(4);
                message.Writer.WriteInt(charId);
                message.Writer.WriteShort(id);
                message.Writer.WriteShort(time);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-45 _ 6 _ Zone
        public static Message SkillNotFocus6(int charId, int id)
        {
            try
            {
                var message = new Message(-45);
                message.Writer.WriteByte(6);
                message.Writer.WriteInt(charId);
                message.Writer.WriteShort(id);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-45 _ 7 _ Zone
        public static Message SkillNotFocus7(int charId, int id, int time)
        {
            try
            {
                var message = new Message(-45);
                message.Writer.WriteByte(7);
                message.Writer.WriteInt(charId);
                message.Writer.WriteShort(id);
                message.Writer.WriteShort(time);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-45 _ 8 _ Zone
        public static Message SkillNotFocus8(int charId, int id)
        {
            try
            {
                var message = new Message(-45);
                message.Writer.WriteByte(8);
                message.Writer.WriteInt(charId);
                message.Writer.WriteShort(id);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateClan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-44
        public static Message Shop(Character character, int typeUi, int idShop)
        {
            Message message;
            try
            {
                var shopTemplates = Cache.Gi().SHOP_TEMPLATES.FirstOrDefault(s => s.Key == idShop).Value;
                if (shopTemplates == null) return null;
                message = new Message(-44);
                message.Writer.WriteByte(typeUi);
                message.Writer.WriteByte(shopTemplates.Count);
                switch (typeUi)
                {
                    case 0:
                    {
                        shopTemplates.ForEach(shop =>
                        {
                            message.Writer.WriteUTF(shop.Name);
                            message.Writer.WriteByte(shop.ItemShops.Count);
                            shop.ItemShops.ForEach(item =>
                            {
                                var iditem = item.Id;
                                var options = item.Options;
                                var buygold = item.BuyGold;
                                var itemTemplate = ItemCache.ItemTemplate(iditem);
                                if (DataCache.IdDauThanx30.Contains(iditem))
                                {
                                    var levelMagic = (MagicTreeManager.Get(character.Id) != null ? MagicTreeManager.Get(character.Id).Level : 1);
                                    if (levelMagic == 1) levelMagic = 2;
                                    iditem = (short)DataCache.IdDauThanx30[levelMagic - 2];
                                    itemTemplate = ItemCache.ItemTemplate(iditem);
                                    options = itemTemplate.Options.Copy();
                                    var index = DataCache.IdDauThanx30.IndexOf(iditem);
                                    if (index == 0) index = 1;
                                    buygold *= index;
                                }
                                var percent = 1;
                                switch (item.Id)
                                {
                                    //Kỹ năng đệ tử 1
                                    case 402:
                                    {
                                        if (character.Disciple != null)
                                        {
                                            percent *= character.Disciple.Skills[0].Point;
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
                                                percent *= character.Disciple.Skills[1].Point;
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
                                                percent *= character.Disciple.Skills[2].Point;
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
                                                percent *= character.Disciple.Skills[3].Point;
                                            }
                                        }
                                        break;
                                    }
                                    case 517:
                                        percent += character.PlusBag;
                                        break;
                                    case 518:
                                        percent += character.PlusBox;
                                        break;
                                }
                                message.Writer.WriteShort(iditem);
                                message.Writer.WriteInt(item.BuyCoin * percent);
                                message.Writer.WriteInt(buygold * percent);

                                if (idShop < 4 && character.InfoChar.ItemAmulet.ContainsKey(item.Id))
                                {
                                    var time  = character.InfoChar.ItemAmulet.FirstOrDefault(i => i.Key == item.Id).Value;
                                    var timeLeft = time - ServerUtils.CurrentTimeMillis();
                                    if (timeLeft > 0)
                                    {
                                        var timeAmulet = ServerUtils.GetTimeAmulet(timeLeft);
                                        message.Writer.WriteByte(1);
                                        message.Writer.WriteByte(timeAmulet[0]);
                                        message.Writer.WriteShort(timeAmulet[1]);
                                    }
                                    else
                                    {
                                        message.Writer.WriteByte(item.Options.Count);
                                        item.Options.ForEach(option =>
                                        {
                                            message.Writer.WriteByte(option.Id);
                                            message.Writer.WriteShort(option.Param);
                                        });
                                    }
                                }
                                else
                                {
                                    message.Writer.WriteByte(item.Options.Count);
                                    options.ForEach(option =>
                                    {
                                        message.Writer.WriteByte(option.Id);
                                        message.Writer.WriteShort(option.Param);
                                    });
                                }
                                
                                message.Writer.WriteByte(item.IsNewItem ? 1 : 0);
                                if (itemTemplate.IsTypeBody())
                                {
                                    message.Writer.WriteByte(1); // send part
                                    if (itemTemplate.Type == 5)
                                    {
                                        if (ItemCache.IsItemAvtNotHead(item.HeadTemp))
                                        {
                                            message.Writer.WriteShort(item.HeadTemp);
                                            message.Writer.WriteShort(item.BodyTemp);
                                            message.Writer.WriteShort(item.LegTemp);
                                            message.Writer.WriteShort(item.BagTemp);
                                        }
                                        else
                                        {
                                            message.Writer.WriteShort(item.HeadTemp);
                                            message.Writer.WriteShort(item.BodyTemp == -1 ? ItemCache.PartHeadToBody(item.HeadTemp) : item.BodyTemp);
                                            message.Writer.WriteShort(item.LegTemp == -1 ? ItemCache.PartHeadToLeg(item.HeadTemp) : item.LegTemp);
                                            message.Writer.WriteShort(item.BagTemp);
                                        }
                                    }
                                    else
                                    {
                                        message.Writer.WriteShort(character.GetHead());
                                        message.Writer.WriteShort(itemTemplate.Type == 0 ? itemTemplate.Part : character.GetBody());
                                        message.Writer.WriteShort(itemTemplate.Type == 1 ? itemTemplate.Part : character.GetLeg());
                                        message.Writer.WriteShort(itemTemplate.Type == 11 ? itemTemplate.Part : character.GetBag());
                                    }
                                }
                                else
                                {
                                    message.Writer.WriteByte(0);
                                }
                            });
                        });
                        break;
                    }
                    case 1:
                    {
                        shopTemplates.ForEach(shop =>
                        {
                            var items = shop.ItemShops.Where(i => !character.BoughtSkill.Contains(i.Id)).ToList();
                            
                            message.Writer.WriteUTF(shop.Name);
                            message.Writer.WriteByte(items.Count);
                            items.ForEach(item =>
                            {
                                message.Writer.WriteShort(item.Id); //id
                                message.Writer.WriteLong(item.Power); //power
                                message.Writer.WriteByte(item.Options.Count);
                                item.Options.ForEach(option =>
                                {
                                    message.Writer.WriteByte(option.Id);
                                    message.Writer.WriteShort(option.Param);
                                });
                                message.Writer.WriteByte(item.IsNewItem ? 1 : 0); //new item
                                message.Writer.WriteByte(0); //part
                            });
                        });
                        break;
                    }
                    case 3:
                    {
                        shopTemplates.ForEach(shop =>
                        {
                            message.Writer.WriteUTF(shop.Name);
                            message.Writer.WriteByte(shop.ItemShops.Count);
                            shop.ItemShops.ForEach(item =>
                            {
                                var iditem = item.Id;
                                var options = item.Options;
                                var buygold = item.BuyGold;
                                var itemTemplate = ItemCache.ItemTemplate(iditem);
                                
                                message.Writer.WriteShort(iditem);
                                message.Writer.WriteShort(item.BuyGold);
                                message.Writer.WriteInt(item.BuyCoin);

                                message.Writer.WriteByte(item.Options.Count);

                                options.ForEach(option =>
                                {
                                    message.Writer.WriteByte(option.Id);
                                    message.Writer.WriteShort(option.Param);
                                });
                                
                                message.Writer.WriteByte(item.IsNewItem ? 1 : 0);
                                if (itemTemplate.IsTypeBody())
                                {
                                    message.Writer.WriteByte(1); // send part
                                    if (itemTemplate.Type == 5)
                                    {
                                        if (ItemCache.IsItemAvtNotHead(item.HeadTemp))
                                        {
                                            message.Writer.WriteShort(item.HeadTemp);
                                            message.Writer.WriteShort(item.BodyTemp);
                                            message.Writer.WriteShort(item.LegTemp);
                                            message.Writer.WriteShort(item.BagTemp);
                                        }
                                        else
                                        {
                                            message.Writer.WriteShort(item.HeadTemp);
                                            message.Writer.WriteShort(item.BodyTemp == -1 ? ItemCache.PartHeadToBody(item.HeadTemp) : item.BodyTemp);
                                            message.Writer.WriteShort(item.LegTemp == -1 ? ItemCache.PartHeadToLeg(item.HeadTemp) : item.LegTemp);
                                            message.Writer.WriteShort(item.BagTemp);
                                        }
                                    }
                                    else
                                    {
                                        message.Writer.WriteShort(character.GetHead());
                                        message.Writer.WriteShort(itemTemplate.Type == 0 ? itemTemplate.Part : character.GetBody());
                                        message.Writer.WriteShort(itemTemplate.Type == 1 ? itemTemplate.Part : character.GetLeg());
                                        message.Writer.WriteShort(itemTemplate.Type == 11 ? itemTemplate.Part : character.GetBag());
                                    }
                                }
                                else
                                {
                                    message.Writer.WriteByte(0);
                                }
                            });
                        });
                        break;
                    }
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Shop in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        public static Message SubBox(List<Item> items)
        {
            Message message;
            try
            {
                message = new Message(-44);
                message.Writer.WriteByte(4);
                message.Writer.WriteByte(1);
                message.Writer.WriteUTF("Rương\nphụ");
                if (items.Count < 100)
                {
                    message.Writer.WriteByte(items.Count);
                }
                else 
                {
                    message.Writer.WriteByte(100);
                }

                for(int i = 0; i < items.Count(); ++i)
                {
                    if (i > 100) continue;
                    var item = items.ElementAt(i);
                    /* .... */
                    message.Writer.WriteShort(item.Id);
                    message.Writer.WriteUTF(item.Reason);
                    message.Writer.WriteByte(item.Options.Count);
                    item.Options.ForEach(op =>
                    {
                        message.Writer.WriteByte(op.Id);
                        message.Writer.WriteShort(op.Param);
                    });
                    message.Writer.WriteByte(0);
                    message.Writer.WriteByte(0);
                }

                // items.ForEach(item =>
                // {
                //     message.Writer.WriteShort(item.Id);
                //     message.Writer.WriteUTF(item.Reason);
                //     message.Writer.WriteByte(item.Options.Count);
                //     item.Options.ForEach(op =>
                //     {
                //         message.Writer.WriteByte(op.Id);
                //         message.Writer.WriteShort(op.Param);
                //     });
                //     message.Writer.WriteByte(0);
                //     message.Writer.WriteByte(0);
                // });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SubBox in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-43
        public static Message UseItem(int action, int where, int index, string info)
        {
            Message message;
            try
            {
                message = new Message(-43);
                message.Writer.WriteByte(action);
                message.Writer.WriteByte(where);
                message.Writer.WriteByte(index);
                message.Writer.WriteUTF(info);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UseItem in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-42 _ ME
        public static Message MeLoadPoint(ICharacter character)
        {
            if (character == null) return null;
            try
            {
                var message = new Message(-42);
                message.Writer.WriteInt((int)character.InfoChar.OriginalHp);
                message.Writer.WriteInt((int)character.InfoChar.OriginalMp);
                message.Writer.WriteInt(character.InfoChar.OriginalDamage);
                message.Writer.WriteInt((int)character.HpFull);
                message.Writer.WriteInt((int)character.MpFull);
                if (character.InfoChar.Hp >= character.HpFull)
                {
                    character.InfoChar.Hp = character.HpFull;
                }
                if (character.InfoChar.Mp >= character.MpFull)
                {
                    character.InfoChar.Mp = character.MpFull;
                }
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteInt((int)character.InfoChar.Mp);
                message.Writer.WriteByte(character.InfoChar.Speed);
                message.Writer.WriteByte(character.InfoChar.HpFrom1000);
                message.Writer.WriteByte(character.InfoChar.MpFrom1000);
                message.Writer.WriteByte(character.InfoChar.DamageFrom1000);
                message.Writer.WriteInt(character.DamageFull);
                message.Writer.WriteInt(character.DefenceFull);
                message.Writer.WriteByte(character.CritFull);
                message.Writer.WriteLong(character.InfoChar.Potential);
                message.Writer.WriteShort(character.InfoChar.Exp);
                message.Writer.WriteShort(character.InfoChar.OriginalDefence);
                message.Writer.WriteByte(character.InfoChar.OriginalCrit);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MeLoadPoint in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-41
        public static Message SendLevelCaption(int gender)
        {
            try
            {
                var levels = Cache.Gi().LEVELS.Where(x => x.Gender == gender).ToList();
                var message = new Message(-41);
                message.Writer.WriteByte(levels.Count);
                levels.ForEach(level => message.Writer.WriteUTF(level.Name));
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendLevelCaption in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-37 _ ME
        public static Message SendBody(ICharacter character, int type = 0)
        {
            Message message;
            try
            {
                message = new Message(-37);
                message.Writer.WriteByte(type);
                message.Writer.WriteShort(character.GetHead());
                message.Writer.WriteByte(character.ItemBody.Count);
                
                character.ItemBody.ForEach(item =>
                {
                    if(item == null) message.Writer.WriteShort(-1);
                    else
                    {
                        var itemTemplate = ItemCache.ItemTemplate(item.Id);
                        message.Writer.WriteShort(item.Id);
                        message.Writer.WriteInt(item.Quantity);
                        message.Writer.WriteUTF(itemTemplate.Name);
                        message.Writer.WriteUTF(itemTemplate.Description);
                        message.Writer.WriteByte(item.Options.Count);
                        item.Options.ForEach(op =>
                        {
                            message.Writer.WriteByte(op.Id);
                            message.Writer.WriteShort(op.Param); 
                        });
                    }
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendBody in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-36 _ ME
        public static Message SendBag(ICharacter character, int type = 0, int index = -1, int quantity = 0)
        {
            Message message;
            try
            {
                message = new Message(-36);
                message.Writer.WriteByte(type);
                if (type == 0)
                {
                    character.CharacterHandler.BagSort();
                    if (character.BagLength() >= character.ItemBag.Count)
                    {
                        message.Writer.WriteByte(character.BagLength());
                    }
                    else 
                    {
                        message.Writer.WriteByte(character.ItemBag.Count);
                    }
                    ItemTemplate itemTemplate;
                    character.ItemBag.ForEach(item =>
                    {
                        itemTemplate = ItemCache.ItemTemplate(item.Id);
                        message.Writer.WriteShort(item.Id);
                        message.Writer.WriteInt(item.Quantity);
                        message.Writer.WriteUTF(itemTemplate.Name);
                        message.Writer.WriteUTF(itemTemplate.Description);
                        message.Writer.WriteByte(item.Options.Count);
                        item.Options.ForEach(op =>
                        {
                            message.Writer.WriteByte(op.Id);
                            message.Writer.WriteShort(op.Param); 
                        });
                    });
                    for(var i = character.ItemBag.Count; i < character.BoxLength(); i++)
                    {
                        message.Writer.WriteShort(-1);
                    }
                }
                else
                {
                    message.Writer.WriteByte(index);
                    message.Writer.WriteByte(quantity);
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendBag in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-35 _ ME
        public static Message SendBox(ICharacter character, int type = 0, bool isBoxClan = false)
        {
            Message message;
            try
            {
                message = new Message(-35);
                message.Writer.WriteByte(type);
                if (type == 0)
                {
                    character.CharacterHandler.BoxSort();
                    if (character.BoxLength() >= character.ItemBox.Count)
                    {
                        message.Writer.WriteByte(character.BoxLength());
                    }
                    else 
                    {
                        message.Writer.WriteByte(character.ItemBox.Count);
                    }
                    ItemTemplate itemTemplate;
                    character.ItemBox.ForEach(item =>
                    {
                        itemTemplate = ItemCache.ItemTemplate(item.Id);
                        message.Writer.WriteShort(item.Id);
                        message.Writer.WriteInt(item.Quantity);
                        message.Writer.WriteUTF(itemTemplate.Name);
                        message.Writer.WriteUTF(itemTemplate.Description);
                        message.Writer.WriteByte(item.Options.Count);
                        item.Options.ForEach(op =>
                        {
                            message.Writer.WriteByte(op.Id);
                            message.Writer.WriteShort(op.Param); 
                        });
                    });
                    for(int i = character.ItemBox.Count; i < character.BoxLength(); i++)
                    {
                        message.Writer.WriteShort(-1);
                    }
                }
                else
                {
                    message.Writer.WriteByte(1);
                    message.Writer.WriteByte(isBoxClan ? 1 : 0);
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendBox in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-34_0
        public static Message MagicTree0(int id)
        {
            var magicTree = MagicTreeManager.Get(id);
            if (magicTree == null)
            {
                return null;
            }
            try
            {
                var dataPosition = Cache.Gi().DATA_MAGICTREE[magicTree.Level];
                var second = (magicTree.Seconds - ServerUtils.CurrentTimeMillis()) / 1000;
                var message = new Message(-34);
                message.Writer.WriteByte(0);
                message.Writer.WriteShort(magicTree.NpcId);
                message.Writer.WriteUTF($"Đậu thần cấp {magicTree.Level}");
                message.Writer.WriteShort(magicTree.X);
                message.Writer.WriteShort(magicTree.Y);
                message.Writer.WriteByte(magicTree.Level);
                message.Writer.WriteShort(magicTree.Peas);
                message.Writer.WriteShort(magicTree.MaxPea);
                message.Writer.WriteUTF(magicTree.IsUpdate ? "Đang trong thời gian nâng cấp" : "Đang kết hạt\nCây lớn sinh nhiều hạt hơn");
                message.Writer.WriteInt((int)second);
                message.Writer.WriteByte(dataPosition.Count);
                dataPosition.ForEach(data =>
                {
                    message.Writer.WriteByte(data[0]);
                    message.Writer.WriteByte(data[1]);
                });
                message.Writer.WriteBool(magicTree.IsUpdate);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MagicTree0 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        public static Message MagicTree0(MagicTree magicTree)
        {
            if (magicTree == null) return null;
            try
            {
                var dataPosition = Cache.Gi().DATA_MAGICTREE[magicTree.Level];
                var second = (magicTree.Seconds - ServerUtils.CurrentTimeMillis()) / 1000;
                var message = new Message(-34);
                message.Writer.WriteByte(0);
                message.Writer.WriteShort(magicTree.NpcId);
                message.Writer.WriteUTF($"Đậu thần cấp {magicTree.Level}");
                message.Writer.WriteShort(magicTree.X);
                message.Writer.WriteShort(magicTree.Y);
                message.Writer.WriteByte(magicTree.Level);
                message.Writer.WriteShort(magicTree.Peas);
                message.Writer.WriteShort(magicTree.MaxPea);
                message.Writer.WriteUTF(magicTree.IsUpdate ? "Đang trong thời gian nâng cấp" : "Đang kết hạt\nCây lớn sinh nhiều hạt hơn");
                message.Writer.WriteInt((int)second);
                message.Writer.WriteByte(dataPosition.Count);
                dataPosition.ForEach(data =>
                {
                    message.Writer.WriteByte(data[0]);
                    message.Writer.WriteByte(data[1]);
                });
                message.Writer.WriteBool(magicTree.IsUpdate);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MagicTree0 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-34_1
        public static Message MagicTree1(List<string> menus)
        {
            try
            {
                var message = new Message(-34);
                message.Writer.WriteByte(1);
                menus.ForEach(menu => message.Writer.WriteUTF(menu));
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MagicTree1 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-34_2
        public static Message MagicTree2(int total, int level)
        {
            try
            {
                var message = new Message(-34);
                message.Writer.WriteByte(2);
                message.Writer.WriteShort((short)total); ////remain peas
                message.Writer.WriteInt(level * 60); ////seconds
                return message;
            }
            catch (Exception e)
            {   
                Server.Gi().Logger.Error($"Error MagicTree2 in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-32
        public static Message SendBgImg(int zoomLevel, short id)
        {
            try
            {
                var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().BackgroundImg, zoomLevel, id));
                var bytes = ServerUtils.ReadFileBytes(path);
                var message = new Message(-32);
                message.Writer.WriteShort(id);
                message.Writer.WriteInt(bytes.Length);
                message.Writer.Write(bytes);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendBgImg in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-31
        public static Message SendItemBackgrounds()
        {
            Message message;
            try
            {
                message = new Message(-31);
                message.Writer.WriteShort(Cache.Gi().BACKGROUND_ITEM_TEMPLATES.Count);
                Cache.Gi().BACKGROUND_ITEM_TEMPLATES.ForEach(item =>
                {
                    message.Writer.WriteShort(item.BackgroundId);
                    message.Writer.WriteByte(item.Layer);
                    message.Writer.WriteShort(item.X);
                    message.Writer.WriteShort(item.Y);
                    message.Writer.WriteByte(0);
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendItemBackgrounds in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-30_62 _ ME
        public static Message UpdateSkill(short id)
        {
            try
            {
                var message = MessageSubCommand(62);
                message.Writer.WriteShort(id);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateSkill in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-30_35
        public static Message ChangeTypePk(int charId, int type)
        {
            try
            {
                var message = MessageSubCommand(35);
                message.Writer.WriteInt(charId);
                message.Writer.WriteByte(type);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ChangeTypePk in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-30_23 ME
        public static Message AddSkill(short skillId)
        {
            try
            {
                var message = MessageSubCommand(23);
                message.Writer.WriteShort(skillId);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error AddSkill in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-30_15 SendZone
        public static Message LoadLive(ICharacter character)
        {
            if (character == null) return null;
            try
            {
                var message = MessageSubCommand(15);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteInt((int)character.HpFull);
                message.Writer.WriteShort(character.InfoChar.X);
                message.Writer.WriteShort(character.InfoChar.Y);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error LoadLive in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-30_14 SendZone
        public static Message PlayerLoadHp(ICharacter character, int type)
        {
            if (character == null) return null;
            Message message;
            try
            {
                message = MessageSubCommand(14);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteByte(type);
                if(character.InfoChar.Hp >= character.HpFull) {
                    message.Writer.WriteInt((int)character.HpFull);
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerLoadHp in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-30_13 //Send Zone
        public static Message PlayerLoadBody(ICharacter character)
        {
            if (character == null) return null;
            Message message;
            try
            {
                message = MessageSubCommand(13);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteInt((int)character.HpFull);
                message.Writer.WriteShort(0); //getEff5buffHp
                message.Writer.WriteShort(0); //getEff5buffMp
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerLoadBody in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-30_12 Send Zone
        public static Message PlayerLoadQuan(ICharacter character)
        {
            if (character == null) return null;
            Message message;
            try
            {
                message = MessageSubCommand(12);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteInt((int)character.HpFull);
                message.Writer.WriteShort(0); //getEff5buffHp
                message.Writer.WriteShort(0); //getEff5buffMp
                message.Writer.WriteShort(character.GetLeg());
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerLoadQuan in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-30_11 //Send Zone
        public static Message PlayerLoadAo(ICharacter character)
        {
            if (character == null) return null;
            Message message;
            try
            {
                message = MessageSubCommand(11);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteInt((int)character.HpFull);
                message.Writer.WriteShort(0); //getEff5buffHp
                message.Writer.WriteShort(0); //getEff5buffMp
                message.Writer.WriteShort(character.GetBody());
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerLoadAo in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-30_10 _ Zone
        public static Message PlayerLoadVuKhi(ICharacter character)
        {
            if (character == null) return null;
            Message message;
            try
            {
                message = MessageSubCommand(10);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteInt((int)character.HpFull);
                message.Writer.WriteShort(0); //getEff5buffHp
                message.Writer.WriteShort(0); //getEff5buffMp
                message.Writer.WriteShort(1); //Id vũ khí
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerLoadVuKhi in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-30_9 _ Zone
        public static Message PlayerLevel(ICharacter character)
        {
            if (character == null) return null;
            try
            {
                var message = MessageSubCommand(9);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteInt((int)character.HpFull);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerLevel in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-30_8 _ ME
        public static Message PlayerLoadSpeed(ICharacter character)
        {
            if (character == null) return null;
            Message message;
            try
            {
                message = MessageSubCommand(8);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteByte(character.InfoChar.Speed);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerLoadSpeed in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-30_7 _ Zone
        public static Message PlayerLoadAll(ICharacter character, string text = "")
        {
            if (character == null) return null;
            try
            {
                var message = MessageSubCommand(7);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteByte(character.ClanId);
                message.Writer.WriteByte(character.InfoChar.Level);
                message.Writer.WriteBoolean(character.IsInvisible());
                message.Writer.WriteByte(character.InfoChar.TypePk);
                message.Writer.WriteByte(character.InfoChar.NClass);
                message.Writer.WriteByte(character.InfoChar.Gender);
                message.Writer.WriteShort(character.GetHead());
                message.Writer.WriteUTF($"{text}{character.Name}");
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteInt((int)character.HpFull);
                message.Writer.WriteShort(character.GetBody());
                message.Writer.WriteShort(character.GetLeg());
                message.Writer.WriteByte(character.GetBag());
                message.Writer.WriteByte(-1);
                message.Writer.WriteShort(character.InfoChar.X);
                message.Writer.WriteShort(character.InfoChar.Y);
                message.Writer.WriteShort(0); //getEff5buffHp
                message.Writer.WriteShort(0); //getEff5buffMp
                message.Writer.WriteByte(0); // eff
                // for(EffectChar effectChar : character.getvEffectChar()) {
                //     message.Writer.WriteByte(effectChar.getId());
                //     message.Writer.WriteInt(effectChar.getTimeStart());
                //     message.Writer.WriteInt(effectChar.getTimeLength());
                //     message.Writer.WriteShort(effectChar.getParam());
                // }
                message.Writer.WriteShort(character.InfoChar.EffectAuraId);
                message.Writer.WriteByte(-1);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerLoadAll in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-30_6 _ ME
        public static Message SendMp(int mp)
        {
            Message message;
            try
            {
                message = MessageSubCommand(6);
                message.Writer.WriteInt(mp);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendMp in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-30_5 _ ME
        public static Message SendHp(int hp)
        {
            try
            {
                var message = MessageSubCommand(5);
                message.Writer.WriteInt(hp);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendHp in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-30_4  _ ME
        public static Message MeLoadInfo(ICharacter character)
        {
            try
            {
                var message = MessageSubCommand(4);
                var IsNewVersion = character?.Player?.Session?.IsNewVersion;
                if (IsNewVersion == null || IsNewVersion == true)
                {
                    message.Writer.WriteLong(character.InfoChar.Gold);
                }
                else 
                {
                    var gold = character.InfoChar.Gold;
                    if (gold > 2000000000)
                    {
                        gold = 2000000000;
                    }
                    message.Writer.WriteInt((int)gold);
                }
                message.Writer.WriteInt((int)character.InfoChar.Diamond);
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteInt((int)character.InfoChar.Mp);
                message.Writer.WriteInt((int)character.InfoChar.DiamondLock);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MeLoadInfo in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-30_2  _ ME
        public static Message MeLoadSkill(ICharacter character)
        {
            try
            {
                var message = MessageSubCommand(2);
                message.Writer.WriteByte(character.Skills.Count);
                character.Skills.ForEach(skill => message.Writer.WriteShort(skill.SkillId));
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MeLoadInfo in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-30_1  _ ME
        public static Message MeLoadGender(int gender, long tiemNang)
        {
            try
            {
                var message = MessageSubCommand(1);
                message.Writer.WriteByte(gender);
                message.Writer.WriteLong(tiemNang);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MeLoadInfo in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-30_0
        public static Message MeLoadAll(ICharacter character)
        {
            try
            {
                var IsNewVersion = character?.Player?.Session?.IsNewVersion;
                var message = MessageSubCommand(0);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteByte(character.InfoChar.Task.Id);
                message.Writer.WriteByte(character.InfoChar.Gender);
                message.Writer.WriteShort(character.GetHead());
                message.Writer.WriteUTF(character.Name);
                message.Writer.WriteByte(character.InfoChar.Pk);
                message.Writer.WriteByte(character.InfoChar.TypePk);
                message.Writer.WriteLong(character.InfoChar.Power);
                message.Writer.WriteShort(0); //getEff5buffHp
                message.Writer.WriteShort(0); //getEff5buffMp
                message.Writer.WriteByte(character.InfoChar.NClass);
                message.Writer.WriteByte(character.Skills.Count);
                character.Skills.ForEach(skill => message.Writer.WriteShort(skill.SkillId));
                if (IsNewVersion == null || IsNewVersion == true)
                {
                    message.Writer.WriteLong(character.InfoChar.Gold);
                }
                else 
                {
                    var gold = character.InfoChar.Gold;
                    if (gold > 2000000000)
                    {
                        gold = 2000000000;
                    }
                    message.Writer.WriteInt((int)gold);
                }
                message.Writer.WriteInt((int)character.InfoChar.DiamondLock); //lượng khoá
                message.Writer.WriteInt((int)character.InfoChar.Diamond); //lượng
                
                message.Writer.WriteByte(character.BodyLength());
                ItemTemplate itemTemplate;
                character.ItemBody.ForEach(item =>
                {
                    if(item == null) 
                        message.Writer.WriteShort(-1);
                    else
                    {
                        itemTemplate = ItemCache.ItemTemplate(item.Id);
                        message.Writer.WriteShort(itemTemplate.Id);
                        message.Writer.WriteInt(item.Quantity);
                        message.Writer.WriteUTF(itemTemplate.Name);
                        message.Writer.WriteUTF(itemTemplate.Description);
                        message.Writer.WriteByte(item.Options.Count);
                        item.Options.ForEach(op =>
                        {
                            message.Writer.WriteByte(op.Id);
                            message.Writer.WriteShort(op.Param); 
                        });
                    }
                });
                
                character.CharacterHandler.BagSort();
                message.Writer.WriteByte(character.BagLength());
                character.ItemBag.ForEach(item =>
                {
                    itemTemplate = ItemCache.ItemTemplate(item.Id);
                    message.Writer.WriteShort(item.Id);
                    message.Writer.WriteInt(item.Quantity);
                    message.Writer.WriteUTF(itemTemplate.Name);
                    message.Writer.WriteUTF(itemTemplate.Description);
                    message.Writer.WriteByte(item.Options.Count);
                    item.Options.ForEach(op =>
                    {
                        message.Writer.WriteByte(op.Id);
                        message.Writer.WriteShort(op.Param); 
                    });
                });
                for (var i = character.ItemBag.Count; i < character.BagLength(); i++)
                {
                    message.Writer.WriteShort(-1);
                }

                character.CharacterHandler.BoxSort();
                message.Writer.WriteByte(character.BoxLength());
                character.ItemBox.ForEach(item =>
                {
                    itemTemplate = ItemCache.ItemTemplate(item.Id);
                    message.Writer.WriteShort(item.Id);
                    message.Writer.WriteInt(item.Quantity);
                    message.Writer.WriteUTF(itemTemplate.Name);
                    message.Writer.WriteUTF(itemTemplate.Description);
                    message.Writer.WriteByte(item.Options.Count);
                    item.Options.ForEach(op =>
                    {
                        message.Writer.WriteByte(op.Id);
                        message.Writer.WriteShort(op.Param); 
                    });
                });
                for(var i = character.ItemBox.Count; i < character.BoxLength(); i++)
                {
                    message.Writer.WriteShort(-1);
                }

                message.Writer.WriteShort(Cache.Gi().AVATAR.Count);
                foreach (var keyValuePair in Cache.Gi().AVATAR)
                {
                    message.Writer.WriteShort(keyValuePair.Key);
                    message.Writer.WriteShort(keyValuePair.Value);
                }

                message.Writer.WriteShort(DataCache.IdMob[character.InfoChar.Gender][0]);
                message.Writer.WriteShort(DataCache.IdMob[character.InfoChar.Gender][1]);
                message.Writer.WriteShort(DataCache.IdMob[character.InfoChar.Gender][2]);

                message.Writer.WriteByte(0); //Nhap the
                message.Writer.WriteInt(1640895349); //deltaTime
                message.Writer.WriteByte(character.InfoChar.IsNewMember ? 1 : 0); //isNewMember
                message.Writer.WriteShort(character.InfoChar.EffectAuraId);
                message.Writer.WriteByte(-1);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MeLoadAll in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-29
        public static Message ServerList()
        {
            try
            {
                var message = MessageNotLogin(2);
                message.Writer.WriteUTF(DatabaseManager.Manager.gI().Link);
                message.Writer.WriteByte(1);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ServerList in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-28_10
        public static Message RequestMapTemplate(TileMap tileMap, Zone zone, ICharacter character)
        {
            if (tileMap == null || zone == null) return null;
            try
            {
                var message = MessageNotMap(10);
                message.Writer.WriteByte(tileMap.Tmw);
                message.Writer.WriteByte(tileMap.Tmh);
                tileMap.Maps.ToList().ForEach(map =>  message.Writer.WriteByte(map));
    
                // //X, Y Character
                // message.Writer.WriteShort(character.InfoChar.X);
                // message.Writer.WriteShort(character.InfoChar.Y);

                // //Waypoint
                // message.Writer.WriteByte(tileMap.WayPoints.Count);
                // tileMap.WayPoints.ForEach(waypoint =>
                // {
                //     message.Writer.WriteShort(waypoint.MinX);
                //     message.Writer.WriteShort(waypoint.MinY);
                //     message.Writer.WriteShort(waypoint.MaxX);
                //     message.Writer.WriteShort(waypoint.MaxY);
                //     message.Writer.WriteBoolean(waypoint.IsEnter);
                //     message.Writer.WriteBoolean(waypoint.IsOffline);
                //     message.Writer.WriteUTF(waypoint.Name);
                // });
                // //Monster
                // message.Writer.WriteByte(zone.MonsterMaps.Count);
                // zone.MonsterMaps.ForEach(monster =>
                // {
                //     message.Writer.WriteBoolean(monster.IsDisable);
                //     message.Writer.WriteBoolean(monster.IsDontMove);
                //     message.Writer.WriteBoolean(monster.IsFire);
                //     message.Writer.WriteBoolean(monster.IsIce);
                //     message.Writer.WriteBoolean(monster.IsWind);
                //     message.Writer.WriteByte(monster.Id);
                //     message.Writer.WriteByte(monster.Sys);
                //     message.Writer.WriteInt((int)monster.Hp);
                //     message.Writer.WriteByte(monster.Level);
                //     message.Writer.WriteInt((int)monster.HpMax);
                //     message.Writer.WriteShort(monster.X);
                //     message.Writer.WriteShort(monster.Y);
                //     message.Writer.WriteByte(monster.Status);
                //     message.Writer.WriteByte(monster.LvBoss);
                //     message.Writer.WriteBoolean(monster.IsBoss);
                // });

                // message.Writer.WriteByte(0);
                // message.Writer.WriteByte(tileMap.Npcs.Count);
                // tileMap.Npcs.ForEach(npc =>
                // {
                //     message.Writer.WriteByte(npc.Status);
                //     message.Writer.WriteShort(npc.X);
                //     message.Writer.WriteShort(npc.Y);
                //     message.Writer.WriteByte((byte)npc.Id);
                //     message.Writer.WriteShort(npc.Avatar);
                // });

                // message.Writer.WriteByte((byte)zone.ItemMaps.Count);
                // zone.ItemMaps.Values.ToList().ForEach(item =>
                // {
                //     message.Writer.WriteShort(item.Id);
                //     message.Writer.WriteShort(item.Item.Id);
                //     message.Writer.WriteShort(item.X);
                //     message.Writer.WriteShort(item.Y);
                //     message.Writer.WriteInt(item.PlayerId);
                //     if (item.PlayerId == -2)
                //     {
                //         message.Writer.WriteShort(item.R);
                //     }
                // });

                // message.Writer.WriteShort(tileMap.BackgroundItems.Count);
                // tileMap.BackgroundItems.ForEach(bgItem =>
                // {
                //     message.Writer.WriteShort(bgItem.Id);
                //     message.Writer.WriteShort(bgItem.X);
                //     message.Writer.WriteShort(bgItem.Y);
                // });
     
                // message.Writer.WriteShort(tileMap.Actions.Count);
                // tileMap.Actions.ForEach(action =>
                // {
                //     message.Writer.WriteUTF(action.Key);
                //     message.Writer.WriteUTF(action.Value);
                // });
                // message.Writer.WriteByte(tileMap.BgType);
                // message.Writer.WriteByte(character.TypeTeleport);
                message.Writer.WriteByte(tileMap.IsMapDouble ? 1 : 0);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error RequestMapTemplate in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-28_8
        public static Message UpdateItem(int type)
        {
            try
            {
                var message = MessageNotMap(8);
                message.Writer.WriteByte(DatabaseManager.Manager.gI().ItemVersion);
                message.Writer.WriteByte(type);
                switch (type) {
                    case 0: {
                        message.Writer.WriteByte(Cache.Gi().ITEM_OPTION_TEMPLATES.Count);
                        Cache.Gi().ITEM_OPTION_TEMPLATES.ForEach(item =>
                        {
                            message.Writer.WriteUTF(item.Name);
                            message.Writer.WriteByte(item.Type);
                        });
                        break;
                    }
                    case 1:
                    {
                        message.Writer.Write(Cache.Gi().DataItemTemplateOld);
                        break;
                    }
                    case 2: {
                        message.Writer.Write(Cache.Gi().DataItemTemplateNew);
                        break;
                    }
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateItem in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-28_7
        public static Message UpdateSkill()
        {
            try
            {
                
                var message = MessageNotMap(7);
                // if (!DatabaseManager.Manager.gI().IsDir)
                // {
                //     message.Writer.Write(Cache.Gi().NRSKILL);
                // }
                // else
                // {
                // }
                var bytes = ServerUtils.ReadFileBytes(ServerUtils.ProjectDir(DatabaseManager.Manager.gI().NRskill));
                message.Writer.Write(bytes);
                
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateSkill in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-28_6
        public static Message UpdateMap()
        {
            try
            {
                var message = MessageNotMap(6);
                if (!DatabaseManager.Manager.gI().IsDir)
                {
                    message.Writer.Write(Cache.Gi().NRMAP);
                }
                else
                {
                    var bytes = ServerUtils.ReadFileBytes(ServerUtils.ProjectDir(DatabaseManager.Manager.gI().NRmap)); 
                    message.Writer.Write(bytes);
                } 
                
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateMap in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-28_4
        public static Message SendVersionMessage()
        {
            try
            {
                var message = MessageNotMap(4);
                message.Writer.WriteByte(DatabaseManager.Manager.gI().DataVersion);
                message.Writer.WriteByte(DatabaseManager.Manager.gI().MapVersion);
                message.Writer.WriteByte(DatabaseManager.Manager.gI().SkillVersion);
                message.Writer.WriteByte(DatabaseManager.Manager.gI().ItemVersion);
                message.Writer.WriteByte(1);
                message.Writer.WriteByte(Cache.Gi().EXPS.Count);
                Cache.Gi().EXPS.ForEach(exp =>
                {
                    message.Writer.WriteLong(exp);
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendVersionMessage in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-27
        public static Message HansakeMessage(sbyte[] bytes)
        {
            try
            {
                var message = new Message(-27);
                message.Writer.WriteByte(bytes.Length);
                message.Writer.WriteByte(bytes[0]);
                for(var i = 1; i < bytes.Length; i++) {
                    message.Writer.WriteByte(bytes[i] ^ bytes[i - 1]);
                }
                message.Writer.WriteUTF("");
                message.Writer.WriteInt(0);
                message.Writer.WriteByte(0);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error HansakeMessage in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-26
        public static Message DialogMessage(string alert)
        {
            try
            {
                var message = new Message(-26);
                message.Writer.WriteUTF(alert);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error DialogMessage in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-25
        public static Message ServerMessage(string alert)
        {
            try
            {
                var message = new Message(-25);
                message.Writer.WriteUTF(alert);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ServerMessage in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        public static Message SetNight(ICharacter character)
        {
            try
            {
                var message = new Message(-83);
                message.Writer.WriteByte(0); //0 = gọi, 1 = ẩn
                message.Writer.WriteShort(111);
                message.Writer.WriteShort(66);
                message.Writer.WriteByte(character.InfoChar.ZoneId);
                message.Writer.WriteInt(-1);
                message.Writer.WriteUTF("huhuhu");
                message.Writer.WriteShort(character.InfoChar.X);
                message.Writer.WriteShort(character.InfoChar.Y);
                message.Writer.WriteByte(0); //0 thường, 1 namek
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error CallDragon in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
            
        }

        public static Message CallDragon(byte type, byte dragon, ICharacter character)
        {
            try
            {
                var message = new Message(-83);
                message.Writer.WriteByte(type); //0 = gọi, 1 = ẩn
                message.Writer.WriteShort((short)character.InfoChar.MapId);
                message.Writer.WriteShort(66);
                message.Writer.WriteByte(character.InfoChar.ZoneId);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteUTF("huhuhu");
                message.Writer.WriteShort(character.InfoChar.X);
                message.Writer.WriteShort(character.InfoChar.Y);
                message.Writer.WriteByte(dragon); //0 thường, 1 namek
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error CallDragon in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
            
        }
        // -122 DUA HAU
        public static Message TrungMaBu(ICharacter character)
        {
            var charReal = (Character) character;
            var seconds = (character.InfoChar.ThoiGianTrungMaBu - ServerUtils.CurrentTimeMillis()) / 1000;
            if (seconds < 0)
            {
                seconds = 0;
            }
            try
            {
                var message = new Message(-122);
                message.Writer.WriteShort(50); //id quả trứng
                message.Writer.WriteByte(1); // số lượng trứng
                message.Writer.WriteShort(4664); //ảnh của trứng
                message.Writer.WriteByte(0);// index qủa trứng
                message.Writer.WriteInt((int)seconds);//thời gian trứng nở
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error TrungMaBu in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        // me_-117
        public static Message NoTrungMaBu()
        {
            try
            {
                var message = new Message(-117);
                message.Writer.WriteByte(101); // percent
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error TrungMaBu in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //-24
        public static Message MapInfo(Zone zone, ICharacter character)
        {
            var tileMap = zone?.Map?.TileMap;
            if (tileMap == null) return null;
            try
            {
                var message = new Message(-24);
                message.Writer.WriteByte(character.InfoChar.MapId);
                message.Writer.WriteByte(tileMap.PlanetID);
                message.Writer.WriteByte(tileMap.TileID);
                message.Writer.WriteByte(tileMap.BgID);
                message.Writer.WriteByte(tileMap.Type);
                message.Writer.WriteUTF(tileMap.Name);
                message.Writer.WriteByte(character.InfoChar.ZoneId);

                //X, Y Character
                message.Writer.WriteShort(character.InfoChar.X);
                message.Writer.WriteShort(character.InfoChar.Y);

                //Waypoint
                message.Writer.WriteByte(tileMap.WayPoints.Count);
                tileMap.WayPoints.ForEach(waypoint =>
                {
                    message.Writer.WriteShort(waypoint.MinX);
                    message.Writer.WriteShort(waypoint.MinY);
                    message.Writer.WriteShort(waypoint.MaxX);
                    message.Writer.WriteShort(waypoint.MaxY);
                    message.Writer.WriteBoolean(waypoint.IsEnter);
                    message.Writer.WriteBoolean(waypoint.IsOffline);
                    message.Writer.WriteUTF(waypoint.Name);
                });
                //Monster
                message.Writer.WriteByte(zone.MonsterMaps.Count);
                zone.MonsterMaps.ForEach(monster =>
                {
                    message.Writer.WriteBoolean(monster.IsDisable);
                    message.Writer.WriteBoolean(monster.IsDontMove);
                    message.Writer.WriteBoolean(monster.IsFire);
                    message.Writer.WriteBoolean(monster.IsIce);
                    message.Writer.WriteBoolean(monster.IsWind);
                    message.Writer.WriteByte(monster.Id);
                    message.Writer.WriteByte(monster.Sys);
                    message.Writer.WriteInt((int)monster.Hp);
                    message.Writer.WriteByte(monster.Level);
                    message.Writer.WriteInt(monster.MaxExp);
                    message.Writer.WriteShort(monster.X);
                    message.Writer.WriteShort(monster.Y);
                    message.Writer.WriteByte(monster.Status);
                    message.Writer.WriteByte(monster.LvBoss);
                    message.Writer.WriteBoolean(monster.IsBoss);
                });

                message.Writer.WriteByte(0);

                // Xử lý trứng ma bư ở đây
                // Thêm count NPC
                var addCountNpc = 0;
                var hasMaBuEgg = false;
                var charRel = (Character) character;
                
                if (charRel.InfoChar.MapId - 21 == charRel.InfoChar.Gender)
                {
                    if (charRel.InfoChar.ThoiGianTrungMaBu > 0) //Có trứng ma bư
                    {
                        addCountNpc += 1;
                        hasMaBuEgg = true;
                    }
                }

                message.Writer.WriteByte((byte)(tileMap.Npcs.Count+addCountNpc));
                
                // Thêm dưa hấu vào trước
                // Thêm trứng ma bư vào
                if (hasMaBuEgg) 
                {
                    message.Writer.WriteByte((byte)0);
                    message.Writer.WriteShort(charRel.TrungMaBuPosition.X);
                    message.Writer.WriteShort(charRel.TrungMaBuPosition.Y);
                    message.Writer.WriteByte((byte)50);
                    message.Writer.WriteShort(0);
                }
                // Thêm npc trong tileMap
                tileMap.Npcs.ForEach(npc =>
                {
                    message.Writer.WriteByte(npc.Status);
                    message.Writer.WriteShort(npc.X);
                    message.Writer.WriteShort(npc.Y);
                    message.Writer.WriteByte((byte)npc.Id);
                    message.Writer.WriteShort(npc.Avatar);
                });

                message.Writer.WriteByte((byte)zone.ItemMaps.Count);
                zone.ItemMaps.Values.ToList().ForEach(item =>
                {
                    message.Writer.WriteShort(item.Id);
                    message.Writer.WriteShort(item.Item.Id);
                    message.Writer.WriteShort(item.X);
                    message.Writer.WriteShort(item.Y);
                    message.Writer.WriteInt(item.PlayerId);
                    if (item.PlayerId == -2)
                    {
                        message.Writer.WriteShort(item.R);
                    }
                });
                
                message.Writer.WriteShort(tileMap.BackgroundItems.Count);
                tileMap.BackgroundItems.ForEach(bgItem =>
                {
                    message.Writer.WriteShort(bgItem.Id);
                    message.Writer.WriteShort(bgItem.X);
                    message.Writer.WriteShort(bgItem.Y);
                });
     
                message.Writer.WriteShort(tileMap.Actions.Count);
                tileMap.Actions.ForEach(action =>
                {
                    message.Writer.WriteUTF(action.Key);
                    message.Writer.WriteUTF(action.Value);
                });
                message.Writer.WriteByte(tileMap.BgType);
                message.Writer.WriteByte(character.TypeTeleport);
                message.Writer.WriteByte(tileMap.IsMapDouble ? 1 : 0);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MapInfo in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-22
        public static Message MapClear()
        {
            try
            {
                var message = new Message(-22);
                message.Writer.WriteShort(0);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MapClear in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-21 _ Zone
        public static Message ItemMapRemove(int itemMap)
        {
            try
            {
                var message = new Message(-21);
                message.Writer.WriteShort(itemMap); //id
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ItemMapRemove in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-20 _ Me
        public static Message ItemMapMePick(int itemMapId, int quantity, string text)
        {
            try
            {
                var message = new Message(-20);
                message.Writer.WriteShort(itemMapId); //id
                message.Writer.WriteUTF(text); //name
                message.Writer.WriteShort((short)quantity); //quantity
                message.Writer.WriteShort((short)quantity); //quantity
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ItemMapMePick in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-19 _ Zone
        public static Message ItemMapPlayerPick(int itemMap, int characterId)
        {
            try
            {
                var message = new Message(-19);
                message.Writer.WriteShort(itemMap); //id
                message.Writer.WriteInt(characterId); //id
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ItemMapPlayerPick in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-16 _ Me
        public static Message MeLive()
        {
            try
            {
                var message = new Message(-16);
                message.Writer.WriteShort(0); //
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MeDie in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-17 _ Me
        public static Message MeDie(ICharacter character, int minePower)
        {
            try
            {
                var message = new Message(-17);
                message.Writer.WriteByte(character.InfoChar.TypePk); //
                message.Writer.WriteShort(character.InfoChar.X); //
                message.Writer.WriteShort(character.InfoChar.Y); //
                if (minePower > 0)
                {
                    message.Writer.WriteLong(minePower); //
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MeDie in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-13  _ Zone
        public static Message MobLive(IMonster monster)
        {
            try
            {
                var message = new Message(-13);
                message.Writer.WriteByte(monster.IdMap);
                message.Writer.WriteByte(monster.Sys);
                message.Writer.WriteByte(monster.LvBoss);
                message.Writer.WriteInt((int)monster.Hp);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MobLive in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-12  _ Zone
        public static Message MonsterDie(int id)
        {
            try
            {
                var message = new Message(-12);
                message.Writer.WriteByte(id);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MonsterDie in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-11  _ Me
        public static Message MonsterAttackMe(int idMap, int damage, int mp)
        {
            try
            {
                var message = new Message(-11);
                message.Writer.WriteByte(idMap);
                message.Writer.WriteInt(damage);
                message.Writer.WriteInt(mp);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MonsterDie in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-10  _ Zone
        public static Message MonsterAttackPlayer(int idMap, ICharacter character)
        {
            try
            {
                var message = new Message(-10);
                message.Writer.WriteByte(idMap);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteInt((int)character.InfoChar.Mp);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MonsterDie in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-9  _ Zone
        public static Message MonsterHp(IMonster monster, bool isCrit, int damage, int type)
        {
            try
            {
                var message = new Message(-9);
                message.Writer.WriteByte(monster.IdMap);
                message.Writer.WriteInt((int)monster.Hp);
                message.Writer.WriteInt(damage);
                message.Writer.WriteBoolean(isCrit);
                message.Writer.WriteByte(type); //Hiệu ứng đòn đánh
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MonsterDie in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-9_2 _ Zone
        public static Message MonsterHp(IMonster monster)
        {
            try
            {
                var message = new Message(-9);
                message.Writer.WriteByte(monster.IdMap);
                message.Writer.WriteInt((int)monster.Hp);
                message.Writer.WriteInt(1);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MonsterDie in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-8 _ Zone
        public static Message PlayerDie(ICharacter character)
        {
            try
            {
                var message = new Message(-8);
                message.Writer.WriteInt(character.Id); //
                message.Writer.WriteByte(character.InfoChar.TypePk); //
                message.Writer.WriteShort(character.InfoChar.X); //
                message.Writer.WriteShort(character.InfoChar.Y); //
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MeDie in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-7 _ Zone
        public static Message PlayerMove(int id, short x, short y)
        {
            Message message;
            try
            {
                message = new Message(-7);
                message.Writer.WriteInt(id);
                message.Writer.WriteShort(x);
                message.Writer.WriteShort(y);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerMove in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //-6
        public static Message PlayerRemove(int id)
        {
            Message message;
            try
            {
                message = new Message(-6);
                message.Writer.WriteInt(id);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerRemove in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-5 _ Zone
        public static Message PlayerAdd(ICharacter character, string text = "")
        {
            if (character == null) return null;
            try
            {
                var message = new Message(-5);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteInt(character.ClanId);
                message.Writer.WriteByte(character.InfoChar.Level);     //level
                message.Writer.WriteBoolean(character.IsInvisible());   //Tàn hình
                message.Writer.WriteByte(character.InfoChar.TypePk);    //type pk
                message.Writer.WriteByte(character.InfoChar.NClass);    //nClass
                message.Writer.WriteByte(character.InfoChar.Gender);    //gender
                message.Writer.WriteShort(character.GetHead());         //head
                message.Writer.WriteUTF($"{text}{character.Name}");  
                if (character.InfoChar.Hp >= character.HpFull)
                {
                    character.InfoChar.Hp = character.HpFull;
                }
                message.Writer.WriteInt((int)character.InfoChar.Hp);    //hp
                message.Writer.WriteInt((int)character.HpFull);         //hp full
                message.Writer.WriteShort(character.GetBody());         //body
                message.Writer.WriteShort(character.GetLeg());          //leg
                message.Writer.WriteByte(character.GetBag());           //bag
                message.Writer.WriteByte(-1);
                message.Writer.WriteShort(character.InfoChar.X);        //x
                message.Writer.WriteShort(character.InfoChar.Y);        //y
                message.Writer.WriteShort(0);                       //eff 5 buff hp
                message.Writer.WriteShort(0);                       //eff 5 buff mp
                message.Writer.WriteByte(0);                        // eff
                message.Writer.WriteByte(character.TypeTeleport);             //teleport
                message.Writer.WriteByte(character.InfoSkill.Monkey.MonkeyId);    //isMonkey
                message.Writer.WriteShort(character.InfoChar.MountId);  //mount
                message.Writer.WriteByte(character.Flag);               //Flag
                message.Writer.WriteByte(character.InfoChar.Fusion.IsFusion ? 1 : 0);                        //hợp thể
                message.Writer.WriteShort(character.InfoChar.EffectAuraId);
                message.Writer.WriteByte(-1);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerAdd in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //-3 _ ME
        //Type 0: + power
        //Type 1: + tiemNang
        //Type 2: + All
        public static Message UpdateExp(int type, long exp)
        {
            try
            {
                var message = new Message(-3);
                message.Writer.WriteByte(type);
                message.Writer.WriteInt((int)exp);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error UpdateExp in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //2 _ ME
        public static Message LoadingCreateChar()
        {
            try
            {
                var message = new Message(2);
                message.Writer.WriteShort(0);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error LoadingCreateChar in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //6 _ ME
        public static Message BuyItem(ICharacter character)
        {
            try
            {
                var message = new Message(6);
                var IsNewVersion = character?.Player?.Session?.IsNewVersion;
                if (IsNewVersion == true)
                {
                    message.Writer.WriteLong(character.InfoChar.Gold);
                }
                else 
                {
                    var gold = character.InfoChar.Gold;
                    if (gold > 2000000000) gold = 2000000000;
                    message.Writer.WriteInt((int)gold);
                }
                message.Writer.WriteInt((int)character.InfoChar.Diamond);
                message.Writer.WriteInt((int)character.InfoChar.DiamondLock);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error BuyItem in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //7 _ ME
        public static Message SellItem(int type, int index, string info)
        {
            try
            {
                var message = new Message(7);
                message.Writer.WriteByte(type);
                message.Writer.WriteShort(index);
                message.Writer.WriteUTF(info);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SellItem in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //11 _ ME
        public static Message SendMonsterTemplate(int zoomLevel, int id)
        {
            if (zoomLevel is < 1 or > 4) zoomLevel = 2;
            try
            {
                var type = 0;
                if (DataCache.TypeNewBoss_1.Contains(id)) type = 1;
                else if (DataCache.TypeNewBoss_2.Contains(id)) type = 2;

                var bytes = new byte[] { };
                switch (zoomLevel)
                {
                    case 1:
                    {
                        if (Cache.Gi().DATA_MONSTERS_X1.ContainsKey(id))
                        {
                            bytes = Cache.Gi().DATA_MONSTERS_X1[id].ToArray();
                        }
                        else
                        {
                            var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().Monster, zoomLevel, id, type));
                            bytes = ServerUtils.ReadFileBytes(path);
                            Cache.Gi().DATA_MONSTERS_X1.TryAdd(id, bytes.ToList());
                        }
                        break;
                    }
                    case 2:
                    {
                        if (Cache.Gi().DATA_MONSTERS_x2.ContainsKey(id))
                        {
                            bytes = Cache.Gi().DATA_MONSTERS_x2[id].ToArray();
                        }
                        else
                        {
                            var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().Monster, zoomLevel, id, type));
                            bytes = ServerUtils.ReadFileBytes(path);
                            Cache.Gi().DATA_MONSTERS_x2.TryAdd(id, bytes.ToList());
                        }
                        break;
                    }
                    case 3:
                    {
                        if (Cache.Gi().DATA_MONSTERS_x3.ContainsKey(id))
                        {
                            bytes = Cache.Gi().DATA_MONSTERS_x3[id].ToArray();
                        }
                        else
                        {
                            var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().Monster, zoomLevel, id, type));
                            bytes = ServerUtils.ReadFileBytes(path);
                            Cache.Gi().DATA_MONSTERS_x3.TryAdd(id, bytes.ToList());
                        }
                        break;
                    }
                    case 4:
                    {
                        if (Cache.Gi().DATA_MONSTERS_x4.ContainsKey(id))
                        {
                            bytes = Cache.Gi().DATA_MONSTERS_x4[id].ToArray();
                        }
                        else
                        {
                            var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().Monster, zoomLevel, id, type));
                            bytes = ServerUtils.ReadFileBytes(path);
                            Cache.Gi().DATA_MONSTERS_x4.TryAdd(id, bytes.ToList());
                        }
                        break;
                    }
                }

                if (bytes.Length <= 0) return null;
                var message = new Message(11);
                message.Writer.WriteByte(id);
                message.Writer.WriteByte(type);
                message.Writer.Write(bytes);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendMonsterTemplate in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //15 _ Zone
        public static Message PlayerLoadLive(ICharacter character)
        {
            try
            {
                var message = new Message(15);
                message.Writer.WriteInt(character.Id);
                if (character.InfoChar.Hp >= character.HpFull)
                {
                    character.InfoChar.Hp = character.HpFull;
                }
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteInt((int)character.HpFull);
                message.Writer.WriteShort(character.InfoChar.X);
                message.Writer.WriteShort(character.InfoChar.Y);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PlayerLoadLive in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //29 _ ME
        public static Message OpenUiZone(Threading.Map map)
        {
            try
            {
                var message = new Message(29);
                message.Writer.WriteByte(map.Zones.Count);
                map.Zones.ForEach(zone =>
                {
                    var numCharInZone = zone.Characters.Count;
                    var numCharMax = map.TileMap.MaxPlayers;
                    var type = 0;
                    if (numCharInZone >= numCharMax / 2 &&
                        numCharInZone < numCharMax * 3 / 4)
                    {
                        type = 1;
                    }
                    else if(numCharInZone >= numCharMax * 3 / 4)
                    {
                        type = 2;
                    }
                    message.Writer.WriteByte(zone.Id);
                    message.Writer.WriteByte(type);
                    message.Writer.WriteByte(numCharInZone);
                    message.Writer.WriteByte(numCharMax);
                    message.Writer.WriteByte(0); // rank
                });
                
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error OpenUiZone in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //32 _ ME
        public static Message OpenUiConfirm(short npcId, string text, List<string> menus, int gender)
        {
            try
            {
                var message = new Message(32);
                message.Writer.WriteShort(npcId);
                message.Writer.WriteUTF(text);
                message.Writer.WriteByte(menus.Count);
                menus.ForEach(menu => message.Writer.WriteUTF(menu));
                if (gender > 2)
                {
                    message.Writer.WriteShort(gender);
                }
                else
                {
                    message.Writer.WriteShort(DataCache.IdMob[gender][2]);
                }
                
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error OpenUiConfirm in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //38
        public static Message OpenUiSay(short npcId, string text, bool isNpc = true, int gender = 0)
        {
            try
            {
                var message = new Message(38);
                message.Writer.WriteShort(npcId);
                message.Writer.WriteUTF(text);
                if (!isNpc)
                {
                    if (gender > 2)
                    {
                        message.Writer.WriteShort(gender);
                    }
                    else
                    {
                        message.Writer.WriteShort(DataCache.IdMob[gender][2]);
                    }
                    
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error OpenUiSay in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //40
        public static Message SendTask(ICharacter character)
        {
            try
            {
                var task = Cache.Gi().TASK_TEMPLATES.Values
                    .FirstOrDefault(t => t.Id == character.InfoChar.Task.Id);
                if (task == null) return null;
                var message = new Message(40);
                message.Writer.WriteShort(character.InfoChar.Task.Id);
                message.Writer.WriteByte(character.InfoChar.Task.Index);

                message.Writer.WriteUTF(task.Name);
                message.Writer.WriteUTF(task.Detail);

                message.Writer.WriteByte(task.SubNames.Count);
                var indexTask = 0;
                task.SubNames.ForEach(subname =>
                {
                    var a = Cache.Gi().TASKS.FirstOrDefault(x => x.Key == character.InfoChar.Task.Id).Value[indexTask];
                    if(a is 0 or 1 or 2)
                    {
                        a = character.InfoChar.Gender switch
                        {
                            0 => 0,
                            1 => 2,
                            2 => 1,
                            _ => a
                        };
                    }
                    message.Writer.WriteUTF(subname);
                    message.Writer.WriteByte(a);
                    message.Writer.WriteShort(Cache.Gi().MAPTASKS.FirstOrDefault(x => x.Key == character.InfoChar.Task.Id).Value[indexTask]);
                    message.Writer.WriteUTF(task.ContentInfo[indexTask]);
                    indexTask++;
                });
                message.Writer.WriteShort(character.InfoChar.Task.Count);
                task.Counts.ForEach(count => message.Writer.WriteShort(count));
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendTask in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //44
        public static Message PublicChat(int charId, string text)
        {
            try
            {
                var message = new Message(44);
                message.Writer.WriteInt(charId);
                message.Writer.WriteUTF(text);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error PublicChat in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //50
        public static Message GameInfo()
        {
            try
            {
                var message = new Message(50);
                message.Writer.WriteByte(Cache.Gi().GAME_INFO_TEMPLATES.Count-1);
                Cache.Gi().GAME_INFO_TEMPLATES.Where(info => info.Id != 0).ToList().ForEach(info =>
                {
                    message.Writer.WriteShort(info.Id);
                    message.Writer.WriteUTF(info.Main);
                    message.Writer.WriteUTF(info.Content);
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error GameInfo in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //54 _ Zone
        public static Message PlayerAttackMonster(int charId, List<IMonster> monsters, int skillId)
        {
            try
            {
                var message = new Message(54);
                message.Writer.WriteInt(charId);
                message.Writer.WriteByte(skillId);
                monsters.ForEach(m => message.Writer.WriteByte(m.IdMap));
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error GameInfo in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //56 _ Zone
        public static Message HaveAttackPlayer(ICharacter character, bool isFatal, long damage, int type)
        {
            try
            {
                var message = new Message(56);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteInt((int)character.InfoChar.Hp);
                message.Writer.WriteInt((int)damage);
                message.Writer.WriteBool(isFatal);
                message.Writer.WriteByte(type);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error GameInfo in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //66
        public static Message SendImgByName(int zoomLevel, string name)
        {
            if (zoomLevel is < 1 or > 4) zoomLevel = 2;
            try
            {
                var nFrame = 3;
                if(name.Contains("4_1")) nFrame = 2;
                else if(name.Contains("7_1") || name.Contains("9_0") || name.Contains("3_1") || name.Contains("aura_0_0") || name.Contains("aura_0_1")) nFrame = 4;
                else if(name.Contains("aura_2_0")) nFrame = 5;
                var bytes = ServerUtils.ReadFileBytes(ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().Mount, zoomLevel, name)));
                if (bytes == null) {
                    Server.Gi().Logger.Error($"Error SendImgByName by Name: {name}");
                    return null;
                }
                var message = new Message(66);
                message.Writer.WriteUTF(name);
                message.Writer.WriteByte(nFrame);
                message.Writer.WriteInt(bytes.Length);
                message.Writer.Write(bytes);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SendImgByName in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //68
        public static Message ItemMapAdd(ItemMap itemMap)
        {
            try
            {
                var itemTempalte = ItemCache.ItemTemplate(itemMap.Item.Id);
                var message = new Message(68);
                message.Writer.WriteShort(itemMap.Id); //id
                message.Writer.WriteShort(itemTempalte.Id); //template
                message.Writer.WriteShort(itemMap.X); //x
                message.Writer.WriteShort(itemMap.Y); //y
                message.Writer.WriteInt(itemMap.PlayerId); //player id
                if(itemMap.PlayerId == -2) {
                    message.Writer.WriteShort(itemMap.R); //r
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ItemMapAdd in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //74
        public static Message MonsterFlyLeaveItem(int mobMapId, ItemMap itemMap)
        {
            try
            {
                var message = new Message(74);
                message.Writer.WriteByte(mobMapId); //id
                message.Writer.WriteShort(itemMap.Id); //
                message.Writer.WriteShort(itemMap.Item.Id); //x
                message.Writer.WriteShort(itemMap.X); //
                message.Writer.WriteShort(itemMap.Y); //
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MonsterFlyLeaveItem in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //82 _ Zone
        public static Message MonsterDontMove(int monsterId, bool isDontMove)
        {
            try
            {
                var message = new Message(82);
                message.Writer.WriteByte(monsterId); //id
                message.Writer.WriteBool(isDontMove); //
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MonsterFlyLeaveItem in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //84 _ Zone
        public static Message ReturnPointMap(ICharacter character)
        {
            try
            {
                var message = new Message(84);
                message.Writer.WriteInt(character.Id); //id
                message.Writer.WriteShort(character.InfoChar.X); //
                message.Writer.WriteShort(character.InfoChar.Y); //
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error MonsterFlyLeaveItem in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //92
        public static Message WorldChat(Character character, string text, int type)
        {
            try
            {
                var name = character != null ? character.Name : "";
                var message = new Message(92);
                message.Writer.WriteUTF(name);
                message.Writer.WriteUTF("|5|"+text);
                if (character == null) return message;
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteShort(character.GetHead());
                message.Writer.WriteShort(character.GetBody());
                message.Writer.WriteShort(character.GetBag());
                message.Writer.WriteShort(character.GetLeg());
                message.Writer.WriteByte(type);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error WorldChat in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //93
        public static Message ServerChat(string text)
        {
            try
            {
                var message = new Message(93);
                message.Writer.WriteUTF(text);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error ServerChat in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //112
        public static Message SpeacialSkill(ICharacter character, int type)
        {
            try
            {
                var charRel = (Character) character;
                var message = new Message(112);
                message.Writer.WriteByte(type);
                if(type == 0) {
                    message.Writer.WriteShort(charRel.SpecialSkill.Img);
                    message.Writer.WriteUTF(charRel.SpecialSkill.Info);
                } else {
                    var specialSkillTemplate = Cache.Gi().SPECIAL_SKILL_TEMPLATES.FirstOrDefault(s => s.Key == charRel.InfoChar.Gender).Value;
                    if (specialSkillTemplate == null) return null;
                    message.Writer.WriteByte(1); //Số lượng tab
                    message.Writer.WriteUTF("Nội tại");
                    message.Writer.WriteByte(specialSkillTemplate.Count);// Số lượng skill
                    specialSkillTemplate.ForEach(skill =>
                        {
                            message.Writer.WriteShort(skill.Img);
                            message.Writer.WriteUTF(skill.Info);
                        }
                    );
                }
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SpeacialSkill in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //123
        public static Message SendPos(ICharacter character, int type)
        {
            try
            {
                var message = new Message(123);
                message.Writer.WriteInt(character.Id);
                message.Writer.WriteShort(character.InfoChar.X);
                message.Writer.WriteShort(character.InfoChar.Y);
                message.Writer.WriteByte(type);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error NpcChat in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        
        //124 _ Me
        public static Message NpcChat(short npcId, string text)
        {
            try
            {
                var message = new Message(124);
                message.Writer.WriteShort(npcId);
                message.Writer.WriteUTF(text);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error NpcChat in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //125 _ Zone
        public static Message Fusion(int id, int type)
        {
            try
            {
                var message = new Message(125);
                message.Writer.WriteByte(type);
                message.Writer.WriteInt(id);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error NpcChat in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }

        //127 _ ME
        public static Message Radar0(List<Card> cards)
        {
            try
            {
                var message = new Message(127);
                message.Writer.WriteByte(0);
                message.Writer.WriteShort(Cache.Gi().RADAR_TEMPLATE.Count);
                Cache.Gi().RADAR_TEMPLATE.ForEach(radar =>
                {
                    var card = cards.FirstOrDefault(c => c.Id == radar.Id);
                    if (card == null)
                    {
                        card = new Card()
                        {
                            MaxAmount = radar.Max,
                            Options = radar.Options
                        };
                    }
                    message.Writer.WriteShort(radar.Id);
                    message.Writer.WriteShort(radar.IconId);
                    message.Writer.WriteByte(radar.Rank);
                    message.Writer.WriteByte(card.Amount);  //amount
                    message.Writer.WriteByte(card.MaxAmount);  //max_amount
                    message.Writer.WriteByte(radar.Type);  //type 0: monster, 1: charpart
                    switch (radar.Type)
                    {
                        case 0:
                            message.Writer.WriteShort(radar.Template); //Monster
                            break;
                        case 1:
                            message.Writer.WriteShort(radar.Data[0]); //Head
                            message.Writer.WriteShort(radar.Data[1]); //Body
                            message.Writer.WriteShort(radar.Data[2]); //Leg
                            message.Writer.WriteShort(radar.Data[3]); //bag
                            break;
                    }
                    message.Writer.WriteUTF(radar.Name);  //name
                    message.Writer.WriteUTF(radar.Info);  //info
                    message.Writer.WriteByte(card.Level);  //LEvel
                    message.Writer.WriteByte(card.Used);  //use
                    message.Writer.WriteByte(radar.Options.Count);  //option radar
                    card.Options.ForEach(option =>
                    {
                        message.Writer.WriteByte(option.Id);  //id
                        message.Writer.WriteShort(option.Param);  //param
                        message.Writer.WriteByte(option.ActiveCard);  //ActiveCard
                    });
                });
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error NpcChat in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //127_1 _ ME
        public static Message Radar1(int id, int use)
        {
            try
            {
                var message = new Message(127);
                message.Writer.WriteByte(1);
                message.Writer.WriteShort(id);
                message.Writer.WriteByte(use);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error NpcChat in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //127_2 _ ME _ set level
        public static Message Radar2(int id, int level)
        {
            try
            {
                var message = new Message(127);
                message.Writer.WriteByte(2);
                message.Writer.WriteShort(id);
                message.Writer.WriteByte(level);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error NpcChat in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
        //127_3 _ ME _ set amount
        public static Message Radar3(int id, int amount, int max_amount)
        {
            try
            {
                var message = new Message(127);
                message.Writer.WriteByte(3);
                message.Writer.WriteShort(id);
                message.Writer.WriteByte(amount);
                message.Writer.WriteByte(max_amount);
                return message;
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error NpcChat in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return null;
            }
        }
    }
}
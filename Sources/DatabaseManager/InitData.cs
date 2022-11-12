using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Newtonsoft.Json;
using NRO_Server.Application.Constants;
using NRO_Server.Application.IO;
using NRO_Server.Application.Manager;
using NRO_Server.Application.Threading;
using NRO_Server.Model;
using NRO_Server.Model.Character;
using NRO_Server.Model.Clan;
using NRO_Server.Model.Data;
using NRO_Server.Model.Info;
using NRO_Server.Model.Item;
using NRO_Server.Model.Map;
using NRO_Server.Model.Monster;
using NRO_Server.Model.Option;
using NRO_Server.Model.Template;
using NRO_Server.Model.SkillCharacter;

namespace NRO_Server.DatabaseManager
{
    public class InitData
    {
        public InitData()
        {
            ClearCache();
            InitDBData();
            InitCache();
            InitMapServer();
            InitDBAccount();
        }

        private void InitDBData()
        {
            DbContext.gI()?.ConnectToData();
            using (DbCommand command = DbContext.gI()?.Connection.CreateCommand())
            {
                if (command != null)
                {
                    
                    Server.Gi().Logger.Print("Loading Database...");
                    #region Read Table Task
                    command.CommandText = "SELECT * FROM `tasks`";
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            if (!Cache.Gi().TASKS.ContainsKey(id))
                            {
                                Cache.Gi().TASKS.Add(id, JsonConvert.DeserializeObject<int[]>(reader.GetString(1)));
                                Cache.Gi().MAPTASKS.Add(id, JsonConvert.DeserializeObject<int[]>(reader.GetString(2)));
                            }
                        }
                    }

                    reader.Close();

                    #endregion

                    #region Read Table Task Template

                    command.CommandText = "SELECT * FROM `tasktemplate`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            short id = reader.GetInt16(0);
                            if (!Cache.Gi().TASK_TEMPLATES.ContainsKey(id))
                            {
                                Cache.Gi().TASK_TEMPLATES.Add(id, new TaskTemplate()
                                {
                                    Id = id,
                                    Name = reader.GetString(1),
                                    Detail = reader.GetString(2),
                                    SubNames = JsonConvert.DeserializeObject<List<string>>(reader.GetString(3)),
                                    Counts = JsonConvert.DeserializeObject<List<short>>(reader.GetString(4)),
                                    ContentInfo = JsonConvert.DeserializeObject<List<string>>(reader.GetString(5)),
                                });
                            }
                        }
                    }

                    reader.Close();

                    #endregion

                    #region Read Table nr_arrow

                    command.CommandText = "SELECT * FROM `nr_arrow`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().NRArrows.Add(new Arrow()
                            {
                                Id = reader.GetInt16(0),
                                Data = JsonConvert.DeserializeObject<List<short>>(reader.GetString(1))
                            });
                        }

                    reader.Close();

                    #endregion

                    #region Read Table nr_dart

                    command.CommandText = "SELECT * FROM `nr_dart`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().NRDarts.AddRange(JsonConvert.DeserializeObject<List<Dart>>(reader.GetString(1)));
                            break;
                        }

                    reader.Close();

                    #endregion

                    #region Read Table nr_effect

                    command.CommandText = "SELECT * FROM `nr_effect`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().NREffects.Add(new Effect()
                            {
                                Id = reader.GetInt16(0),
                                Data = JsonConvert.DeserializeObject<short[][]>(reader.GetString(1))
                            });
                        }

                    reader.Close();

                    #endregion

                    #region Read Table nr_image

                    command.CommandText = "SELECT * FROM `nr_image`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().NRImages.Add(new Image()
                            {
                                Id = reader.GetInt16(0),
                                Data = JsonConvert.DeserializeObject<short[]>(reader.GetString(1))
                            });
                        }

                    reader.Close();

                    #endregion

                    #region Read Table nr_part

                    command.CommandText = "SELECT * FROM `nr_part`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().NRParts.Add(new Part()
                            {
                                Id = reader.GetInt16(0),
                                Type = reader.GetByte(1),
                                Data = JsonConvert.DeserializeObject<short[][]>(reader.GetString(2))
                            });
                        }

                    reader.Close();

                    #endregion

                    #region Read Table nr_skill

                    command.CommandText = "SELECT * FROM `nr_skill`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().NRSkills.Add(new Skill()
                            {
                                Id = reader.GetInt16(0),
                                EffectHappenOnMob = reader.GetInt16(1),
                                NumEff = Byte.Parse(reader.GetString(2)),
                                SkillStand = JsonConvert.DeserializeObject<short[][]>(reader.GetString(3)),
                                SkillFly = JsonConvert.DeserializeObject<short[][]>(reader.GetString(4))
                            });
                        }

                    reader.Close();

                    #endregion

                    #region Read Table exps

                    command.CommandText = "SELECT * FROM `exps`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().EXPS.Add(reader.GetInt64(1));
                        }

                    reader.Close();

                    #endregion

                    #region Read Table limitPower

                    command.CommandText = "SELECT * FROM `limitPower`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            var id = reader.GetInt32(0);
                            if (!Cache.Gi().LIMIT_POWERS.ContainsKey(id))
                            {
                                Cache.Gi().LIMIT_POWERS.TryAdd(id,
                                    JsonConvert.DeserializeObject<List<LimitPower>>(reader.GetString(1))!.ElementAt(0));
                            }
                        }

                    reader.Close();

                    #endregion

                    #region Read Table level

                    command.CommandText = "SELECT * FROM `level`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Level level = new Level()
                            {
                                Id = reader.GetInt32(0),
                                Gender = Byte.Parse(reader.GetString(1)),
                                Name = reader.GetString(2),
                            };
                            Cache.Gi().LEVELS.Add(level);
                        }

                    reader.Close();

                    #endregion

                    #region Read Table datahead

                    command.CommandText = "SELECT * FROM `datahead`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().AVATAR.TryAdd(reader.GetInt32(0), reader.GetInt32(1));
                        }

                    reader.Close();

                    #endregion

                    #region Read Table itembackground

                    command.CommandText = "SELECT * FROM `itembackground`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().BACKGROUND_ITEM_TEMPLATES.Add(new BackgroundItemTemplate()
                            {
                                Id = reader.GetInt32(0),
                                BackgroundId = reader.GetInt32(1),
                                Layer = reader.GetInt32(2),
                                X = reader.GetInt32(3),
                                Y = reader.GetInt32(4),
                            });
                        }

                    reader.Close();

                    #endregion

                    #region Read Table monster

                    command.CommandText = "SELECT * FROM `monster`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            MonsterTemplate monsterTemplate = new MonsterTemplate()
                            {
                                Id = reader.GetInt32(0),
                                Type = Byte.Parse(reader.GetString(1)),
                                Name = reader.GetString(2),
                                Hp = reader.GetInt32(3),
                                RangeMove = Byte.Parse(reader.GetString(4)),
                                Speed = Byte.Parse(reader.GetString(5)),
                                DartType = Byte.Parse(reader.GetString(6)),
                                LeaveItemType = reader.GetInt32(7)
                            };

                            Cache.Gi().MONSTER_TEMPLATES.Add(monsterTemplate);
                        }

                    reader.Close();

                    #endregion

                    #region Read Table Npc

                    command.CommandText = "SELECT * FROM `npc`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().NPC_TEMPLATES.Add(new NpcTemplate()
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Head = reader.GetInt16(2),
                                Body = reader.GetInt16(3),
                                Leg = reader.GetInt16(4),
                                Menu = JsonConvert.DeserializeObject<string[][]>(reader.GetString(5))
                            });
                        }

                    reader.Close();

                    #endregion

                    #region Read Table boss

                    command.CommandText = "SELECT * FROM `boss`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            BossTemplate bossTemplate = new BossTemplate()
                            {
                                Id = reader.GetInt32(0),
                                Type = reader.GetInt32(1),
                                Name = reader.GetString(2),
                                Hp = reader.GetInt32(3),
                                Mp = reader.GetInt32(4),
                                Stamina = (short)reader.GetInt32(5),
                                Skills = JsonConvert.DeserializeObject<List<SkillCharacter>>(reader.GetString(6),
                                    DataCache.SettingNull),
                                Damage = reader.GetInt32(7),
                                Defence = reader.GetInt32(8),
                                CritChance = reader.GetInt32(9),
                                Hair = (short)reader.GetInt32(10),
                                KhangTroi = reader.GetBoolean(11),
                            };

                            Cache.Gi().BOSS_TEMPLATES.Add(bossTemplate);
                        }

                    reader.Close();

                    #endregion

                    #region Read Table Tile

                    command.CommandText = "SELECT * FROM `tile`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().TILE_TYPES = JsonConvert.DeserializeObject<int[][]>(reader.GetString(1));
                            Cache.Gi().TILE_INDEXS = JsonConvert.DeserializeObject<int[][][]>(reader.GetString(2));
                            break;
                        }

                    reader.Close();

                    #endregion

                    #region Read Table optionitem

                    command.CommandText = "SELECT * FROM `optionitem`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().ITEM_OPTION_TEMPLATES.Add(new ItemOptionTemplate()
                            {
                                Id = reader.GetInt32(0),
                                Type = Byte.Parse(reader.GetString(1)),
                                Name = reader.GetString(2)
                            });
                        }

                    reader.Close(); ;

                    #endregion

                    #region Read Table item

                    command.CommandText = "SELECT * FROM `item`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            short id = reader.GetInt16(0);
                            List<OptionItem> list =
                                JsonConvert.DeserializeObject<List<OptionItem>>(reader.GetString(16));
                            if (list == null)
                            {
                                list = new List<OptionItem>()
                                {
                                    new OptionItem()
                                    {
                                        Id = 73,
                                        Param = 0
                                    }
                                };
                            }
                            else if (list.Count <= 0)
                                list.Add(new OptionItem()
                                {
                                    Id = 73,
                                    Param = 0
                                });

                            ItemTemplate itemTemplate = new ItemTemplate()
                            {
                                Id = id,
                                Type = Byte.Parse(reader.GetString(1)),
                                Skill = Byte.Parse(reader.GetString(2)),
                                Gender = Byte.Parse(reader.GetString(3)),
                                Name = reader.GetString(4),
                                SubName = reader.GetString(5),
                                Description = reader.GetString(6),
                                Level = Byte.Parse(reader.GetString(7)),
                                IconId = reader.GetInt16(8),
                                Part = reader.GetInt16(9),
                                IsUpToUp = reader.GetBoolean(10),
                                IsDrop = reader.GetBoolean(11),
                                Require = reader.GetInt64(12),
                                IsExpires = reader.GetBoolean(13),
                                SecondsExpires = reader.GetInt64(14),
                                SaleCoin = reader.GetInt32(15)
                            };
                            itemTemplate.Options.AddRange(list);
                            Cache.Gi().ITEM_TEMPLATES.TryAdd(id, itemTemplate);
                        }

                    reader.Close();

                    #endregion

                    #region Read Table Skill

                    command.CommandText = "SELECT * FROM `skilltemplate`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            SkillTemplate skillTemplate = new SkillTemplate()
                            {
                                Id = reader.GetInt32(0),
                                NClass = Byte.Parse(reader.GetString(1)),
                                Name = reader.GetString(2),
                                MaxPoint = Byte.Parse(reader.GetString(3)),
                                ManaUseType = Byte.Parse(reader.GetString(4)),
                                Type = Byte.Parse(reader.GetString(5)),
                                IconId = reader.GetInt16(6),
                                DamageInfo = reader.GetString(7),
                                Description = reader.GetString(8),
                                SkillDataTemplates =
                                    JsonConvert.DeserializeObject<List<SkillDataTemplate>>(reader.GetString(9))
                            };
                            Cache.Gi().SKILL_TEMPLATES.Add(skillTemplate);
                        }

                    reader.Close();

                    #endregion

                    #region Read Table Skill Option

                    command.CommandText = "SELECT * FROM `optionskill`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().SKILL_OPTIONS.Add(new SkillOption()
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                            });
                        }

                    reader.Close();

                    #endregion

                    #region Read Table map

                    command.CommandText = "SELECT * FROM `map`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            var tileMap = new TileMap()
                            {
                                Id = reader.GetInt32(0),
                                Type = reader.GetInt32(1),
                                PlanetID = Byte.Parse(reader.GetString(2)),
                                TileID = reader.GetInt32(3),
                                BgID = reader.GetInt32(4),
                                BgType = reader.GetInt32(5),
                                IsMapDouble = reader.GetBoolean(6),
                                Name = reader.GetString(7),
                                Teleport = reader.GetInt32(8),
                                MaxPlayers = reader.GetInt32(9),
                                ZoneNumbers = reader.GetInt32(10),
                                X0 = reader.GetInt16(11),
                                Y0 = reader.GetInt16(12)
                            };
                            var objects = JsonConvert.DeserializeObject<object[][]>(reader.GetString(13));
                            if (objects != null)
                                foreach (var wp in objects)
                                {
                                    tileMap.WayPoints.Add(new WayPoint()
                                    {
                                        MinX = short.Parse(wp[0].ToString() ?? "0"),
                                        MinY = short.Parse(wp[1].ToString() ?? "0"),
                                        MaxX = short.Parse(wp[2].ToString() ?? "0"),
                                        MaxY = short.Parse(wp[3].ToString() ?? "0"),
                                        IsEnter = byte.Parse(wp[4].ToString() ?? "0") != 0,
                                        IsOffline = byte.Parse(wp[5].ToString() ?? "0") != 0,
                                        Name = wp[6].ToString(),
                                        MapNextId = byte.Parse(wp[7].ToString() ?? "0"),
                                    });
                                }

                            objects = JsonConvert.DeserializeObject<object[][]>(reader.GetString(14));
                            if (objects != null)
                                foreach (var monster in objects)
                                {
                                    tileMap.MonsterMaps.Add(new MonsterMap()
                                    {
                                        Id = short.Parse(monster[0].ToString() ?? "0"),
                                        Level = int.Parse(monster[1].ToString() ?? "0"),
                                        X = short.Parse(monster[2].ToString() ?? "0"),
                                        Y = short.Parse(monster[3].ToString() ?? "0"),
                                        Status = 5,
                                        LvBoss = byte.Parse(monster[5].ToString() ?? "0"),
                                        IsBoss = byte.Parse(monster[6].ToString() ?? "0") != 0,
                                    });
                                }

                            objects = JsonConvert.DeserializeObject<object[][]>(reader.GetString(15));
                            if (objects != null)
                                foreach (var npc in objects)
                                {
                                    tileMap.Npcs.Add(new Npc()
                                    {
                                        Status = byte.Parse(npc[0].ToString() ?? "0"),
                                        X = short.Parse(npc[1].ToString() ?? "0"),
                                        Y = short.Parse(npc[2].ToString() ?? "0"),
                                        Id = short.Parse(npc[3].ToString() ?? "0"),
                                        Avatar = short.Parse(npc[4].ToString() ?? "0")
                                    });
                                }

                            objects = JsonConvert.DeserializeObject<object[][]>(reader.GetString(16));
                            if (objects != null)
                                foreach (var background in objects)
                                {
                                    tileMap.BackgroundItems.Add(new BackgroundItem()
                                    {
                                        Id = short.Parse(background[0].ToString() ?? "0"),
                                        X = short.Parse(background[1].ToString() ?? "0"),
                                        Y = short.Parse(background[2].ToString() ?? "0"),
                                    });
                                }

                            objects = JsonConvert.DeserializeObject<object[][]>(reader.GetString(17));
                            if (objects != null)
                                foreach (var action in objects)
                                {
                                    tileMap.Actions.Add(new ActionItem()
                                    {
                                        Key = action[0].ToString(),
                                        Value = action[1].ToString(),
                                    });
                                }

                            Cache.Gi().TILE_MAPS.Add(tileMap);
                        }

                    reader.Close();
                    #endregion
                    
                    #region Read Table Store
                    command.CommandText = "SELECT * FROM `shop`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            var shopId = reader.GetInt16(3);
                            var shopTemplate = new ShopTemplate()
                            {
                                Id = reader.GetInt32(0),
                                Type = byte.Parse(reader.GetString(4)),
                                Name = reader.GetString(5),
                                ItemShops = JsonConvert.DeserializeObject<List<ItemShop>>(reader.GetString(6),
                                    DataCache.SettingNull)
                            };
                            if (!Cache.Gi().SHOP_TEMPLATES.ContainsKey(shopId))
                            {
                                Cache.Gi().SHOP_TEMPLATES.Add(shopId, new List<ShopTemplate>());                          
                            }

                            Cache.Gi().SHOP_TEMPLATES[shopId].Add(shopTemplate);
                        }
                    reader.Close();
                    #endregion

                    #region Read Data RADAR
                    command.CommandText = "SELECT * FROM `radar`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            var template = new RadarTemplate()
                            {
                                Id = reader.GetInt16(0),
                                IconId = reader.GetInt16(1),
                                Rank = reader.GetInt32(2),
                                Max = reader.GetInt32(3),
                                Type = reader.GetInt32(4),
                                Template = reader.GetInt32(5),
                                Data = JsonConvert.DeserializeObject<List<short>>(reader.GetString(6)),
                                Name = reader.GetString(7),
                                Info = reader.GetString(8),
                                Options = JsonConvert.DeserializeObject<List<OptionRadar>>(reader.GetString(9)),
                                Require = reader.GetInt16(10),
                                RequireLevel = reader.GetInt16(11),
                                AuraId = reader.GetInt16(12), 
                            };
                            if (Cache.Gi().RADAR_TEMPLATE.FirstOrDefault(r => r.Id == template.Id) == null)
                            {
                                Cache.Gi().RADAR_TEMPLATE.Add(template);
                            }
                        }
                    reader.Close();
                    #endregion

                    #region Read Data MagicTree
                    command.CommandText = "SELECT * FROM `datamagictree`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            Cache.Gi().DATA_MAGICTREE.TryAdd(reader.GetInt16(0), JsonConvert.DeserializeObject<List<List<int>>>(reader.GetString(1)));
                        }
                    reader.Close();
                    #endregion

                    #region Read Table Clan Image

                    command.CommandText = "SELECT * FROM `clanimage`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            var clanImage = new ClanImage()
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Gold = reader.GetInt32(2),
                                Diamond = reader.GetInt32(3),
                                Data = JsonConvert.DeserializeObject<List<short>>(reader.GetString(4)),
                            };
                            if (!Cache.Gi().CLAN_IMAGES.Contains(clanImage))
                            {
                                Cache.Gi().CLAN_IMAGES.Add(clanImage);
                            }
                        }

                    reader.Close();

                    #endregion

                    #region Read Table Special Skill
                    command.CommandText = "SELECT * FROM `special_skill`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            var specialSkillId = reader.GetInt32(1);
                            var skillTemplate = new SpecialSkillTemplate()
                            {
                                Id  = reader.GetInt32(0),
                                Gender  = (short)specialSkillId,
                                Info  = reader.GetString(2),
                                InfoFormat  = reader.GetString(3),
                                Img  = (short)reader.GetInt32(4),
                                SkillId  = (short)reader.GetInt32(5),
                                Min  = (short)reader.GetInt32(6),
                                Max  = (short)reader.GetInt32(7),
                                Vip  = reader.GetInt32(8),
                            };

                            if (!Cache.Gi().SPECIAL_SKILL_TEMPLATES.ContainsKey(specialSkillId))
                            {
                                Cache.Gi().SPECIAL_SKILL_TEMPLATES.Add(specialSkillId, new List<SpecialSkillTemplate>());                          
                            }

                            Cache.Gi().SPECIAL_SKILL_TEMPLATES[specialSkillId].Add(skillTemplate);
                        }
                    reader.Close();
                    #endregion
                }

                #region Create NClass Data
                for (var i = 0; i < 5; i++)
                {
                    var nClass = new NClass();
                    nClass.Id = i;
                    nClass.SkillTemplates = Cache.Gi().SKILL_TEMPLATES.Where(x => i < 4 && (x.NClass == i || x.NClass == 3)).ToList();
                    nClass.Name = i switch
                    {
                        0 => "Trái Đất",
                        1 => "Namek",
                        2 => "Saiyan",
                        3 => "Chung",
                        4 => "Khỉ đột",
                        _ => nClass.Name
                    };
                    Cache.Gi().NClasses.Add(nClass);
                }
                #endregion
            }
            DbContext.gI()?.CloseConnect();
        }

        private void InitDBAccount()
        {
            DbContext.gI()?.ConnectToAccount();
            using (DbCommand command = DbContext.gI()?.Connection.CreateCommand())
            {
                

                if (command != null)
                {
                    #region Read Game info
                    command.CommandText = "SELECT * FROM `gameinfo`";
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        Cache.Gi().GAME_INFO_TEMPLATES.Clear();
                        while (reader.Read())
                        {
                            Cache.Gi().GAME_INFO_TEMPLATES.Add(new GameInfoTemplate()
                            {
                                Id = reader.GetInt32(0),
                                Main = reader.GetString(1),
                                Content = reader.GetString(2)
                            });
                        }
                    }

                    reader.Close();

                    #endregion
                    
                    #region Read magictree

                    command.CommandText = "SELECT * FROM `magictree`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        MagicTreeManager.Entrys.Clear();
                        while (reader.Read())
                        {
                            var id = reader.GetInt32(0);
                            MagicTreeManager.Add(new MagicTree()
                            {
                                Id = id,
                                NpcId = reader.GetInt16(1),
                                X = reader.GetInt16(2),
                                Y = reader.GetInt16(3),
                                Level = reader.GetInt32(4),
                                Peas = reader.GetInt32(5),
                                MaxPea = reader.GetInt32(6),
                                Seconds = reader.GetInt64(7),
                                IsUpdate = byte.Parse(reader.GetString(8)) == 1 ? true : false,
                                Diamond = reader.GetInt32(9),
                            });
                        }
                    }
                    reader.Close();
                    #endregion

                    #region Read Clan

                    command.CommandText = "SELECT * FROM `clan`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        ClanManager.Entrys.Clear();
                        while (reader.Read())
                        {
                            ClanManager.Add(new Clan()
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Slogan = reader.GetString(2),
                                ImgId = reader.GetInt32(3),
                                Power = reader.GetInt64(4),
                                LeaderName = reader.GetString(5),
                                CurrMember = reader.GetInt32(6),
                                MaxMember = reader.GetInt32(7),
                                Date = reader.GetInt32(8),
                                Level = reader.GetInt32(9),
                                Point = reader.GetInt32(10),
                                Members = JsonConvert.DeserializeObject<List<ClanMember>>(reader.GetString(11)),
                                Messages = JsonConvert.DeserializeObject<List<ClanMessage>>(reader.GetString(12)),
                                CharacterPeas = JsonConvert.DeserializeObject<List<CharacterPea>>(reader.GetString(13)),
                            });
                        }
                    }

                    reader.Close();
                    #endregion

                    #region Read regex chat
                    command.CommandText = "SELECT * FROM `regexchat`";
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        Cache.Gi().RegexText.Clear();
                        while (reader.Read())
                        {
                            var text = reader.GetString(1);
                            text = text.ToLower();
                            if (text != "" && !Cache.Gi().RegexText.Contains(text))
                            {
                                Cache.Gi().RegexText.Add(text);
                            }
                        }
                    }
                    reader.Close();
                    #endregion
                }
               
            }
            DbContext.gI()?.CloseConnect();
        }

        private void ClearCache()
        {
            Cache.ClearCache();
        }
        
        private void InitCache()
        {
            #region Load Map From Resource
            Cache.Gi().TILE_MAPS.ForEach(tile => tile.LoadMapFromResource());
            #endregion
            SetupPart();
            
            Cache.Gi().NR_DART.AddRange(InitCacheDart());
            Cache.Gi().NR_ARROW.AddRange(InitCacheArrow());
            Cache.Gi().NR_IMAGE.AddRange(InitCacheImage());
            Cache.Gi().NR_EFFECT.AddRange(InitCacheEffect());
            Cache.Gi().NR_PART.AddRange(InitCachePart());
            Cache.Gi().NR_SKILL.AddRange(InitCacheSkill()); 
            Cache.Gi().NRMAP.AddRange(InitCacheNrmap()); 
            Cache.Gi().NRSKILL.AddRange(InitCacheNRSKILL()); 
            Cache.Gi().DataItemTemplateOld.AddRange(SetupItemTemplateOld()); 
            Cache.Gi().DataItemTemplateNew.AddRange(SetupItemTemplateNew()); 
            Cache.Gi().VersionIconX1.AddRange(SetupVersionIconX1()); 
            Cache.Gi().VersionIconX2.AddRange(SetupVersionIconX2()); 
            Cache.Gi().VersionIconX3.AddRange(SetupVersionIconX3()); 
            Cache.Gi().VersionIconX4.AddRange(SetupVersionIconX4()); 
            
            
            Cache.Gi().NRDarts.Clear();
            Cache.Gi().NRArrows.Clear();
            Cache.Gi().NREffects.Clear();
            Cache.Gi().NRImages.Clear();
            Cache.Gi().NRParts.Clear();
            Cache.Gi().NRSkills.Clear();
        }
        
        private void InitMapServer()
        {
            MapManager.InitMapServer();
        }

        private static IEnumerable<sbyte> InitCacheDart()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            writer.WriteShort(Cache.Gi().NRDarts.Count);
            Cache.Gi().NRDarts.ForEach(dart =>
            {
                writer.WriteShort(dart.Id);
                writer.WriteShort(dart.NUpdate);
                writer.WriteShort(dart.Va);
                writer.WriteShort(dart.XdPercent);
                        
                writer.WriteShort(dart.Tail.Length);
                foreach (var t in dart.Tail)
                    writer.WriteShort(t);
                        
                writer.WriteShort(dart.TailBorder.Length);
                foreach (var t in dart.TailBorder)
                    writer.WriteShort(t);
                        
                writer.WriteShort(dart.Xd1.Length);
                foreach (var t in dart.Xd1)
                    writer.WriteShort(t);
                        
                writer.WriteShort(dart.Xd2.Length);
                foreach (var t in dart.Xd2)
                    writer.WriteShort(t);
                        
                writer.WriteShort(dart.Head.Length);
                foreach (var h in dart.Head)
                {
                    writer.WriteShort(h.Length);
                    foreach (var s in h)
                        writer.WriteShort((short)s);
                }
                        
                writer.WriteShort(dart.HeadBorder.Length);
                foreach (var h in dart.HeadBorder)
                {
                    writer.WriteShort(h.Length);
                    foreach (var s in h)
                        writer.WriteShort(s);
                }
            });
            data = writer.GetData().ToList();
            writer.Close();
            return data;
        }
        private static IEnumerable<sbyte> InitCacheArrow()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            writer.WriteShort(Cache.Gi().NRArrows.Count);
            Cache.Gi().NRArrows.ForEach(arrow =>
            {
                writer.WriteShort(arrow.Id);
                arrow.Data.ForEach(d => writer.WriteShort(d));
            });
            data = writer.GetData().ToList();
            writer.Close();
            return data;
        }
        
        private static IEnumerable<sbyte> InitCacheEffect()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            writer.WriteShort(Cache.Gi().NREffects.Count);
            Cache.Gi().NREffects.ForEach(eff =>
            {
                writer.WriteShort(eff.Id);
                writer.WriteByte(eff.Data.Length);
                foreach (var effData in eff.Data)
                {
                    writer.WriteShort(effData[0]);
                    writer.WriteByte(effData[1]);
                    writer.WriteByte(effData[2]);
                }
            });
            data = writer.GetData().ToList();
            writer.Close();
            return data;
        }
        
        private static IEnumerable<sbyte> InitCacheImage()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            writer.WriteShort(Cache.Gi().NRImages.Count);
            Cache.Gi().NRImages.ForEach(img =>
            {
                writer.WriteByte(img.Data[0]);
                writer.WriteShort(img.Data[1]);
                writer.WriteShort(img.Data[2]);
                writer.WriteShort(img.Data[3]);
                writer.WriteShort(img.Data[4]);
            });
            data = writer.GetData().ToList();
            writer.Close();
            return data;
        }
        
        private static IEnumerable<sbyte> InitCachePart()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            writer.WriteShort(Cache.Gi().NRParts.Count);
            Cache.Gi().NRParts.ForEach(p =>
            {
                writer.WriteByte(p.Type);
                foreach (var part in p.Data)
                {
                    writer.WriteShort(part[0]);
                    writer.WriteByte(part[1]);
                    writer.WriteByte(part[2]);
                }
            });
            data = writer.GetData().ToList();
            writer.Close();
            return data;
        }

        private static IEnumerable<sbyte> InitCacheSkill()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            writer.WriteShort(Cache.Gi().NRSkills.Count);
            Cache.Gi().NRSkills.ForEach(skill =>
            {
                writer.WriteShort(skill.Id);
                writer.WriteShort(skill.EffectHappenOnMob);
                writer.WriteByte(skill.NumEff);
                writer.WriteByte(skill.SkillStand.Length);
                foreach (var stand in skill.SkillStand)
                {
                    writer.WriteByte( stand[0]);
                    writer.WriteShort( stand[1]);
                    writer.WriteShort( stand[2]);
                    writer.WriteShort( stand[3]);
                    writer.WriteShort( stand[4]);
                    writer.WriteShort( stand[5]);
                    writer.WriteShort( stand[6]);
                    writer.WriteShort( stand[7]);
                    writer.WriteShort( stand[8]);
                    writer.WriteShort( stand[9]);
                    writer.WriteShort( stand[10]);
                    writer.WriteShort( stand[11]);
                    writer.WriteShort( stand[12]);
                }
            
                writer.WriteByte(skill.SkillFly.Length);
                foreach (var fly in skill.SkillFly)
                {
                    writer.WriteByte( fly[0]);
                    writer.WriteShort( fly[1]);
                    writer.WriteShort( fly[2]);
                    writer.WriteShort( fly[3]);
                    writer.WriteShort( fly[4]);
                    writer.WriteShort( fly[5]);
                    writer.WriteShort( fly[6]);
                    writer.WriteShort( fly[7]);
                    writer.WriteShort( fly[8]);
                    writer.WriteShort( fly[9]);
                    writer.WriteShort( fly[10]);
                    writer.WriteShort( fly[11]);
                    writer.WriteShort( fly[12]);
                }
            });
            data = writer.GetData().ToList();
            writer.Close();
            return data;
        }
        private static IEnumerable<sbyte> InitCacheNrmap()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            writer.WriteByte(Manager.gI().MapVersion);
            writer.WriteByte(Cache.Gi().TILE_MAPS.Count);
            Cache.Gi().TILE_MAPS.ForEach(tileMap =>
            {
                writer.WriteUTF(tileMap.Name);
            });

            writer.WriteByte(Cache.Gi().NPC_TEMPLATES.Count);
            Cache.Gi().NPC_TEMPLATES.ForEach(npcTemplate =>
            {
                writer.WriteUTF(npcTemplate.Name);
                writer.WriteShort(npcTemplate.Head);
                writer.WriteShort(npcTemplate.Body);
                writer.WriteShort(npcTemplate.Leg);
                writer.WriteByte(npcTemplate.Menu.Length);
                foreach (var menus in npcTemplate.Menu)
                {
                    writer.WriteByte(menus.Length);
                    foreach (var menu in menus) {
                        writer.WriteUTF(menu);
                    }
                } 
            });

            writer.WriteByte(Cache.Gi().MONSTER_TEMPLATES.Count);
            Cache.Gi().MONSTER_TEMPLATES.ForEach(monsterTemplate =>
            {
                writer.WriteByte(monsterTemplate.Type);
                writer.WriteUTF(monsterTemplate.Name);
                writer.WriteInt(monsterTemplate.Hp);
                writer.WriteByte(monsterTemplate.RangeMove);
                writer.WriteByte(monsterTemplate.Speed);
                writer.WriteByte(monsterTemplate.DartType);
            });
            data = writer.GetData().ToList();
            writer.Close();
            return data;
        }
        
        private IEnumerable<sbyte> InitCacheNRSKILL()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            writer.WriteByte(Manager.gI().SkillVersion);
            writer.WriteByte(Cache.Gi().SKILL_OPTIONS.Count);
            Cache.Gi().SKILL_OPTIONS.ForEach(option =>
            {
                writer.WriteUTF(option.Name);
            });
            writer.WriteByte(Cache.Gi().NClasses.Count);
            Cache.Gi().NClasses.ForEach(n =>
            {
                writer.WriteUTF(n.Name);
                writer.WriteByte(n.SkillTemplates.Count);
                n.SkillTemplates.ForEach(s =>
                {
                    writer.WriteByte(s.Id);
                    writer.WriteUTF(s.Name);
                    writer.WriteByte(s.MaxPoint);
                    writer.WriteByte(s.ManaUseType);
                    writer.WriteByte(s.Type);
                    writer.WriteShort(s.IconId);
                    writer.WriteUTF(s.DamageInfo);
                    writer.WriteUTF(s.Description);
                    writer.WriteByte(s.SkillDataTemplates.Count);
                    s.SkillDataTemplates.ForEach(d =>
                    {
                        writer.WriteShort(d.SkillId);
                        writer.WriteByte(d.Point);
                        writer.WriteLong(d.PowRequire);
                        writer.WriteShort(d.ManaUse);
                        writer.WriteInt(d.CoolDown);
                        writer.WriteShort(d.Dx);
                        writer.WriteShort(d.Dy);
                        writer.WriteByte(d.MaxFight);
                        writer.WriteShort(d.Damage);
                        writer.WriteShort(d.Price);
                        writer.WriteUTF(d.MoreInfo);
                    });
                });
            });
            data = writer.GetData().ToList();
            writer.Close();
            return data;
        }

        public void SetupPart()
        {
            Cache.Gi().NRParts.Where(part => part.Type == 0).ToList().ForEach(p =>
            {
                Cache.Gi().PARTS.TryAdd(p.Data[0][0], p.Id);
                Cache.Gi().PARTS.TryAdd(p.Data[1][0], p.Id);
                Cache.Gi().PARTS.TryAdd(p.Data[2][0], p.Id);
            }); 
        }

        private static IEnumerable<sbyte> SetupItemTemplateOld()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            writer.WriteShort(Manager.gI().ItemOld);
            for (var i = 0; i < Manager.gI().ItemOld; i++)
            {
                var itemTemplate = Cache.Gi().ITEM_TEMPLATES[(short)i];
                writer.WriteByte(itemTemplate.Type);
                writer.WriteByte(itemTemplate.Gender);
                writer.WriteUTF(itemTemplate.Name);
                writer.WriteUTF(itemTemplate.Description);
                writer.WriteByte(itemTemplate.Level);
                writer.WriteInt((int)itemTemplate.Require);
                writer.WriteShort(itemTemplate.IconId);
                writer.WriteShort(itemTemplate.Part);
                writer.WriteBoolean(itemTemplate.IsUpToUp);
            }
            data = writer.GetData().ToList();
            writer.Close();
            return data;
        }

        private static IEnumerable<sbyte> SetupItemTemplateNew()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            if(Manager.gI().ItemNew - Manager.gI().ItemOld == 0) {
                writer.WriteShort(0);
                writer.WriteShort(0);
            } else {
                writer.WriteShort(Manager.gI().ItemOld);
                writer.WriteShort(Manager.gI().ItemNew);
                for(var i = Manager.gI().ItemOld; i < Manager.gI().ItemNew; i++)
                {
                    var itemTemplate = Cache.Gi().ITEM_TEMPLATES[i];
                    writer.WriteByte(itemTemplate.Type);
                    writer.WriteByte(itemTemplate.Gender);
                    writer.WriteUTF(itemTemplate.Name);
                    writer.WriteUTF(itemTemplate.Description);
                    writer.WriteByte(itemTemplate.Level);
                    writer.WriteInt((int)itemTemplate.Require);
                    writer.WriteShort(itemTemplate.IconId);
                    writer.WriteShort(itemTemplate.Part);
                    writer.WriteBoolean(itemTemplate.IsUpToUp); 
                }
            }
            data = writer.GetData().ToList();
            writer.Close();
            return data;
        }

        private static IEnumerable<sbyte> SetupVersionIconX1()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            Console.WriteLine("Setup Version Icon X1");
            for (var i = 0; i < 9981; i++)
            {
                try
                {
                    if (i >= 9652 && i <= 9717) {
                        writer.WriteByte(0);
                    }
                    else 
                    {
                        var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().IconImg, 1, i));
                        var bytes = ServerUtils.ReadFileBytes(path);
                        Cache.Gi().DATA_ICON_X1.TryAdd(i, bytes.ToList());
                        writer.WriteByte(bytes.Length%127);
                    }
                }
                catch (Exception)
                {
                    writer.WriteByte(0);
                }
            }
            data = writer.GetData().ToList();
            writer.Close();
            Console.WriteLine("Setup Version Icon X1 [Done]");
            return data;
        }

        private static IEnumerable<sbyte> SetupVersionIconX2()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            Console.WriteLine("Setup Version Icon X2");
            for (var i = 0; i < 9981; i++)
            {
                try
                {
                    if (i >= 9652 && i <= 9717) 
                    {
                        writer.WriteByte(0);
                    }
                    else 
                    {
                        var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().IconImg, 2, i));
                        var bytes = ServerUtils.ReadFileBytes(path);
                        Cache.Gi().DATA_ICON_X2.TryAdd(i, bytes.ToList());
                        writer.WriteByte(bytes.Length%127);
                    }
                }
                catch (Exception)
                {
                    writer.WriteByte(0);
                }
            }
            data = writer.GetData().ToList();
            writer.Close();
            Console.WriteLine("Setup Version Icon X2 [Done]");
            return data;
        }

        private static IEnumerable<sbyte> SetupVersionIconX3()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            Console.WriteLine("Setup Version Icon X3");
            for (var i = 0; i < 9981; i++)
            {
                try
                {
                    if (i >= 9652 && i <= 9717) {
                        writer.WriteByte(0);
                    }
                    else 
                    {
                        var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().IconImg, 3, i));
                        var bytes = ServerUtils.ReadFileBytes(path);
                        Cache.Gi().DATA_ICON_X3.TryAdd(i, bytes.ToList());
                        writer.WriteByte(bytes.Length%127);
                    }
                }
                catch (Exception)
                {
                    writer.WriteByte(0);
                }
            }
            data = writer.GetData().ToList();
            writer.Close();
            Console.WriteLine("Setup Version Icon X3 [Done]");
            return data;
        }

        private static IEnumerable<sbyte> SetupVersionIconX4()
        {
            List<sbyte> data;
            var writer = new MyWriter();
            Console.WriteLine("Setup Version Icon X4");
            for (var i = 0; i < 9981; i++)
            {
                try
                {
                    if (i >= 9652 && i <= 9717) {
                        writer.WriteByte(0);
                    }
                    else 
                    {
                        var path = ServerUtils.ProjectDir(string.Format(DatabaseManager.Manager.gI().IconImg, 4, i));
                        var bytes = ServerUtils.ReadFileBytes(path);
                        Cache.Gi().DATA_ICON_X4.TryAdd(i, bytes.ToList());
                        writer.WriteByte(bytes.Length%127);
                    }
                }
                catch (Exception)
                {
                    writer.WriteByte(0);
                }
            }
            data = writer.GetData().ToList();
            writer.Close();
            Console.WriteLine("Setup Version Icon X4 [Done]");
            return data;
        }
    }
}
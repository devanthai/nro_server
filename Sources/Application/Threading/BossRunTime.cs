﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NRO_Server.Application.IO;
using NRO_Server.Application.Main;
using NRO_Server.Application.Manager;
using NRO_Server.Application.Constants;
using Org.BouncyCastle.Math.Field;
using NRO_Server.Model;
using NRO_Server.Model.Character;
using NRO_Server.DatabaseManager;

namespace NRO_Server.Application.Threading
{
    public class BossRunTime
    {
        private static BossRunTime Instance { get; set; } = null;

        public static bool IsStop = false;

        #region Super Broly Boss
        private static bool IsSuperBrolySpawn = false;
        private static List<int> SuperBrolyMaps = new List<int>{5, 29, 13, 20, 36, 38, 33};
        private static Boss superBroly = null;
        private static int superBrolyId = -1;
        private static bool IsSuperBrolyNotify = false;
        private static long superBrolySpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
        #endregion

        #region Black Goku
        private static List<int> BlackGokuMaps = new List<int>{92, 93, 94};
        private static Boss blackGoku = null;
        private static int blackGokuId = -1;
        private static bool IsBlackGokuSpawn = false;
        private static bool IsBlackGokuNotify = false;

        private static Boss superBlackGoku = null;
        private static int superBlackGokuId = -1;
        private static bool IsSuperBlackGokuSpawn = false;
        private static bool IsSuperBlackGokuNotify = false;

        private static long blackGokuSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
        #endregion

        #region Fide

        private static List<int> FideMaps = new List<int>{68, 69, 70, 71, 72};
        private static int CurrentFideMapId = 0;
        private static int CurrentFideZoneId = 0;


        private static Boss fide01 = null;
        private static Boss fide02 = null;
        private static Boss fide03 = null;

        private static bool IsFide01Spawn = false;
        private static bool IsFide01Notify = false;
        private static int fide01Id = -1;
        private static long fide01SpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();

        private static bool IsFide02Spawn = false;
        private static bool IsFide02Notify = false;
        private static int fide02Id = -1;

        private static bool IsFide03Spawn = false;
        private static bool IsFide03Notify = false;
        private static int fide03Id = -1;

        #endregion

        #region Cell

        private static List<int> CellMaps = new List<int>{103};
        private static Boss cell01 = null;
        private static Boss cell02 = null;
        private static Boss cell03 = null;
        private static bool IsCell01Spawn = false;
        private static bool IsCell02Spawn = false;
        private static bool IsCell03Spawn = false;
        private static bool IsCellNotify = false;
        private static int cell01Id = -1;
        private static int cell02Id = -1;
        private static int cell03Id = -1;
        private static long cellSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();

        #endregion

        #region Cooler

        private static List<int> CoolerMaps = new List<int>{107,108,110};
        private static int CurrentCoolerMapId = 0;
        private static int CurrentCoolerZoneId = 0;

        private static Boss cooler01 = null;
        private static Boss cooler02 = null;

        private static bool IsCooler01Spawn = false;
        private static bool IsCooler01Notify = false;
        private static int cooler01Id = -1;
        private static long cooler01SpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();

        private static bool IsCooler02Spawn = false;
        private static bool IsCooler02Notify = false;
        private static int cooler02Id = -1;


        #endregion
        #region Boss thỏ phê cỏ
        // private static bool IsThoPheCoSpawn = false;
        // private static List<int> ThoPheCoMaps = new List<int>{0,5,7,14};
        // private static Boss ThoPheCo = null;
        // private static int ThoPheCoId = -1;
        // private static bool IsThoPheCoNotify = false;
        // private static long ThoPheCoSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
        #endregion

        #region Boss thỏ đại ca
        // private static bool IsThoDaiCaSpawn = false;
        // private static List<int> ThoDaiCaMaps = new List<int>{5, 29, 13, 20, 36, 38, 33};
        // private static Boss ThoDaiCa = null;
        // private static int ThoDaiCaId = -1;
        // private static bool IsThoDaiCaNotify = false;
        // private static long ThoDaiCaSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
        #endregion

        #region Chilled
        private static bool IsChilledSpawn = false;
        private static List<int> ChilledMaps = new List<int>{161};
        private static Boss Chilled = null;
        private static int ChilledId = -1;
        private static bool IsChilledNotify = false;
        private static long ChilledSpawnTimeDelay = 600000 + ServerUtils.CurrentTimeMillis();
        #endregion
        public static BossRunTime Gi()
        {
            return Instance ??= new BossRunTime();
        }

        public BossRunTime()
        {
            
        }

        public void BossDie(int bossId)
        {
            try
            {
                if (bossId == superBrolyId && IsSuperBrolySpawn)
                {
                    superBroly = null;
                    IsSuperBrolySpawn = false;
                    superBrolyId = -1;
                    superBrolySpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                }
                else if (bossId == blackGokuId && IsBlackGokuSpawn)
                {
                    blackGoku = null;
                    IsBlackGokuSpawn = false;
                    blackGokuId = -1;
                    blackGokuSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                    if (!IsSuperBlackGokuSpawn)
                    {
                        SpawnSuperBlackGoku();
                    }
                }
                else if (bossId == superBlackGokuId && IsSuperBlackGokuSpawn)
                {
                    superBlackGoku = null;
                    IsSuperBlackGokuSpawn = false;
                    superBlackGokuId = -1;
                    blackGokuSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                }
                else if (bossId == fide01Id && IsFide01Spawn)
                {
                    fide01 = null;
                    IsFide01Spawn = false;
                    fide01Id = -1;
                    fide01SpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                    if (!IsFide02Spawn)
                    {
                        SpawnFideBoss(2);
                    }
                }
                else if (bossId == fide02Id && IsFide02Spawn)
                {
                    fide02 = null;
                    IsFide02Spawn = false;
                    fide02Id = -1;
                    fide01SpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                    if (!IsFide03Spawn)
                    {
                        SpawnFideBoss(3);
                    }
                }
                else if (bossId == fide03Id && IsFide03Spawn)
                {
                    fide03 = null;
                    IsFide03Spawn = false;
                    fide03Id = -1;
                    fide01SpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                }
                else if (bossId == cell01Id && IsCell01Spawn)
                {
                    cell01 = null;
                    IsCell01Spawn = false;
                    cell01Id = -1;
                    cellSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                }
                else if (bossId == cell02Id && IsCell02Spawn)
                {
                    cell02 = null;
                    IsCell02Spawn = false;
                    cell02Id = -1;
                    cellSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                }
                else if (bossId == cell03Id && IsCell03Spawn)
                {
                    cell03 = null;
                    IsCell03Spawn = false;
                    cell03Id = -1;
                    cellSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                }
                else if (bossId == cooler01Id && IsCooler01Spawn)
                {
                    cooler01 = null;
                    IsCooler01Spawn = false;
                    cooler01Id = -1;
                    cooler01SpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                    if (!IsCooler02Spawn)
                    {
                        SpawnCoolerBoss(2);
                    }
                }
                else if (bossId == cooler02Id && IsCooler02Spawn)
                {
                    cooler02 = null;
                    IsCooler02Spawn = false;
                    cooler02Id = -1;
                    cooler01SpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                } 
                // else if (bossId == ThoPheCoId && IsThoPheCoSpawn)
                // {
                //     ThoPheCo = null;
                //     IsThoPheCoSpawn = false;
                //     ThoPheCoId = -1;
                //     ThoPheCoSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                // }
                // else if (bossId == ThoDaiCaId && IsThoDaiCaSpawn)
                // {
                //     ThoDaiCa = null;
                //     IsThoDaiCaSpawn = false;
                //     ThoDaiCaId = -1;
                //     ThoDaiCaSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                // }
                else if (bossId == ChilledId && IsChilledSpawn)
                {
                    Chilled = null;
                    IsChilledSpawn = false;
                    ChilledId = -1;
                    ChilledSpawnTimeDelay = 600000 + ServerUtils.CurrentTimeMillis();
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error BossDie in BossRunTime.cs: {e.Message} \n {e.StackTrace}", e);
            }
            
        }

        public void StartBossRunTime()
        {
            new Thread(new ThreadStart(() =>
            {
                while (Server.Gi().IsRunning)
                {
                    var now = ServerUtils.TimeNow();
                    try
                    {
                        #region Super Broly

                        if ((superBrolySpawnTimeDelay < ServerUtils.CurrentTimeMillis())) 
                        {
                            superBrolySpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                            if (!IsSuperBrolySpawn)
                            {
                                IsSuperBrolySpawn = true;
                                int sbRandomZoneNum = ServerUtils.RandomNumber(0, 15);
                                int sbRandomMapIndex = ServerUtils.RandomNumber(SuperBrolyMaps.Count);
                                int sbRandomMap = SuperBrolyMaps[sbRandomMapIndex];
                                var zone = MapManager.Get(sbRandomMap)?.GetZoneById(sbRandomZoneNum);
                                if (zone != null)
                                {
                                    superBroly = new Boss();
                                    superBroly.CreateBoss(DataCache.BOSS_SUPER_BROLY_TYPE);
                                    superBroly.CharacterHandler.SetUpInfo();
                                    zone.ZoneHandler.AddBoss(superBroly);
                                    superBrolyId = superBroly.Id;
                                    IsSuperBrolySpawn = true;
                                    ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Super Broly " + superBrolyId + " vừa xuất hiện tại " + zone.Map.TileMap.Name ));
                                    Server.Gi().Logger.Print("Boss Spawn Broly...: Map " + sbRandomMap + " Zone: " + sbRandomZoneNum + " Name: " + zone.Map.TileMap.Name + " DataCache.CURRENT_BOSS_ID: " + DataCache.CURRENT_BOSS_ID);
                                    IsSuperBrolyNotify = true;
                                }
                            }
                            else if (!IsSuperBrolyNotify && superBroly != null && superBrolyId != -1)
                            {
                                ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Super Broly " + superBrolyId + " vừa xuất hiện tại " + superBroly.Zone.Map.TileMap.Name));
                                IsSuperBrolyNotify = true;
                            }
                            // Get Random Map
                        } 
                        else 
                        {
                            IsSuperBrolyNotify = false;
                        }

                        #endregion

                        #region Black Goku

                        if ((now.Hour == 7 || now.Hour == 8 || now.Hour == 10 ||now.Hour == 12 || now.Hour == 13 || now.Hour == 14 || now.Hour == 15 ||now.Hour == 19 || now.Hour == 22 || now.Hour == 1 || now.Hour == 3 || now.Hour == 4) && (blackGokuSpawnTimeDelay < ServerUtils.CurrentTimeMillis())) 
                        {
                            blackGokuSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                            if (!IsBlackGokuSpawn && !IsSuperBlackGokuSpawn) 
                            {
                                IsBlackGokuSpawn = true;
                                int sbRandomZoneNum = ServerUtils.RandomNumber(0, 15);
                                int sbRandomMapIndex = ServerUtils.RandomNumber(BlackGokuMaps.Count);
                                int sbRandomMap = BlackGokuMaps[sbRandomMapIndex];
                                var zone = MapManager.Get(sbRandomMap)?.GetZoneById(sbRandomZoneNum);
                                if (zone != null)
                                {
                                    blackGoku = new Boss();
                                    blackGoku.CreateBoss(DataCache.BOSS_BLACK_GOKU_TYPE);
                                    blackGoku.CharacterHandler.SetUpInfo();
                                    zone.ZoneHandler.AddBoss(blackGoku);
                                    blackGokuId = blackGoku.Id;
                                    IsBlackGokuSpawn = true;
                                    IsBlackGokuNotify = true;
                                    ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Black Goku " + blackGokuId + " vừa xuất hiện tại " + zone.Map.TileMap.Name));
                                    Server.Gi().Logger.Print("Boss Spawn Black Goku...: Map " + sbRandomMap + " Zone: " + sbRandomZoneNum + " Name: " + zone.Map.TileMap.Name + " DataCache.CURRENT_BOSS_ID: " + DataCache.CURRENT_BOSS_ID);
                                }
                                // Get Random Map
                            }
                            else if(!IsBlackGokuNotify && IsBlackGokuSpawn && blackGoku != null && blackGokuId != -1)
                            {
                                ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Black Goku " + blackGokuId + " vừa xuất hiện tại " + blackGoku.Zone.Map.TileMap.Name));
                                IsBlackGokuNotify = true;
                            }
                            else if(!IsSuperBlackGokuNotify && IsSuperBlackGokuSpawn && superBlackGoku != null && superBlackGokuId != -1)
                            {
                                ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Super Black Goku " + superBlackGokuId + " vừa xuất hiện tại " + superBlackGoku.Zone.Map.TileMap.Name));
                                IsSuperBlackGokuNotify = true;
                            }
                        }
                        else 
                        {
                            IsBlackGokuNotify = false;
                            IsSuperBlackGokuNotify = false;
                        }

                        #endregion
                        
                        #region Fide 01 02 03
                        if ((now.Hour == 7 || now.Hour == 8 ||now.Hour == 13 || now.Hour == 20 || now.Hour == 22) && (fide01SpawnTimeDelay < ServerUtils.CurrentTimeMillis())) 
                        {
                            fide01SpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                            if (!IsFide01Spawn && !IsFide02Spawn && !IsFide03Spawn)
                            {
                                IsFide01Spawn = true;
                                int sbRandomZoneNum = ServerUtils.RandomNumber(0, 15);
                                int sbRandomMapIndex = ServerUtils.RandomNumber(FideMaps.Count);
                                int sbRandomMap = FideMaps[sbRandomMapIndex];
                                var zone = MapManager.Get(sbRandomMap)?.GetZoneById(sbRandomZoneNum);
                                CurrentFideMapId = sbRandomMap;
                                CurrentFideZoneId = sbRandomZoneNum;
                                if (zone != null)
                                {
                                    fide01 = new Boss();
                                    fide01.CreateBoss(DataCache.BOSS_FIDE_01_TYPE);
                                    fide01.CharacterHandler.SetUpInfo();
                                    zone.ZoneHandler.AddBoss(fide01);
                                    fide01Id = fide01.Id;
                                    IsFide01Spawn = true;
                                    IsFide01Notify = true;
                                    ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Fide 01 " + fide01Id + " vừa xuất hiện tại " + zone.Map.TileMap.Name ));
                                    Server.Gi().Logger.Print("Fide...: Map " + sbRandomMap + " Zone: " + sbRandomZoneNum + " Name: " + zone.Map.TileMap.Name + " DataCache.CURRENT_BOSS_ID: " + DataCache.CURRENT_BOSS_ID);
                                }
                            }
                            else if (!IsFide01Notify && IsFide01Spawn && fide01Id != -1 && fide01 != null)
                            {
                                ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Fide 01 " + fide01Id + " vừa xuất hiện tại " + fide01.Zone.Map.TileMap.Name));
                                IsFide01Notify = true;
                            }
                            else if (!IsFide02Notify && IsFide02Spawn && fide02Id != -1 && fide02 != null)
                            {
                                ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Fide 02 "+fide02Id+" vừa xuất hiện tại " + fide02.Zone.Map.TileMap.Name));
                                IsFide02Notify = true;
                            }
                            else if (!IsFide03Notify && IsFide03Spawn && fide03Id != -1 && fide03 != null)
                            {
                                ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Fide 03 "+fide03Id+" vừa xuất hiện tại " + fide03.Zone.Map.TileMap.Name));
                                IsFide03Notify = true;
                            }
                            // Get Random Map
                        } 
                        else 
                        {
                            IsFide01Notify = false;
                            IsFide02Notify = false;
                            IsFide03Notify = false;
                        }
                        #endregion

                        #region Xên bọ hung

                        if ((now.Hour == 6 || now.Hour == 8 ||now.Hour == 11 ||now.Hour == 12 ||now.Hour == 15 ||now.Hour == 16 ||now.Hour == 19 || now.Hour == 22 || now.Hour == 1 || now.Hour == 3) && (cellSpawnTimeDelay < ServerUtils.CurrentTimeMillis())) 
                        {
                            cellSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                            if (!IsCell01Spawn && !IsCell02Spawn && !IsCell03Spawn)
                            {
                                
                                int sbRandomZoneNum = ServerUtils.RandomNumber(0, 15);
                                int sbRandomMapIndex = ServerUtils.RandomNumber(CellMaps.Count);
                                int sbRandomMap = CellMaps[sbRandomMapIndex];
                                var zone = MapManager.Get(sbRandomMap)?.GetZoneById(sbRandomZoneNum);
                                if (zone != null)
                                {
                                    cell01 = new Boss();
                                    cell01.CreateBoss(DataCache.BOSS_CELL_01_TYPE);
                                    cell01.CharacterHandler.SetUpInfo();
                                    zone.ZoneHandler.AddBoss(cell01);
                                    cell01Id = cell01.Id;

                                    cell02 = new Boss();
                                    cell02.CreateBoss(DataCache.BOSS_CELL_02_TYPE);
                                    cell02.CharacterHandler.SetUpInfo();
                                    zone.ZoneHandler.AddBoss(cell02);
                                    cell02Id = cell02.Id;

                                    cell03 = new Boss();
                                    cell03.CreateBoss(DataCache.BOSS_CELL_03_TYPE);
                                    cell03.CharacterHandler.SetUpInfo();
                                    zone.ZoneHandler.AddBoss(cell03);
                                    cell03Id = cell03.Id;

                                    IsCell01Spawn = true;
                                    IsCell02Spawn = true;
                                    IsCell03Spawn = true;
                                    IsCellNotify = true;
                                    ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Bộ ba Xên Bọ Hung vừa xuất hiện tại " + zone.Map.TileMap.Name ));
                                    Server.Gi().Logger.Print("Bộ ba Xên Bọ Hung..: Map " + sbRandomMap + " Zone: " + sbRandomZoneNum + " Name: " + zone.Map.TileMap.Name + " DataCache.CURRENT_BOSS_ID: " + DataCache.CURRENT_BOSS_ID);
                                }
                            }
                            else if (!IsCellNotify)
                            {
                                if (cell01 != null && cell01Id != -1)
                                {
                                    ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Bộ ba Xên Bọ Hung vừa xuất hiện tại " + cell01.Zone.Map.TileMap.Name));
                                }
                                else if (cell02 != null && cell02Id != -1)
                                {
                                    ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Bộ ba Xên Bọ Hung vừa xuất hiện tại " + cell02.Zone.Map.TileMap.Name));
                                }
                                else if (cell03 != null && cell03Id != -1)
                                {
                                    ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Bộ ba Xên Bọ Hung vừa xuất hiện tại " + cell03.Zone.Map.TileMap.Name));
                                }
                                IsCellNotify = true;
                            }
                            // Get Random Map
                        } 
                        else 
                        {
                            IsCellNotify = false;
                        }
                        
                    
                        #endregion
                        #region Cooler
                        if ((now.Hour == 23 || now.Hour == 0 ||now.Hour == 1 || now.Hour == 11 || now.Hour == 12 || now.Hour == 13) && (cooler01SpawnTimeDelay < ServerUtils.CurrentTimeMillis())) 
                        {
                            cooler01SpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                            if (!IsCooler01Spawn && !IsCooler02Spawn)
                            {
                                IsCooler01Spawn = true;
                                int sbRandomZoneNum = ServerUtils.RandomNumber(0, 15);
                                int sbRandomMapIndex = ServerUtils.RandomNumber(CoolerMaps.Count);
                                int sbRandomMap = CoolerMaps[sbRandomMapIndex];
                                var zone = MapManager.Get(sbRandomMap)?.GetZoneById(sbRandomZoneNum);
                                CurrentCoolerMapId = sbRandomMap;
                                CurrentCoolerZoneId = sbRandomZoneNum;
                                if (zone != null)
                                {
                                    cooler01 = new Boss();
                                    cooler01.CreateBoss(DataCache.BOSS_COOLER_01_TYPE);
                                    cooler01.CharacterHandler.SetUpInfo();
                                    zone.ZoneHandler.AddBoss(cooler01);
                                    cooler01Id = cooler01.Id;
                                    IsCooler01Spawn = true;
                                    IsCooler01Notify = true;
                                    ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Cooler 01 " + cooler01Id + " vừa xuất hiện tại " + zone.Map.TileMap.Name ));
                                    Server.Gi().Logger.Print("Cooler...: Map " + sbRandomMap + " Zone: " + sbRandomZoneNum + " Name: " + zone.Map.TileMap.Name + " DataCache.CURRENT_BOSS_ID: " + DataCache.CURRENT_BOSS_ID);
                                }
                            }
                            else if (!IsCooler01Notify && IsCooler01Spawn && cooler01Id != -1 && cooler01 != null)
                            {
                                ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Cooler 01 " + cooler01Id + " vừa xuất hiện tại " + cooler01.Zone.Map.TileMap.Name));
                                IsCooler01Notify = true;
                            }
                            else if (!IsCooler02Notify && IsCooler02Spawn && cooler02Id != -1 && cooler02 != null)
                            {
                                ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Cooler 02 "+cooler02Id+" vừa xuất hiện tại " + cooler02.Zone.Map.TileMap.Name));
                                IsCooler02Notify = true;
                            }
                            // Get Random Map
                        } 
                        else 
                        {
                            IsCooler01Notify = false;
                            IsCooler02Notify = false;
                        }
                        #endregion
                        #region Thỏ Phê Cỏ

                        // if ((ThoPheCoSpawnTimeDelay < ServerUtils.CurrentTimeMillis())) 
                        // {
                        //     ThoPheCoSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                        //     if (!IsThoPheCoSpawn)
                        //     {
                        //         var randChar = ClientManager.Gi().GetRandomCharacter();
                        //         if (randChar != null) 
                        //         {
                        //             var zone = randChar.Zone;
                        //             Server.Gi().Logger.Print("Boss Tho Phe Co Tim nguoi: " + randChar.Name);
                        //             if (zone != null)
                        //             {
                        //                 ThoPheCo = new Boss();
                        //                 ThoPheCo.CreateBoss(DataCache.BOSS_THO_PHE_CO_TYPE, randChar.InfoChar.X, randChar.InfoChar.Y);
                        //                 ThoPheCo.CharacterHandler.SetUpInfo();
                        //                 zone.ZoneHandler.AddBoss(ThoPheCo);
                        //                 ThoPheCoId = ThoPheCo.Id;
                        //                 IsThoPheCoSpawn = true;
                        //                 Server.Gi().Logger.Print("Boss Thỏ Phê Cỏ...: Map " + zone.Map.Id + " Zone: " + zone.Id + " Name: " + zone.Map.TileMap.Name);
                        //                 ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Thỏ Phê Cỏ " + ThoPheCoId + " vừa xuất hiện tại " + zone.Map.TileMap.Name));
                        //                 IsThoPheCoNotify = true;
                        //             }
                        //         }
                        //     }
                        //     else if (!IsThoPheCoNotify && ThoPheCo != null && ThoPheCoId != -1)
                        //     {
                        //         // if (ThoPheCo.CharacterHandler != null)
                        //         // {
                        //         //     ThoPheCo.CharacterHandler.Update();
                        //         // }
                        //         // ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Thỏ Phê Cỏ " + ThoPheCoId + " vừa xuất hiện tại " + ThoPheCo.Zone.Map.TileMap.Name));
                        //         IsThoPheCoNotify = true;
                        //     }
                        //     // Get Random Map
                        // } 
                        // else 
                        // {
                        //     IsThoPheCoNotify = false;
                        // }

                        #endregion

                        // #region Thỏ Đại Ca

                        // if ((ThoDaiCaSpawnTimeDelay < ServerUtils.CurrentTimeMillis())) 
                        // {
                        //     ThoDaiCaSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                        //     if (!IsThoDaiCaSpawn)
                        //     {
                        //         IsThoDaiCaSpawn = true;
                        //         int sbRandomZoneNum = ServerUtils.RandomNumber(0, 15);
                        //         int sbRandomMapIndex = ServerUtils.RandomNumber(ThoDaiCaMaps.Count);
                        //         int sbRandomMap = ThoDaiCaMaps[sbRandomMapIndex];
                        //         var zone = MapManager.Get(sbRandomMap)?.GetZoneById(sbRandomZoneNum);
                        //         if (zone != null)
                        //         {
                        //             ThoDaiCa = new Boss();
                        //             ThoDaiCa.CreateBoss(DataCache.BOSS_THO_DAI_CA_TYPE);
                        //             ThoDaiCa.CharacterHandler.SetUpInfo();
                        //             zone.ZoneHandler.AddBoss(ThoDaiCa);
                        //             ThoDaiCaId = ThoDaiCa.Id;
                        //             IsThoDaiCaSpawn = true;
                        //             ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Thỏ Đại Ca " + ThoDaiCaId + " vừa xuất hiện tại " + zone.Map.TileMap.Name ));
                        //             IsThoDaiCaNotify = true;
                        //         }
                        //     }
                        //     else if (!IsThoDaiCaNotify && ThoDaiCa != null && ThoDaiCaId != -1)
                        //     {
                        //         ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Thỏ Đại Ca " + ThoDaiCaId + " vừa xuất hiện tại " + ThoDaiCa.Zone.Map.TileMap.Name));
                        //         IsThoDaiCaNotify = true;
                        //     }
                        //     // Get Random Map
                        // } 
                        // else 
                        // {
                        //     IsThoDaiCaNotify = false;
                        // }

                        // #endregion

                        #region Chilled

                        // if ((ChilledSpawnTimeDelay < ServerUtils.CurrentTimeMillis())) 
                        // {
                        //     ChilledSpawnTimeDelay = 500000 + ServerUtils.CurrentTimeMillis();
                        //     if (!IsChilledSpawn)
                        //     {
                        //         IsChilledSpawn = true;
                        //         int sbRandomZoneNum = ServerUtils.RandomNumber(0, 15);
                        //         int sbRandomMapIndex = ServerUtils.RandomNumber(ChilledMaps.Count);
                        //         int sbRandomMap = ChilledMaps[sbRandomMapIndex];
                        //         var zone = MapManager.Get(sbRandomMap)?.GetZoneById(sbRandomZoneNum);
                        //         if (zone != null)
                        //         {
                        //             Chilled = new Boss();
                        //             Chilled.CreateBoss(DataCache.BOSS_CHILLED_TYPE);
                        //             Chilled.CharacterHandler.SetUpInfo();
                        //             zone.ZoneHandler.AddBoss(Chilled);
                        //             ChilledId = Chilled.Id;
                        //             IsChilledSpawn = true;
                        //             ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Chilled " + ChilledId + " vừa xuất hiện tại " + zone.Map.TileMap.Name ));
                        //             IsChilledNotify = true;
                        //         }
                        //     }
                        //     else if (!IsChilledNotify && Chilled != null && ChilledId != -1)
                        //     {
                        //         ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Chilled " + ChilledId + " vừa xuất hiện tại " + Chilled.Zone.Map.TileMap.Name));
                        //         IsChilledNotify = true;
                        //     }
                        //     // Get Random Map
                        // } 
                        // else 
                        // {
                        //     IsChilledNotify = false;
                        // }

                        #endregion

                    }
                    catch (Exception e)
                    {
                        Server.Gi().Logger.Error($"Error StartBossRunTime in BossRunTime.cs: {e.Message} \n {e.StackTrace}", e);
                    }
                    Thread.Sleep(1000);
                }
                Server.Gi().Logger.Print("Boss Runtime is close Success...");
                IsStop = true;
            })).Start();
        }

        public void SpawnSuperBlackGoku()
        {
            try
            {
                IsSuperBlackGokuSpawn = true;
                int sbRandomZoneNum = ServerUtils.RandomNumber(0, 15);
                int sbRandomMapIndex = ServerUtils.RandomNumber(BlackGokuMaps.Count);
                int sbRandomMap = BlackGokuMaps[sbRandomMapIndex];
                var zone = MapManager.Get(sbRandomMap)?.GetZoneById(sbRandomZoneNum);
                if (zone != null)
                {
                    superBlackGoku = new Boss();
                    superBlackGoku.CreateBoss(DataCache.BOSS_SUPER_BLACK_GOKU_TYPE);
                    superBlackGoku.CharacterHandler.SetUpInfo();
                    zone.ZoneHandler.AddBoss(superBlackGoku);
                    superBlackGokuId = superBlackGoku.Id;
                    IsSuperBlackGokuSpawn = true;
                    IsSuperBlackGokuNotify = true;
                    ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Super Black Goku " + superBlackGokuId + " vừa xuất hiện tại " + zone.Map.TileMap.Name ));
                    Server.Gi().Logger.Print("Super Boss Spawn Black Goku...: Map " + sbRandomMap + " Zone: " + sbRandomZoneNum + " Name: " + zone.Map.TileMap.Name + " DataCache.CURRENT_BOSS_ID: " + DataCache.CURRENT_BOSS_ID);
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SpawnSuperBlackGoku in BossRunTime.cs: {e.Message} \n {e.StackTrace}", e);
            }

            
        }

        public void SpawnFideBoss(int type)
        {
            try
            {
                if (type == 2)
                {
                    IsFide02Spawn = true;
                    var zone = MapManager.Get(CurrentFideMapId)?.GetZoneById(CurrentFideZoneId);
                    if (zone != null)
                    {
                        fide02 = new Boss();
                        fide02.CreateBoss(DataCache.BOSS_FIDE_02_TYPE);
                        fide02.CharacterHandler.SetUpInfo();
                        zone.ZoneHandler.AddBoss(fide02);
                        fide02Id = fide02.Id;
                        IsFide02Notify = true;
                        IsFide02Spawn = true;
                        ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Fide 02 "+fide02Id+" vừa xuất hiện tại " + zone.Map.TileMap.Name));
                        Server.Gi().Logger.Print("Fide 2...: Map " + zone.Map.TileMap.Name);
                    }
                }
                else 
                {
                    IsFide03Spawn = true;
                    var zone = MapManager.Get(CurrentFideMapId)?.GetZoneById(CurrentFideZoneId);
                    if (zone != null)
                    {
                        fide03 = new Boss();
                        fide03.CreateBoss(DataCache.BOSS_FIDE_03_TYPE);
                        fide03.CharacterHandler.SetUpInfo();
                        zone.ZoneHandler.AddBoss(fide03);
                        fide03Id = fide03.Id;
                        IsFide03Notify = true;
                        IsFide03Spawn = true;
                        ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Fide 03 "+fide03Id+" vừa xuất hiện tại " + zone.Map.TileMap.Name ));
                        Server.Gi().Logger.Print("Fide 3...: Map " + zone.Map.TileMap.Name);
                    }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SpawnFideBoss in BossRunTime.cs: {e.Message} \n {e.StackTrace}", e);
            }
        }

        public void SpawnCoolerBoss(int type)
        {
            try
            {
                if (type == 2)
                {
                    IsCooler02Spawn = true;
                    var zone = MapManager.Get(CurrentCoolerMapId)?.GetZoneById(CurrentCoolerZoneId);
                    if (zone != null)
                    {
                        cooler02 = new Boss();
                        cooler02.CreateBoss(DataCache.BOSS_COOLER_02_TYPE);
                        cooler02.CharacterHandler.SetUpInfo();
                        zone.ZoneHandler.AddBoss(cooler02);
                        cooler02Id = cooler02.Id;
                        IsCooler02Notify = true;
                        IsCooler02Spawn = true;
                        ClientManager.Gi().SendMessageCharacter(Service.ServerChat("BOSS Cooler 02 "+cooler02Id+" vừa xuất hiện tại " + zone.Map.TileMap.Name));
                        Server.Gi().Logger.Print("Cooler 2...: Map " + zone.Map.TileMap.Name);
                    }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error SpawnFideBoss in BossRunTime.cs: {e.Message} \n {e.StackTrace}", e);
            }
        }
    }
}
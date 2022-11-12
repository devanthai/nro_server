using System;
using System.Collections.Generic;
using System.Linq;
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
using NRO_Server.Model.Info;
using NRO_Server.Model.Item;
using NRO_Server.Model.Map;
using NRO_Server.Model.Monster;
using NRO_Server.Model.Effect;
using NRO_Server.Model.Option;
using static System.GC;

namespace NRO_Server.Application.Handlers.Character
{
    public class CharacterHandler : ICharacterHandler
    {
        public Model.Character.Character Character { get; set; }

        public CharacterHandler(Model.Character.Character character)
        {
            Character = character;
        }

        public void SetPlayer(Player player)
        {
            Character.Player = player;
        }
        
        public void SendMessage(Message message)
        {
            Character?.Player?.Session?.SendMessage(message);
        }
        
        public void SendZoneMessage(Message message)
        {
            Character?.Zone?.ZoneHandler?.SendMessage(message);
        }
        
        public void SendMeMessage(Message message)
        {
            Character?.Zone?.ZoneHandler?.SendMessage(message, Character.Id);
        }

        private void UpdateAntiChangeServerTime(string reason = "")
        {
            var timeServer = ServerUtils.CurrentTimeMillis();
            if ((Character.InfoChar.ThoiGianDoiMayChu - timeServer) < 180000)
            {
                Character.InfoChar.ThoiGianDoiMayChu = timeServer + 300000;
            }
            DelayInventoryAction(timeServer, reason);
        }

        private void DelayInventoryAction(long timeServer, string reason)
        {
            if (reason == "Nhặt từ map" || reason == "CSKB" || reason == "CSTT" || reason == "Ăn bánh tt" || reason == "Dùng đá nâng cấp" || reason == "Dùng đá bảo vệ" || reason == "Bán cho shop")
            {
                Character.Delay.SaveInvData = 8000 + timeServer;
            }
            else 
            {
                Character.Delay.InvAction = timeServer + 1000;
            }
        }

        public void Close()
        {
            Character.Zone?.ZoneHandler?.OutZone(Character);
            Character.Me = new InfoFriend(Character)
            {
                IsOnline = false
            };
            if (UserDB.CheckInvalidPortServer(Character.Player.Id) && !Character.Delay.DoiMayChu)
            {
                ServerUtils.WriteLogBug(Character.Name, Character.Player.Id);
            }
            CharacterDB.Update(Character);
            Character?.Disciple?.CharacterHandler.Close();
            Character?.Pet?.CharacterHandler.Close();
            Clear();
        }

        public void UpdateInfo()
        {
            SetUpInfo();
            SendMessage(Service.SendBody(Character));
            SendZoneMessage(Service.UpdateBody(Character));
            SendMessage(Service.MeLoadPoint(Character));
            Character.Me = new InfoFriend(Character);
        }

        public void Update()
        {
            var timeServer = ServerUtils.CurrentTimeMillis();
            
            // 5p save 1 lần
            if (Character.Delay.SaveInvData < timeServer && !Character.Delay.StartLogout && !Character.Delay.IsSavingInventory)
            {
                if (Character.Delay.NeedToSaveBag != false || Character.Delay.NeedToSaveBody != false || Character.Delay.NeedToSaveBox != false || Character.Delay.NeedToSaveLucky != false)
                {
                    Character.Delay.InvAction = timeServer + 15000;
                    Character.Delay.IsSavingInventory = true;

                    if (UserDB.CheckInvalidPortServer(Character.Player.Id))
                    {
                        ServerUtils.WriteLogBug(Character.Name, Character.Player.Id);
                    }
                    
                    if (CharacterDB.SaveInventory(Character, Character.Delay.NeedToSaveBody, Character.Delay.NeedToSaveBox, Character.Delay.NeedToSaveLucky))
                    {
                        Character.Delay.InvAction = timeServer;
                    }
                    else 
                    {
                        SendMessage(Service.DialogMessage("Đã có lỗi xảy ra khi sao lưu tài khoản của bạn, vui lòng thoát game và đăng nhập lại để tránh mất dữ liệu"));
                    }
                    Character.Delay.IsSavingInventory = false;
                }
                // CharacterDB.Update(Character);
                Character.Delay.SaveInvData = 10000 + timeServer;
            }

            lock (Character)
            {
                RemoveSkill(timeServer);
                if (!Character.InfoChar.IsDie)
                {
                    UpdateMask(timeServer);
                    UpdateEffect(timeServer);
                    UpdateFusion(timeServer);
                    UpdateAutoPlay(timeServer);
                }
            }
            // if (Character.Delay.SaveData < timeServer)
            // {
            //     CharacterDB.Update(Character.InfoChar, Character.Id);
            //     Character.Delay.SaveData = 300000 + timeServer;
            //     Console.WriteLine("Update info char: " + Character.Id);
            // }
            
        }

        public void RemoveSkill(long timeServer, bool globalReset = false)
        {
            var infoSkill = Character.InfoSkill;
            if ((infoSkill.TaiTaoNangLuong.IsTTNL &&
                 infoSkill.TaiTaoNangLuong.DelayTTNL <= timeServer) || globalReset)
            {
                SkillHandler.RemoveTTNL(Character);
            }

            if (infoSkill.Monkey.MonkeyId == 1 && infoSkill.Monkey.TimeMonkey <= timeServer || globalReset)
            {
                SkillHandler.HandleMonkey(Character,false);
            }

            if (infoSkill.Protect.IsProtect && infoSkill.Protect.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveProtect(Character);
                if (globalReset && infoSkill.Protect.IsProtect)
                {
                    SendMessage(Service.ItemTime(DataCache.TimeProtect[0], 0));
                }
            }

            if (infoSkill.HuytSao.IsHuytSao && infoSkill.HuytSao.Time <= timeServer)
            {
                SkillHandler.RemoveHuytSao(Character);
            }

            if (infoSkill.MeTroi.IsMeTroi)
            {
                var monsterMap = infoSkill.MeTroi.Monster;
                var charTemp = infoSkill.MeTroi.Character;
                if (globalReset)
                {
                    SkillHandler.RemoveTroi(Character);
                }
                if (monsterMap is {IsDie: true})
                {
                    SkillHandler.RemoveTroi(Character);
                }
                else if (charTemp != null && charTemp.InfoChar.IsDie)
                {
                    SkillHandler.RemoveTroi(Character);
                }
                else if (infoSkill.MeTroi.TimeTroi <= timeServer || monsterMap is {IsDie: true} || charTemp != null && charTemp.InfoChar.IsDie)
                {
                    SkillHandler.RemoveTroi(Character);
                }
            }

            if (infoSkill.PlayerTroi.IsPlayerTroi || globalReset)
            {
                if (globalReset && infoSkill.PlayerTroi.IsPlayerTroi)
                {
                    SendMessage(Service.ItemTime(DataCache.TimeTroi[0], 0));
                    infoSkill.PlayerTroi.PlayerId.ForEach(i => SkillHandler.RemoveTroi(Character.Zone.ZoneHandler.GetCharacter(i)));
                }
                else if(infoSkill.PlayerTroi.IsPlayerTroi && infoSkill.PlayerTroi.TimeTroi <= timeServer && infoSkill.PlayerTroi.PlayerId.Count <= 0)
                {
                    SendMessage(Service.ItemTime(DataCache.TimeTroi[0], 0));
                }
            }

            if (infoSkill.DichChuyen.IsStun && infoSkill.DichChuyen.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveDichChuyen(Character);
            }

            if (infoSkill.ThaiDuongHanSan.IsStun && infoSkill.ThaiDuongHanSan.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveThaiDuongHanSan(Character);
            }

            if (infoSkill.ThoiMien.IsThoiMien && infoSkill.ThoiMien.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveThoiMien(Character);
                if (globalReset && infoSkill.ThoiMien.IsThoiMien)
                {
                    SendMessage(Service.ItemTime(DataCache.TimeThoiMien[0], 0));
                }
            }

            if (infoSkill.Socola.IsSocola && infoSkill.Socola.Time <= timeServer || globalReset)
            {
                SkillHandler.RemoveSocola(Character);
                if (globalReset && infoSkill.Socola.IsSocola)
                {
                    SendMessage(Service.ItemTime(3780, 0));
                }
            }
        }

        public void UpdateAutoPlay(long timeServer)
        {
            if (Character.InfoChar.TimeAutoPlay > 0 && Character.Delay.AutoPlay <= timeServer)
            {
                if (Character.InfoChar.TimeAutoPlay < timeServer || Character.InfoChar.TimeAutoPlay < 0)
                {
                    Character.InfoChar.TimeAutoPlay = 0;
                    SendMessage(Service.ItemTime(4387, 0));
                    SendMessage(Service.AutoPlay(false));
                }
                Character.Delay.AutoPlay = 60000 + timeServer;
            }
        }

        public void UpdateEffect(long timeServer)
        {
            var effect = Character.Effect;

            // Vệ tinh trí lực
            if (Character.InfoMore.IsNearAuraTriLucItem && effect.AuraBuffKi30S.Time <= timeServer && Character.InfoChar.Mp < Character.MpFull)
            {
                PlusMp((int)(Character.MpFull*5/100));
                SendMessage(Service.SendMp((int)Character.InfoChar.Mp));
                effect.AuraBuffKi30S.Time = 30000 + timeServer;
                Character.InfoMore.IsNearAuraTriLucItem = false;
            }

            if (Character.InfoMore.IsNearAuraSinhLucItem && effect.AuraBuffHp30S.Time <= timeServer && Character.InfoChar.Hp < Character.HpFull)
            {
                PlusHp((int)(Character.HpFull*5/100));
                SendMessage(Service.SendHp((int)Character.InfoChar.Hp));
                SendZoneMessage(Service.PlayerLevel(Character));
                effect.AuraBuffHp30S.Time = 30000 + timeServer;
                Character.InfoMore.IsNearAuraSinhLucItem = false;
            }


            if (effect.BuffHp30S.Value > 0 && effect.BuffHp30S.Time <= timeServer && Character.InfoChar.Hp < Character.HpFull)
            {
                PlusHp(effect.BuffHp30S.Value);
                SendMessage(Service.SendHp((int)Character.InfoChar.Hp));
                SendZoneMessage(Service.PlayerLevel(Character));
                effect.BuffHp30S.Time = 30000 + timeServer;
            }

            if (effect.BuffKi1s.Value > 0 && effect.BuffKi1s.Time <= timeServer && Character.InfoChar.Mp < Character.MpFull)
            {
                PlusMp(effect.BuffKi1s.Value);
                SendMessage(Service.SendMp((int)Character.InfoChar.Mp));
                effect.BuffKi1s.Time = 1500 + timeServer;
            }

            // Effect giáp luyện tập
            // Nếu vừa tháo giáp luyện tập ra thì sẽ trừ thời gian
            if (Character.InfoMore.LastGiapLuyenTapItemId != 0 && Character.Delay.GiapLuyenTap != -1 && Character.Delay.GiapLuyenTap < timeServer)
            {
                var giapLuyenTap = GetItemBagById(Character.InfoMore.LastGiapLuyenTapItemId);
                if (giapLuyenTap != null && ItemCache.ItemIsGiapLuyenTap(giapLuyenTap.Id)) 
                {
                    var optionCheck = giapLuyenTap.Options.FirstOrDefault(option => option.Id == 9);
                    if ((optionCheck.Param - 1) > 0)
                    {
                        optionCheck.Param -= 1;
                        SendMessage(Service.SendBag(Character));
                        Character.Delay.GiapLuyenTap = 60000 + timeServer;
                    }
                    else 
                    {
                        optionCheck.Param = 0;
                        Character.InfoMore.LastGiapLuyenTapItemId = 0;
                        SendMessage(Service.SendBag(Character));
                        UpdateInfo();
                        Character.Delay.GiapLuyenTap = -1;
                    }
                } 
                else 
                {
                    Character.InfoMore.LastGiapLuyenTapItemId = 0;
                    Character.Delay.GiapLuyenTap = -1;
                }
            }

            bool IsRemoveBuffEffect = false;
            // Effect thức ăn
            if (Character.InfoBuff.BanhTrungThuTime < timeServer && Character.InfoBuff.BanhTrungThuId > -1)
            {
                Character.InfoBuff.BanhTrungThuId = -1;
                IsRemoveBuffEffect = true;
            }
            // Effect thức ăn
            if (Character.InfoBuff.ThucAnTime < timeServer && Character.InfoBuff.ThucAnId > -1)
            {
                Character.InfoBuff.ThucAnId = -1;
                IsRemoveBuffEffect = true;
            }
            // Effect cuồng nộ
            if (Character.InfoBuff.CuongNoTime < timeServer && Character.InfoBuff.CuongNo)
            {
                Character.InfoBuff.CuongNo = false;
                IsRemoveBuffEffect = true;
            }
            // Effect Bổ huyết
            if (Character.InfoBuff.BoHuyetTime < timeServer && Character.InfoBuff.BoHuyet)
            {
                Character.InfoBuff.BoHuyet = false;
                IsRemoveBuffEffect = true;
            }
            // Effect Bo Khi
            if (Character.InfoBuff.BoKhiTime < timeServer && Character.InfoBuff.BoKhi)
            {
                Character.InfoBuff.BoKhi = false;
                IsRemoveBuffEffect = true;
            }
            // Effect giap xen
            if (Character.InfoBuff.GiapXenTime < timeServer && Character.InfoBuff.GiapXen)
            {
                Character.InfoBuff.GiapXen = false;
                IsRemoveBuffEffect = true;
            }
            // Effect An danh
            if (Character.InfoBuff.AnDanhTime < timeServer && Character.InfoBuff.AnDanh)
            {
                Character.InfoBuff.AnDanh = false;
                IsRemoveBuffEffect = true;
            }
            // Effect Do CSKB
            if (Character.InfoBuff.MayDoCSKBTime < timeServer && Character.InfoBuff.MayDoCSKB)
            {
                Character.InfoBuff.MayDoCSKB = false;
            }

            if (IsRemoveBuffEffect)
            {
                SetUpInfo();
                SendMessage(Service.MeLoadPoint(Character));
            }
            // effect Beautiful
            if (Character.ItemBody[5] != null && Character.ItemBody[5].Id == 464 && Character.Delay.BeautifulTalk < timeServer)
            {
                // get khoản cách ở gần 50 pixel r nói đẹp quá.
                Character.Zone.Characters.Values.Where(c => !c.InfoChar.IsDie && c.Id != Character.Id && Math.Abs(c.InfoChar.X - Character.InfoChar.X) <= 50 && Math.Abs(c.InfoChar.Y - Character.InfoChar.Y) <= 600).ToList().ForEach(temp =>
                {
                    temp.CharacterHandler.SendZoneMessage(Service.PublicChat(temp.Id, TextServer.gI().CHAT_SEXY[ServerUtils.RandomNumber(TextServer.gI().CHAT_SEXY.Count)]));
                });
                Character.Delay.BeautifulTalk = timeServer + 20000;
            }
        }

        public void UpdateMask(long timeServer)
        {
            var item = Character.ItemBody[5];
            if(item == null) return;
            /*switch (item.Id)
            {
                //TODO handle logic for mask
            }*/
        }

        public void UpdateLuyenTap()
        {
            var timeServer = ServerUtils.CurrentTimeMillis();
            if (Character.Delay.TrainGiapLuyenTap > timeServer) return;
            var item = Character.ItemBody[6];
            if(item == null || !ItemCache.ItemIsGiapLuyenTap(item.Id)) return;
            var optionCheck = item.Options.FirstOrDefault(option => option.Id == 9);
            if ((optionCheck.Param + 1) <= ItemCache.GetGiapLuyenTapLimit(item.Id))
            {
                optionCheck.Param += 1;
                SendMessage(Service.SendBag(Character));
            }
            Character.Delay.TrainGiapLuyenTap = 60000 + timeServer;
        }

        private void UpdateFusion(long timeServer)
        {
            var fusion = Character.InfoChar.Fusion;
            var disciple = Character.Disciple;
            if (disciple is {Status: 4} && fusion.IsFusion)
            {
                // Update đệ đang hợp thể
                // if (disciple.InfoSkill.HuytSao.IsHuytSao && disciple.InfoSkill.HuytSao.Time <= timeServer)
                // {
                //     disciple.InfoSkill.HuytSao.IsHuytSao = false;
                //     disciple.InfoSkill.HuytSao.Time = -1;
                //     disciple.InfoSkill.HuytSao.Percent = 0;
                //     disciple.CharacterHandler.SetHpFull();

                //     if (disciple.InfoChar.Hp >= disciple.HpFull)
                //     {
                //         disciple.InfoChar.Hp = disciple.HpFull;
                //     }

                //     SetHpFull();
                //     if (Character.InfoChar.Hp >= Character.HpFull)
                //     {
                //         Character.InfoChar.Hp = Character.HpFull;
                //     }
                //     SendMessage(Service.MeLoadPoint(Character));
                //     SendZoneMessage(Service.PlayerLevel(Character));
                // }

                if (disciple.InfoSkill.Monkey.MonkeyId == 1 && disciple.InfoSkill.Monkey.TimeMonkey <= timeServer)
                {
                    // reset lại máu sư phụ
                    disciple.InfoSkill.Monkey.MonkeyId = 0;
                    disciple.InfoSkill.Monkey.HeadMonkey = -1;
                    disciple.InfoSkill.Monkey.BodyMonkey = -1;
                    disciple.InfoSkill.Monkey.LegMonkey = -1;
                    disciple.InfoSkill.Monkey.TimeMonkey = -1;
                    disciple.CharacterHandler.SetUpInfo();
                    SetUpInfo();
                    if (Character.InfoChar.Hp >= Character.HpFull)
                    {
                        Character.InfoChar.Hp = Character.HpFull;
                    }
                    SendMessage(Service.MeLoadPoint(Character));
                    SendZoneMessage(Service.PlayerLevel(Character));
                }

                if (timeServer >= fusion.TimeStart + fusion.TimeUse && (!fusion.IsPorata && !fusion.IsPorata2))
                {
                    disciple.CharacterHandler.SetUpPosition(isRandom: true);
                    Character.Zone.ZoneHandler.AddDisciple(disciple);
                    SendZoneMessage(Service.Fusion(Character.Id, 1));
                    lock (Character.InfoChar.Fusion)
                    {
                        Character.InfoChar.Fusion.IsFusion = false;
                        Character.InfoChar.Fusion.IsPorata = false;
                        Character.InfoChar.Fusion.IsPorata2 = false;
                        Character.InfoChar.Fusion.TimeStart = timeServer;
                        Character.InfoChar.Fusion.DelayFusion = timeServer + 600000;
                        Character.InfoChar.Fusion.TimeUse = 0;
                    }
                    
                    disciple.Status = 0;
                    SetUpInfo();
                    SendZoneMessage(Service.UpdateBody(Character));
                    SendMessage(Service.SendBody(Character));
                    SendMessage(Service.PlayerLoadSpeed(Character));
                    SendMessage(Service.MeLoadPoint(Character));
                    SendMessage(Service.SendHp((int)Character.InfoChar.Hp));
                    SendMessage(Service.SendMp((int)Character.InfoChar.Mp));
                    SendZoneMessage(Service.PlayerLevel(Character));
                }
            }
        }

        public void SetUpPosition(int mapOld, int mapNew, bool isRandom = false)
        {
            PositionHandler.SetUpPosition(Character, mapOld, mapNew);
        }

        private void CheckExpireItem(Model.Item.Item item, int timeServer, int type)
        {
            try
            {
                // Kiểm tra xem có option HSD ngày hoặc giây không
                var expireOption = item.Options.FirstOrDefault(option => option.Id == 93);
                if (expireOption != null) 
                {
                    // Kiểm tra hạn sử dụng
                    var expireTimeOption = item.Options.FirstOrDefault(option => (option.Id == 73));
                    if (expireTimeOption != null) 
                    {
                        if (expireTimeOption.Param < timeServer)
                        {
                            switch(type)
                            {
                                case 0://body
                                {
                                    RemoveItemBody(item.IndexUI);
                                    break;
                                }
                                case 1:
                                {
                                    RemoveItemBagByIndex(item.IndexUI, item.Quantity, false, reason:"Item hết hạn sử dụng");
                                    break;
                                }
                                case 2:
                                {
                                    RemoveItemBoxByIndex(item.IndexUI, item.Quantity, false);
                                    break;
                                }
                                case 3:
                                {
                                    RemoveItemLuckyBox(item.IndexUI, false);
                                    break;
                                }
                            }
                        }
                        else 
                        {
                            // tính ngày từ giây
                            var leftTime = expireTimeOption.Param - timeServer;
                            expireOption.Param = ServerUtils.ConvertSecondToDay(leftTime);
                        }
                    }
                }
                else if(type == 1 || type == 2)
                {
                    // Console.WriteLine("bag box: " + item.Id);
                    var expireOption2 = item.Options.FirstOrDefault(option => option.Id == 211);
                    if (expireOption2 != null)
                    {
                        var expireTimeOption = item.Options.FirstOrDefault(option => (option.Id == 73));
                        if (expireTimeOption != null) 
                        {
                            if (expireTimeOption.Param < timeServer)
                            {
                                item.Options.Remove(expireOption2);
                                item.Options.Remove(expireTimeOption);
                            }
                            else 
                            {
                                var leftTime = expireTimeOption.Param - timeServer;
                                expireOption2.Param = (leftTime/3600);
                                Console.WriteLine("expireOption2.Param: " + expireOption2.Param + " giay: " + leftTime);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error CheckExpireItem in CharacterHandler.cs: {e.Message} \n {e.StackTrace}", e);
            }
        }

        public void SendInfo()
        {
            //Check hạn sử dụng item
            // Body Item
            var timeServer = ServerUtils.CurrentTimeSecond();
            var soLuongThoiVangTrenNguoi = 0;
            var soLuongThoiVangTrongRuong = 0;
            Character.ItemBody.Where(item => item != null).ToList().ForEach(item =>
            {
                CheckExpireItem(item, timeServer, 0);
            });
            // Bag item
            Character.ItemBag.Where(item => item != null).ToList().ForEach(item =>
            {
                CheckExpireItem(item, timeServer, 1);
                if (item.Id == 457)
                {
                    soLuongThoiVangTrenNguoi += item.Quantity;
                }
            });
            // Box Item
            Character.ItemBox.Where(item => item != null).ToList().ForEach(item =>
            {
                CheckExpireItem(item, timeServer, 2);
                if (item.Id == 457)
                {
                    soLuongThoiVangTrongRuong += item.Quantity;
                }
            });

            if (soLuongThoiVangTrongRuong + soLuongThoiVangTrenNguoi > 800)
            {
                ServerUtils.WriteLog("thoivang/" + Character.Id, $"Char: {Character.Name}, tên tài khoản {Character.Player.Username} (ID:{Character.Player.Id}), Thỏi vàng trên người {soLuongThoiVangTrenNguoi}, thỏi vàng trong rương {soLuongThoiVangTrongRuong}");
            }
            // LuckyBox Item
            Character.LuckyBox.Where(item => item != null).ToList().ForEach(item =>
            {
                CheckExpireItem(item, timeServer, 3);
            });

            try
            {
                Character.ItemBody.Where(item => item != null).ToList().ForEach(item =>
                {
                    if (item.Id == 561)
                    {
                        if (item.GetParamOption(72) == 4 && item.GetParamOption(14) > 22)
                        {
                            var option = item.Options.FirstOrDefault(op => op.Id == 14);
                            if (option != null) 
                            {
                                option.Param = 22;
                            }
                        }
                        else if (item.GetParamOption(72) == 5 && item.GetParamOption(14) > 24)
                        {
                            var option = item.Options.FirstOrDefault(op => op.Id == 14);
                            if (option != null) 
                            {
                                option.Param = 24;
                            }
                        }
                        else if (item.GetParamOption(72) == 6 && item.GetParamOption(14) > 27)
                        {
                            var option = item.Options.FirstOrDefault(op => op.Id == 14);
                            if (option != null) 
                            {
                                option.Param = 27;
                            }
                        }
                        else if (item.GetParamOption(72) == 7 && item.GetParamOption(14) > 30)
                        {
                            var option = item.Options.FirstOrDefault(op => op.Id == 14);
                            if (option != null) 
                            {
                                option.Param = 30;
                            }
                        }
                        else if (item.GetParamOption(72) == 8 && item.GetParamOption(14) > 33)
                        {
                            var option = item.Options.FirstOrDefault(op => op.Id == 14);
                            if (option != null) 
                            {
                                option.Param = 33;
                            }
                        }
                    }
                });

                // Bag item
                Character.ItemBag.Where(item => item != null).ToList().ForEach(item =>
                {
                    if (item.Id == 561)
                    {
                        if (item.GetParamOption(72) == 4 && item.GetParamOption(14) > 22)
                        {
                            var option = item.Options.FirstOrDefault(op => op.Id == 14);
                            if (option != null) 
                            {
                                option.Param = 22;
                            }
                        }
                        else if (item.GetParamOption(72) == 5 && item.GetParamOption(14) > 24)
                        {
                            var option = item.Options.FirstOrDefault(op => op.Id == 14);
                            if (option != null) 
                            {
                                option.Param = 24;
                            }
                        }
                        else if (item.GetParamOption(72) == 6 && item.GetParamOption(14) > 27)
                        {
                            var option = item.Options.FirstOrDefault(op => op.Id == 14);
                            if (option != null) 
                            {
                                option.Param = 27;
                            }
                        }
                        else if (item.GetParamOption(72) == 7 && item.GetParamOption(14) > 30)
                        {
                            var option = item.Options.FirstOrDefault(op => op.Id == 14);
                            if (option != null) 
                            {
                                option.Param = 30;
                            }
                        }
                        else if (item.GetParamOption(72) == 8 && item.GetParamOption(14) > 33)
                        {
                            var option = item.Options.FirstOrDefault(op => op.Id == 14);
                            if (option != null) 
                            {
                                option.Param = 33;
                            }
                        }
                    }
                });
                
            }
            catch (Exception)
            {
                
            }
            
            // Kiểm tra vàng nhiều
            //Check đệ tử
            if (Character.InfoChar.IsHavePet)
            {
                Character.Disciple = DiscipleDB.GetById(-Character.Id);
                if (Character.Disciple != null)
                {
                    Character.Disciple.Character = Character;
                    Character.Disciple.Player = Character.Player;
                    Character.Disciple.CharacterHandler.SetUpPosition(isRandom:true);
                }
                else
                {
                    Character.InfoChar.IsHavePet = false;
                    Character.InfoChar.Fusion.Reset();
                }
            }

            if (Character.InfoChar.PetId != -1)
            {
                var PetItemIndex = Character.ItemBag.FirstOrDefault(item => item.Id == Character.InfoChar.PetId && item.GetParamOption(73) == Character.InfoChar.PetImei);

                if (PetItemIndex != null)
                {
                    var pet = new Pet(PetItemIndex.Id, Character);
                    Character.Pet = pet;
                    Character.Pet.CharacterHandler.SetUpPosition(isRandom:true);
                    Character.InfoMore.PetItemIndex = PetItemIndex.IndexUI;
                } 
                else 
                {//Không tìm thấy pet
                    Character.InfoChar.PetId = -1;
                    Character.InfoChar.PetImei = -1;
                    Character.InfoMore.PetItemIndex = -1;
                }
            
            }

            if (Character.ClanId == -1)
            {
                Character.InfoChar.Bag = -1;
            }
            else
            {
                var clan = ClanManager.Get(Character.ClanId);
                if (clan?.ClanHandler.GetMember(Character.Id) != null)
                {
                    Character.InfoChar.Bag = (sbyte)clan.ImgId;
                }
                else
                {
                    Character.ClanId = -1;
                    Character.InfoChar.Bag = -1;
                }
            }

            if (Character.InfoChar.OSkill.Count == 0)
            {
                Character.InfoChar.OSkill = new List<sbyte>() {-1, -1, -1, -1, -1};
            }

            if (Character.InfoChar.KSkill.Count == 0)
            {
                Character.InfoChar.KSkill = new List<sbyte>() {-1, -1, -1, -1, -1};
            }
            
            var maxPower = Cache.Gi().LIMIT_POWERS[DataCache.MAX_LIMIT_POWER_LEVEL-1].Power;
            if (Character.InfoChar.Power > maxPower)
            {
                Character.InfoChar.Power = maxPower;
            }

            Character.InfoChar.Level = (sbyte)(Cache.Gi().EXPS.Count(exp => exp < Character.InfoChar.Power) - 1);
            Character.InfoChar.LitmitPower = DataCache.DEFAULT_LIMIT_POWER_LEVEL;

            if (maxPower > Character.InfoChar.Power)
            {
                Character.InfoChar.IsPower = true;
            }
            else 
            {
                Character.InfoChar.IsPower = false;
            }
            
            SetUpInfo(); 
            if(Character.InfoChar.PhukienPart == -1) SendMessage(Service.SendImageBag(Character.Id, Character.InfoChar.Bag));
            SendZoneMessage(Service.PlayerLoadAll(Character));
            SendMessage(Service.SendBox(Character));
            SendMessage(Service.SendBag(Character));
            SendMessage(Service.SendBody(Character));
            SendMessage(Service.MeLoadPoint(Character));
            SendMessage(Service.SendTask(Character));
            SendMessage(Service.SpeacialSkill(Character, 0));
            SendMessage(Service.MyClanInfo(Character));
            SendMessage(Service.MeLoadAll(Character));
            SendMessage(Service.SendStamina(Character.InfoChar.Stamina));
            SendMessage(Service.SendMaxStamina(Character.InfoChar.MaxStamina));
            SendMessage(Service.SendNangDong(Character.InfoChar.NangDong));
            SendMessage(Service.GameInfo());
            SendMessage(Service.UpdateCooldown(Character));
            SendMessage(Service.ChangeOnSkill(Character.InfoChar.OSkill));
            SendZoneMessage(Service.UpdateBody(Character));

            if (Character.InfoChar.IsHavePet && Character.Disciple != null)
            {
                SendMessage(Service.Disciple(1, null));  
            }
            else
            {
                SendMessage(Service.Disciple(0, null)); 
            }
            UpdateMountId();
            UpdatePhukien();
            Character.UpdateOldMap();
            // Setup Bùa
            Character.SetupAmulet();

            // setup thời gian sáng tối
            // if (DatabaseManager.Manager.gI().SuKienTrungThu)
            // {
            //     SendMessage(Service.SetNight(Character));
            // }
        }

        public void SendDie()
        {
            lock (Character)
            {
                ClearTest();
                RemoveSkill(ServerUtils.CurrentTimeMillis(), true);
                Character.InfoChar.IsDie = true;
                Character.InfoSkill.Monkey.MonkeyId = 0;
                SetUpInfo();
                SendMessage(Service.PlayerLoadSpeed(Character));
                // SendZoneMessage(Service.UpdateBody(Character));
                SendMessage(Service.MeLoadPoint(Character));
                SendMessage(Service.MeLoadInfo(Character));
                SendMessage(Service.MeDie(Character, 0));
                SendZoneMessage(Service.PlayerDie(Character));
                LeaveGold();
                if (!Character.Trade.IsTrade) return;
                var charTemp = (Model.Character.Character) Character.Zone.ZoneHandler.GetCharacter(Character.Trade.CharacterId);
                if (charTemp != null && charTemp.Trade.CharacterId == Character.Id)
                {
                    charTemp.CloseTrade(true);
                    charTemp.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().CLOSE_TRADE));
                }
                Character.CloseTrade(true);
            }
        }

        public void ClearTest()
        {
            if (Character.Test != null)
            {
                Character.Test.IsTest = false;
                Character.Test.TestCharacterId = -1;
                Character.Test.GoldTest = 0;
            }
            Character.InfoChar.TypePk = 0;
            SendZoneMessage(Service.ChangeTypePk(Character.Id, 0));
        }

        public void RemoveTroi(int charId)
        {
            var infoSkill = Character.InfoSkill.PlayerTroi;
            if (infoSkill.IsPlayerTroi)
            {
                infoSkill.PlayerId.RemoveAll(i => i == charId);
                if (infoSkill.PlayerId.Count <= 0)
                {
                    infoSkill.IsPlayerTroi = false;
                    infoSkill.TimeTroi = -1;
                    infoSkill.PlayerId.Clear();
                    SendZoneMessage(Service.SkillEffectPlayer(charId, Character.Id, 2, 32));
                }
            }
        }

        private void LeaveGold()
        {
            var quantity = Character.InfoChar.Gold switch
            {
                > 1000 and <= 500000 => Character.InfoChar.Gold / 30,
                > 500000 and <= 1000000 => ServerUtils.RandomNumber(10000, 20000),
                > 1000000 and <= 100000000 => ServerUtils.RandomNumber(20000, 30000),
                > 100000000 => ServerUtils.RandomNumber(30000, 50000),
                _ => 1
            };
            var itemMap = LeaveItemHandler.LeaveGoldPlayer(Character.Id, (int)quantity);
            itemMap.X = Character.InfoChar.X;
            itemMap.Y = Character.InfoChar.Y;
            Character.Zone.ZoneHandler.LeaveItemMap(itemMap);
        }

        public void UpdateMountId()
        {
            var itemBag = Character.ItemBag.FirstOrDefault(item => DataCache.IdMount.Contains(item.Id));
            if (itemBag != null)
            {
                var id = itemBag.Id;
                id = id switch
                {
                    733 => 30001,
                    734 => 30002,
                    735 => 30003,
                    743 => 30004,
                    744 => 30005,
                    746 => 30006,
                    795 => 30007,
                    849 => 30008,
                    897 => 30009,
                    920 => 30010,
                    _ => id
                };
                Character.InfoChar.MountId = id;
            }
            else
            {
                Character.InfoChar.MountId = -1;
            }
        }
        public void UpdatePhukien()
        {
            var itemBag = Character.ItemBag.FirstOrDefault(item => ItemCache.ItemTemplate(item.Id).Type == 11);
            if (itemBag != null)
            {
                var itemTemplate = ItemCache.ItemTemplate(itemBag.Id);
                Character.InfoChar.PhukienPart = itemTemplate.Part;
                Character.CharacterHandler.SendZoneMessage(
                    Service.SendImageBag(Character.Id, itemTemplate.Part));
            }
            else
            {
                Character.InfoChar.PhukienPart = -1;
                Character.CharacterHandler.SendZoneMessage(Character.ClanId == -1
                    ? Service.SendImageBag(Character.Id, -1)
                    : Service.SendImageBag(Character.Id, Character.InfoChar.Bag));
            }
            SetUpInfo();
            Character.CharacterHandler.SetUpInfo();
            Character.CharacterHandler.SendMessage(Service.MeLoadPoint(Character));
            Character.CharacterHandler.SendMessage(Service.SendHp((int)Character.InfoChar.Hp));
            Character.CharacterHandler.SendMessage(Service.SendMp((int)Character.InfoChar.Mp));
            Character.CharacterHandler.SendZoneMessage(Service.PlayerLevel(Character));
        }

        // public void UpdatePet()
        // {
        //     if (Character.InfoChar.PetId != -1)
        //     {
        //         OptionItem optionParam;
        //         var PetItemIndex = Character.ItemBag.FirstOrDefault(item => item.Id == Character.InfoChar.PetId && (optionParam = item.Options.FirstOrDefault(petOp => petOp.Id == 73)) != null && optionParam.Param == Character.InfoChar.PetImei);
        //         if (PetItemIndex != null)
        //         {
        //             Character.InfoMore.PetItemIndex = PetItemIndex.IndexUI;
        //         }
        //         else 
        //         {
        //             Character.InfoChar.PetId = -1;
        //             Character.InfoChar.PetImei = -1;
        //         }
        //     }
        //     else 
        //     {
        //         Character.InfoMore.PetItemIndex = -1;
        //     }
        // }

        public int GetParamItem(int id)
        {
            var param = 0;
            Character.ItemBody.Where(item => item != null).ToList().ForEach(item =>
            {
                var option = item.Options.Where(option => option.Id == id).ToList();
                param += option.Sum(optionItem => optionItem.Param);
            });
            Character.InfoChar.Cards.Values.Where(r => r.Used == 1).ToList().ForEach(r =>
            {
                foreach (var optionRadar in r.Options.Where(optionRadar => optionRadar.Id == id))
                {
                    if (optionRadar.ActiveCard == r.Level)
                    {
                        param += optionRadar.Param;
                    }
                    else if (r.Level == -1 && optionRadar.ActiveCard == 0)
                    {
                        param += optionRadar.Param;
                    }
                }
            });
            var itemBag = Character.ItemBag.FirstOrDefault(item => ItemCache.ItemTemplate(item.Id).Type == 11);
            if (itemBag != null) param += itemBag.GetParamOption(id);

            if (Character.InfoMore.PetItemIndex != -1)
            {
                var petItem = Character.ItemBag.FirstOrDefault(item => item.IndexUI == Character.InfoMore.PetItemIndex);
                if (petItem != null) param += petItem.GetParamOption(id);
            }
            return param;
        }

        public List<int> GetListParamItem(int id)
        {
            var param = new List<int>();
            Character.ItemBody.Where(item => item != null).ToList().ForEach(item =>
            {
                var option = item.Options.Where(option => option.Id == id).ToList();
                param.AddRange(option.Select(optionItem => optionItem.Param));
            });
            Character.InfoChar.Cards.Values.Where(r => r.Used == 1).ToList().ForEach(r =>
            {
                foreach (var optionRadar in r.Options.Where(optionRadar => optionRadar.Id == id))
                {
                    if (optionRadar.ActiveCard == r.Level)
                    {
                        param.Add(optionRadar.Param);
                    }
                    else if (r.Level == -1 && optionRadar.ActiveCard == 0)
                    {
                        param.Add(optionRadar.Param);
                    }
                }
            });
            
            var itemBag = Character.ItemBag.FirstOrDefault(item => ItemCache.ItemTemplate(item.Id).Type == 11);
            if (itemBag != null) param.Add(itemBag.GetParamOption(id));

            if (Character.InfoMore.PetItemIndex != -1)
            {
                var petItem = Character.ItemBag.FirstOrDefault(item => item.IndexUI == Character.InfoMore.PetItemIndex);
                if (petItem != null) param.Add(petItem.GetParamOption(id));
            }
            return param;
        }

        public void SetUpFriend()
        {
            Character.Me = new InfoFriend(Character);
            Character.Friends.ForEach(friend =>
            {
                var charCheck = (Model.Character.Character)ClientManager.Gi().GetCharacter(friend.Id);
                friend = charCheck != null ? new InfoFriend(charCheck) : CharacterDB.GetInfoCharacter(friend.Id);
            });
        }

        public void SetUpInfo()
        {
            SetupPetIndex();
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
            SetTuDongLuyenTap();
            SetEnhancedOption();            
            SetInfoBuff();
        }

        private void SetupPetIndex()
        {
            if (Character.InfoChar.PetId != -1)
            {
                var PetItemIndex = Character.ItemBag.FirstOrDefault(item => item.Id == Character.InfoChar.PetId && item.GetParamOption(73) == Character.InfoChar.PetImei);
                if (PetItemIndex != null)
                {
                    Character.InfoMore.PetItemIndex = PetItemIndex.IndexUI;
                }
                else 
                {
                    Character.InfoMore.PetItemIndex = -1;
                }
            }
            else 
            {
                Character.InfoMore.PetItemIndex = -1;
            }
        }

        public void SetInfoSet()
        {
            Character.InfoSet.Reset();

            Character.InfoSet.IsFullSetThanLinh = true;
            for (int i = 0; i < 5; i++)
            {
                if (Character.ItemBody[i] == null || Character.ItemBody[i].Id > 567 || Character.ItemBody[i].Id < 555)
                {
                    Character.InfoSet.IsFullSetThanLinh = false;
                    break;
                }
            }

            switch (Character.InfoChar.Gender)
            {
                case 0:
                {
                    if (Character.ItemBody[0] != null)
                    {
                        // IsFullSetKirin = false;
                        // IsFullSetSongoku = false;
                        // IsFullSetThienXingHang = false;
                        var getSetTXH = Character.ItemBody[0].Options.FirstOrDefault(option => option.Id == 127);

                        if (getSetTXH != null)
                        {
                            Character.InfoSet.IsFullSetThienXinHang = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Character.ItemBody[i] == null || Character.ItemBody[i].Options.FirstOrDefault(option => option.Id == 127) == null)
                                {
                                    Character.InfoSet.IsFullSetThienXinHang = false;
                                    break;
                                }
                            }
                            return;
                        }

                        var getSetKirin = Character.ItemBody[0].Options.FirstOrDefault(option => option.Id == 128);

                        if (getSetKirin != null)
                        {
                            Character.InfoSet.IsFullSetKirin = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Character.ItemBody[i] == null || Character.ItemBody[i].Options.FirstOrDefault(option => option.Id == 128) == null)
                                {
                                    Character.InfoSet.IsFullSetKirin = false;
                                    break;
                                }
                            }
                            return;
                        }

                        var getSetSGK = Character.ItemBody[0].Options.FirstOrDefault(option => option.Id == 129);

                        if (getSetSGK != null)
                        {
                            Character.InfoSet.IsFullSetSongoku = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Character.ItemBody[i] == null || Character.ItemBody[i].Options.FirstOrDefault(option => option.Id == 129) == null)
                                {
                                    Character.InfoSet.IsFullSetSongoku = false;
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
                    if (Character.ItemBody[0] != null)
                    {
                        // IsFullSetKirin = false;
                        // IsFullSetSongoku = false;
                        // IsFullSetThienXingHang = false;
                        var getSetPicolo = Character.ItemBody[0].Options.FirstOrDefault(option => option.Id == 130);

                        if (getSetPicolo != null)
                        {
                            Character.InfoSet.IsFullSetPicolo = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Character.ItemBody[i] == null || Character.ItemBody[i].Options.FirstOrDefault(option => option.Id == 130) == null)
                                {
                                    Character.InfoSet.IsFullSetPicolo = false;
                                    break;
                                }
                            }
                            return;
                        }

                        var getSetOcTieu = Character.ItemBody[0].Options.FirstOrDefault(option => option.Id == 131);

                        if (getSetOcTieu != null)
                        {
                            Character.InfoSet.IsFullSetOcTieu = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Character.ItemBody[i] == null || Character.ItemBody[i].Options.FirstOrDefault(option => option.Id == 131) == null)
                                {
                                    Character.InfoSet.IsFullSetOcTieu = false;
                                    break;
                                }
                            }
                            return;
                        }

                        var getSetPikkoro = Character.ItemBody[0].Options.FirstOrDefault(option => option.Id == 132);

                        if (getSetPikkoro != null)
                        {
                            Character.InfoSet.IsFullSetPikkoro = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Character.ItemBody[i] == null || Character.ItemBody[i].Options.FirstOrDefault(option => option.Id == 132) == null)
                                {
                                    Character.InfoSet.IsFullSetPikkoro = false;
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
                    if (Character.ItemBody[0] != null)
                    {
                        // IsFullSetKirin = false;
                        // IsFullSetSongoku = false;
                        // IsFullSetThienXingHang = false;
                        var getSetKakarot = Character.ItemBody[0].Options.FirstOrDefault(option => option.Id == 133);

                        if (getSetKakarot != null)
                        {
                            Character.InfoSet.IsFullSetKakarot = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Character.ItemBody[i] == null || Character.ItemBody[i].Options.FirstOrDefault(option => option.Id == 133) == null)
                                {
                                    Character.InfoSet.IsFullSetKakarot = false;
                                    break;
                                }
                            }
                            return;
                        }

                        var getSetCadic = Character.ItemBody[0].Options.FirstOrDefault(option => option.Id == 134);

                        if (getSetCadic != null)
                        {
                            Character.InfoSet.IsFullSetCadic = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Character.ItemBody[i] == null || Character.ItemBody[i].Options.FirstOrDefault(option => option.Id == 134) == null)
                                {
                                    Character.InfoSet.IsFullSetCadic = false;
                                    break;
                                }
                            }
                            return;
                        }

                        var getSetNappa = Character.ItemBody[0].Options.FirstOrDefault(option => option.Id == 135);

                        if (getSetNappa != null)
                        {
                            Character.InfoSet.IsFullSetNappa = true;
                            for (int i = 1; i < 5; i++)
                            {
                                if (Character.ItemBody[i] == null || Character.ItemBody[i].Options.FirstOrDefault(option => option.Id == 135) == null)
                                {
                                    Character.InfoSet.IsFullSetNappa = false;
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

        public void SetInfoBuff()
        {
            var timeServer = ServerUtils.CurrentTimeMillis();
            // thuc an
            // Kiểm tra thời gian thức ăn và hiện item time
            // Effect thức ăn
            if (Character.InfoBuff.ThucAnTime > timeServer && Character.InfoBuff.ThucAnId != -1)
            {
                var giayConLai = (Character.InfoBuff.ThucAnTime - timeServer)/1000;
                if (giayConLai <= 0) giayConLai = 0;
                var template = ItemCache.ItemTemplate(Character.InfoBuff.ThucAnId);
                SendMessage(Service.ItemTime(template.IconId, (int)giayConLai));
            }
            // Effect banh trung thu
            if (Character.InfoBuff.BanhTrungThuTime > timeServer && Character.InfoBuff.BanhTrungThuId != -1)
            {
                var giayConLai = (Character.InfoBuff.BanhTrungThuTime - timeServer)/1000;
                if (giayConLai <= 0) giayConLai = 0;
                var template = ItemCache.ItemTemplate(Character.InfoBuff.BanhTrungThuId);
                SendMessage(Service.ItemTime(template.IconId, (int)giayConLai));
            }
            // Effect cuồng nộ
            if (Character.InfoBuff.CuongNoTime > timeServer)
            {
                var giayConLai = (Character.InfoBuff.CuongNoTime - timeServer)/1000;
                if (giayConLai <= 0) giayConLai = 0;
                SendMessage(Service.ItemTime(2754, (int)giayConLai));
            }
            // Effect Bổ huyết
            if (Character.InfoBuff.BoHuyetTime > timeServer)
            {
                var giayConLai = (Character.InfoBuff.BoHuyetTime - timeServer)/1000;
                if (giayConLai <= 0) giayConLai = 0;
                SendMessage(Service.ItemTime(2755, (int)giayConLai));
            }
            // Effect Bo Khi
            if (Character.InfoBuff.BoKhiTime > timeServer)
            {
                var giayConLai = (Character.InfoBuff.BoKhiTime - timeServer)/1000;
                if (giayConLai <= 0) giayConLai = 0;
                SendMessage(Service.ItemTime(2756, (int)giayConLai));
            }
            // Effect giap xen
            if (Character.InfoBuff.GiapXenTime > timeServer)
            {
                var giayConLai = (Character.InfoBuff.GiapXenTime - timeServer)/1000;
                if (giayConLai <= 0) giayConLai = 0;
                SendMessage(Service.ItemTime(2757, (int)giayConLai));
            }
            // Effect An danh
            if (Character.InfoBuff.AnDanhTime > timeServer)
            {
                var giayConLai = (Character.InfoBuff.AnDanhTime - timeServer)/1000;
                if (giayConLai <= 0) giayConLai = 0;
                SendMessage(Service.ItemTime(2760, (int)giayConLai));
            }
            // Effect Do CSKB
            if (Character.InfoBuff.MayDoCSKBTime > timeServer)
            {
                var giayConLai = (Character.InfoBuff.MayDoCSKBTime - timeServer)/1000;
                if (giayConLai <= 0) giayConLai = 0;
                SendMessage(Service.ItemTime(2758, (int)giayConLai));
            }
            // Effect Cu Ca Rot
            if (Character.InfoBuff.CuCarotTime > timeServer)
            {
                var giayConLai = (Character.InfoBuff.CuCarotTime - timeServer)/1000;
                if (giayConLai <= 0) giayConLai = 0;
                SendMessage(Service.ItemTime(4082, (int)giayConLai));
            }
        }

        public void SetEnhancedOption()
        {
            Character.InfoOption.Reset();

            Character.InfoOption.PhanPercentSatThuong += GetParamItem(97);

            Character.InfoOption.PhanTramXuyenGiapChuong += GetParamItem(98);

            Character.InfoOption.PhanTramXuyenGiapCanChien += GetParamItem(99);

            Character.InfoOption.PhanTramNeDon += GetParamItem(108);

            Character.InfoOption.PhanTramVangTuQuai += GetParamItem(100);

            Character.InfoOption.PhanTramTNSM += GetParamItem(101);
        }

        public void SetTuDongLuyenTap()
        {
            var timeServer = ServerUtils.CurrentTimeMillis();
            if (Character.InfoChar.TimeAutoPlay > 0)
            {
                var giayConLai = (Character.InfoChar.TimeAutoPlay - timeServer)/1000;
                if (giayConLai <= 0) giayConLai = 0;
                SendMessage(Service.ItemTime(4387, (int)giayConLai));
                SendMessage(Service.AutoPlay(true));
            }
        }

        public void SetHpFull()
        {
            var hp = Character.InfoChar.OriginalHp;
            hp += GetParamItem(2) * 100;
            hp += GetParamItem(6);
            hp += GetParamItem(22) * 1000;
            hp += GetParamItem(48);
            GetListParamItem(77).ForEach(param => hp += hp*param/100);
            GetListParamItem(109).ForEach(param => hp -= hp*param/100);  

            if (Character.InfoChar.Fusion.IsFusion && Character.Disciple != null) {
                // Đệ ma bư +120%
                var disHP = Character.Disciple.HpFull;

                if (Character.Disciple.Type == 2)
                {
                    hp += (disHP + (disHP*20/100));
                }
                // Đệ thường +100%
                else if (Character.Disciple.Type == 1)
                {
                    hp += disHP;
                }
                // Bông tai porata 2
                if (Character.InfoChar.Fusion.IsPorata2)
                {
                    var bongTaiPorata2 = GetItemBagById(921);
                    if (bongTaiPorata2 != null)
                    {
                        var optionCheck = bongTaiPorata2.Options.FirstOrDefault(option => option.Id != 72);
                        if (optionCheck != null && optionCheck.Id == 77)
                        {
                            hp += hp*optionCheck.Param/100;
                        }
                    }
                    // bông tai pt2 tăng 10%
                    hp += hp*10/100;
                }
            }

            // Nappa
            if (Character.InfoSet.IsFullSetNappa)
            {
                hp += hp*80/100;
            }

            if (Character.InfoSkill.Monkey.MonkeyId != 0) 
            {
                hp += hp * Character.InfoSkill.Monkey.Hp / 100;
            }

            if (Character.InfoSkill.HuytSao.IsHuytSao) 
            {
                hp += hp * Character.InfoSkill.HuytSao.Percent / 100;
            }
            // Bổ Huyết
            if (Character.InfoBuff.BoHuyet)
            {
                hp += hp;
            }

            if (Character.InfoBuff.BanhTrungThuId != -1)
            {
                switch (Character.InfoBuff.BanhTrungThuId)
                {
                    case 465:
                    {
                        hp += hp*5/100;
                        break;
                    }
                    case 466:
                    {
                        hp += hp*10/100;
                        break;
                    }
                    case 472:
                    {
                        hp += hp*15/100;
                        break;
                    }
                    case 473:
                    {
                        hp += hp*20/100;
                        break;
                    }
                }
            }
            

            Character.HpFull = hp;
        }

        public void SetMpFull()
        {
            var mp = Character.InfoChar.OriginalMp;
            mp += GetParamItem(2) * 100;
            mp += GetParamItem(7);
            mp += GetParamItem(23) * 1000;
            mp += GetParamItem(48);
            GetListParamItem(103).ForEach(param => mp += mp*param/100);

            if (Character.InfoChar.Fusion.IsFusion && Character.Disciple != null) {
                // Đệ ma bư +120%
                if (Character.Disciple.Type == 2)
                {
                    mp += (Character.Disciple.MpFull + (Character.Disciple.MpFull*20/100));
                }
                // Đệ thường +100%
                else if (Character.Disciple.Type == 1)
                {
                    mp += Character.Disciple.MpFull;
                }
                // Bông tai porata 2
                if (Character.InfoChar.Fusion.IsPorata2)
                {
                    var bongTaiPorata2 = GetItemBagById(921);
                    if (bongTaiPorata2 != null)
                    {
                        var optionCheck = bongTaiPorata2.Options.FirstOrDefault(option => option.Id != 72);
                        if (optionCheck != null && optionCheck.Id == 103)
                        {
                            mp += mp*optionCheck.Param/100;
                        }
                    }

                    mp += mp*10/100;
                }
            }

            if (Character.InfoBuff.BanhTrungThuId != -1)
            {
                switch (Character.InfoBuff.BanhTrungThuId)
                {
                    case 465:
                    {
                        mp += mp*5/100;
                        break;
                    }
                    case 466:
                    {
                        mp += mp*10/100;
                        break;
                    }
                    case 472:
                    {
                        mp += mp*15/100;
                        break;
                    }
                    case 473:
                    {
                        mp += mp*20/100;
                        break;
                    }
                }
            }

            // Bổ khí
            if (Character.InfoBuff.BoKhi)
            {
                mp += mp;
            }
            Character.MpFull = mp;
        }

        public void SetDamageFull()
        {
            var damage = Character.InfoChar.OriginalDamage;
            damage += GetParamItem(0);
            GetListParamItem(50).ForEach(param => damage += damage*param/100);
            GetListParamItem(147).ForEach(param => damage += damage*param/100);          

            if (Character.InfoChar.Fusion.IsFusion && Character.Disciple != null) {
                // Đệ ma bư +120%
                var disDmg = Character.Disciple.DamageFull;

                if (Character.Disciple.Type == 2)
                {
                    damage += (disDmg + (disDmg*20/100));
                }
                // Đệ thường +100%
                else if (Character.Disciple.Type == 1)
                {
                    damage += disDmg;
                }
                // Bông tai porata 2
                if (Character.InfoChar.Fusion.IsPorata2)
                {
                    var bongTaiPorata2 = GetItemBagById(921);
                    if (bongTaiPorata2 != null)
                    {
                        var optionCheck = bongTaiPorata2.Options.FirstOrDefault(option => option.Id != 72);
                        if (optionCheck != null && optionCheck.Id == 50)
                        {
                            damage += damage*optionCheck.Param/100;
                        }
                    }

                    damage += damage*10/100;
                }
            }

            if (Character.InfoSkill.Monkey.MonkeyId != 0) damage += damage * Character.InfoSkill.Monkey.Damage / 100;
            // Cuồng nộ
            if (Character.InfoBuff.CuongNo)
            {
                damage += damage;
            }
            
            // Kiểm tra có mặc giáp luyện tập hay không
            var itemGiap = Character.ItemBody[6];
            if(itemGiap != null && ItemCache.ItemIsGiapLuyenTap(itemGiap.Id))
            {
                damage -= (damage*ItemCache.GetGiapLuyenTapPTSucManh(itemGiap.Id))/100;
            }
            
            // Kiểm tra xem có vừa tháo giáp tập luyện ra không
            if (Character.InfoMore.LastGiapLuyenTapItemId != 0)
            {
                var giapLuyenTap = GetItemBagById(Character.InfoMore.LastGiapLuyenTapItemId);
                if (giapLuyenTap != null && ItemCache.ItemIsGiapLuyenTap(giapLuyenTap.Id))
                {
                    var optionCheck = giapLuyenTap.Options.FirstOrDefault(option => option.Id == 9);
                    if (optionCheck.Param > 0)
                    {
                        damage += (damage*ItemCache.GetGiapLuyenTapPTSucManh(giapLuyenTap.Id))/100;
                    }
                }
                else 
                {
                    Character.InfoMore.LastGiapLuyenTapItemId = 0;
                    Character.Delay.GiapLuyenTap = -1;
                }
            }

            // Thức ăn
            if (Character.InfoBuff.ThucAnId != -1)
            {
                damage += damage*10/100;
            }

            if (Character.InfoBuff.BanhTrungThuId != -1)
            {
                switch (Character.InfoBuff.BanhTrungThuId)
                {
                    case 465:
                    {
                        damage += damage*10/100;
                        break;
                    }
                    case 466:
                    {
                        damage += damage*15/100;
                        break;
                    }
                    case 472:
                    {
                        damage += damage*20/100;
                        break;
                    }
                    case 473:
                    {
                        damage += damage*25/100;
                        break;
                    }
                }
            }
            Character.DamageFull = damage;
        }

        public void SetDefenceFull()
        {
            var defence = Character.InfoChar.OriginalDefence * 4;
            defence += GetParamItem(47);
            GetListParamItem(94).ForEach(param => defence += defence*param/100);

            if (Character.InfoChar.Fusion.IsFusion && Character.Disciple != null) {
                // Bông tai porata 2
                if (Character.InfoChar.Fusion.IsPorata2)
                {
                    var bongTaiPorata2 = GetItemBagById(921);
                    if (bongTaiPorata2 != null)
                    {
                        var optionCheck = bongTaiPorata2.Options.FirstOrDefault(option => option.Id != 72);
                        if (optionCheck != null && optionCheck.Id == 94)
                        {
                            defence += defence*optionCheck.Param/100;
                        }
                    }

                    defence += defence*10/100;
                }
            }
            Character.DefenceFull = Math.Abs(defence);
        }

        public void SetCritFull()
        {
            int crtCal;
            if (Character.InfoSkill.Monkey.MonkeyId != 0)
            {
                crtCal = 115;
            }
            else
            {
                crtCal = Character.InfoChar.OriginalCrit;
                crtCal += GetParamItem(14);
            }

            if (Character.InfoChar.Fusion.IsFusion && Character.Disciple != null) {
                // Bông tai porata 2
                if (Character.InfoChar.Fusion.IsPorata2)
                {
                    var bongTaiPorata2 = GetItemBagById(921);
                    if (bongTaiPorata2 != null)
                    {
                        var optionCheck = bongTaiPorata2.Options.FirstOrDefault(option => option.Id != 72);
                        if (optionCheck != null && optionCheck.Id == 14)
                        {
                            crtCal += optionCheck.Param;
                        }
                    }
                }
            }
            Character.CritFull = crtCal;
        }

        public void SetHpPlusFromDamage()
        {
            var hpPlus = GetParamItem(95);
            Character.HpPlusFromDamage = hpPlus;

            Character.HpPlusFromDamageMonster = GetParamItem(104);
        }

        public void SetMpPlusFromDamage()
        {
            var mpPlus = GetParamItem(96);
            Character.MpPlusFromDamage = mpPlus;
        }

        public void SetSpeed()
        {
            var speed = 5;
            if (Character.InfoSkill.Monkey.MonkeyId != 0) speed = 8;
            if (Character.InfoChar.Fusion.IsFusion) speed = 7;
            var plus = speed * (GetParamItem(148) + GetParamItem(114) + GetParamItem(16)) / 100;
            switch (plus)
            {
                case <= 1:
                    speed+=1;
                    break;
                case > 1 and <= 2:
                    speed += 2;
                    break;
                // case > 2:
                //     speed += plus;
                //     break;
            }
            Character.InfoChar.Speed = (sbyte)speed;
        }

        public void SetBuffHp30s()
        {
            var hpPlus = GetParamItem(27);
            Character.Effect.BuffHp30S.Value = hpPlus;
            if (Character.Effect.BuffHp30S.Time == -1)
            {
                Character.Effect.BuffHp30S.Time = 30000 + ServerUtils.CurrentTimeMillis();
            }
            
        }

        public void SetBuffMp1s()
        {
            var mpPlus = (int)Character.MpFull * GetParamItem(162)/100;
            Character.Effect.BuffKi1s.Value = mpPlus;
            if (Character.Effect.BuffKi1s.Time == -1)
            {
                Character.Effect.BuffKi1s.Time = 1500 + ServerUtils.CurrentTimeMillis();
            }
        }

        public void SetBuffHp5s()
        {
            //TODO HANDLE PLUS HP 5s
        }

        public void SetBuffHp10s()
        {
            //TODO HANDLE PLUS HP 10s
        }

        public void Clear() => SuppressFinalize(this);

        public void BagSort()
        {
            var listItemCheck = Character.ItemBag
                .Where(item => ItemCache.ItemTemplate(item.Id).IsUpToUp && item.Quantity < 99).ToList();
            Character.ItemBag.RemoveAll(item => listItemCheck.Contains(item));
            var enumerable = listItemCheck
                .GroupBy(i => i.Id)
                .Select(g =>
                {
                    var item = ItemCache.GetItemDefault(g.Key);
                    item.Quantity = g.Sum(it => it.Quantity);
                    return item;
                }).ToList();
            enumerable.ToList().ForEach(item =>
            {
                if (item.Quantity <= 99) return;
                var itemNew = ItemHandler.Clone(item);
                itemNew.Quantity = item.Quantity - 99;
                enumerable.Add(itemNew);
                item.Quantity = 99;
            });
            Character.ItemBag.AddRange(enumerable);
            var count = 0;
            Character.ItemBag.ForEach(item => item.IndexUI = count++);
        }

        public void Upindex(int index)
        {
            var itemBag = GetItemBagByIndex(index);
            if (itemBag == null) return;
            if (index >= Character.ItemBag.Count) return;
            var count = 0;
            Character.ItemBag.ForEach(item => item.IndexUI = count++);
        }
        public void BoxSort()
        {
            var listItemCheck = Character.ItemBox
                .Where(item => ItemCache.ItemTemplate(item.Id).IsUpToUp && item.Quantity < 99).ToList();
            Character.ItemBox.RemoveAll(item => listItemCheck.Contains(item));
            var enumerable = listItemCheck
                .GroupBy(i => i.Id)
                .Select(g =>
                {
                    var item = ItemCache.GetItemDefault(g.Key);
                    item.Quantity = g.Sum(it => it.Quantity);
                    return item;
                }).ToList();
            enumerable.ToList().ForEach(item =>
            {
                if (item.Quantity <= 99) return;
                var itemNew = ItemHandler.Clone(item);
                itemNew.Quantity = item.Quantity - 99;
                enumerable.Add(itemNew);
                item.Quantity = 99;
            });
            Character.ItemBox.AddRange(enumerable);
            var count = 0;
            Character.ItemBox.ForEach(item => item.IndexUI = count++);
        }

        public void HandleJoinMap(Zone zone)
        {
            lock (zone.Characters)
            {
                zone.Characters.Values.Where(x => x.Id != Character.Id).ToList().ForEach(c =>
                {
                    SendMessage(Service.PlayerAdd(c));
                    var infoSkill = c.InfoSkill;
                    if (infoSkill.MeTroi.IsMeTroi)
                    {
                        if (infoSkill.MeTroi.Monster != null)
                        {
                            SendMessage(Service.SkillEffectMonster(c.Id, infoSkill.MeTroi.Monster.IdMap, 1, 32));
                        } 
                    }

                    if (infoSkill.PlayerTroi.IsPlayerTroi)
                    {
                        infoSkill.PlayerTroi.PlayerId.ForEach(o =>
                        {
                            SendMessage(Service.SkillEffectPlayer(o, c.Id, 1, 32));
                        });
                    }

                    if (infoSkill.ThaiDuongHanSan.IsStun)
                    {
                        SendMessage(Service.SkillEffectPlayer(c.Id, c.Id, 1, 40));
                    }

                    if (infoSkill.DichChuyen.IsStun)
                    {
                        SendMessage(Service.SkillEffectPlayer(c.Id, c.Id, 1, 40));
                    }

                    if (infoSkill.Protect.IsProtect)
                    {
                        SendMessage(Service.SkillEffectPlayer(c.Id, c.Id, 1, 33));
                    }

                    if (infoSkill.ThoiMien.IsThoiMien)
                    {
                        SendMessage(Service.SkillEffectPlayer(c.Id, c.Id, 1, 41));
                    }
                });
            }

            lock (zone.Disciples)
            {
                foreach (var disciplesValue in zone.Disciples.Values)
                {
                    var text = "#";
                    if (Character.Id + disciplesValue.Id == 0) text = "$";
                    SendMessage(Service.PlayerAdd(disciplesValue, text));
                    var infoSkill = disciplesValue.InfoSkill;
                    if (infoSkill.MeTroi.IsMeTroi)
                    {
                        if (infoSkill.MeTroi.Monster != null)
                        {
                            SendMessage(Service.SkillEffectMonster(disciplesValue.Id, infoSkill.MeTroi.Monster.IdMap, 1, 32));
                        } 
                    }

                    if (infoSkill.PlayerTroi.IsPlayerTroi)
                    {
                        infoSkill.PlayerTroi.PlayerId.ForEach(o =>
                        {
                            SendMessage(Service.SkillEffectPlayer(o, disciplesValue.Id, 1, 32));
                        });
                    }

                    if (infoSkill.ThaiDuongHanSan.IsStun)
                    {
                        SendMessage(Service.SkillEffectPlayer(disciplesValue.Id, disciplesValue.Id, 1, 40));
                    }

                    if (infoSkill.DichChuyen.IsStun)
                    {
                        SendMessage(Service.SkillEffectPlayer(disciplesValue.Id, disciplesValue.Id, 1, 40));
                    }

                    if (infoSkill.Protect.IsProtect)
                    {
                        SendMessage(Service.SkillEffectPlayer(disciplesValue.Id, disciplesValue.Id, 1, 33));
                    }

                    if (infoSkill.ThoiMien.IsThoiMien)
                    {
                        SendMessage(Service.SkillEffectPlayer(disciplesValue.Id, disciplesValue.Id, 1, 41));
                    }
                }
            }

            lock (zone.Pets)
            {
                foreach (var petValue in zone.Pets.Values)
                {
                    var text = "#";
                    if ((Character.Id + 1000) + petValue.Id == 0) text = "$";
                    SendMessage(Service.PlayerAdd(petValue, text));
                }
            }

            lock (zone.Bosses)
            {
                foreach (var bossesValue in zone.Bosses.Values)
                {
                    SendMessage(Service.PlayerAdd(bossesValue));
                    var infoSkill = bossesValue.InfoSkill;
                    if (infoSkill.MeTroi.IsMeTroi)
                    {
                        if (infoSkill.MeTroi.Monster != null)
                        {
                            SendMessage(Service.SkillEffectMonster(bossesValue.Id, infoSkill.MeTroi.Monster.IdMap, 1, 32));
                        } 
                    }

                    if (infoSkill.PlayerTroi.IsPlayerTroi)
                    {
                        infoSkill.PlayerTroi.PlayerId.ForEach(o =>
                        {
                            SendMessage(Service.SkillEffectPlayer(o, bossesValue.Id, 1, 32));
                        });
                    }

                    if (infoSkill.ThaiDuongHanSan.IsStun)
                    {
                        SendMessage(Service.SkillEffectPlayer(bossesValue.Id, bossesValue.Id, 1, 40));
                    }

                    if (infoSkill.DichChuyen.IsStun)
                    {
                        SendMessage(Service.SkillEffectPlayer(bossesValue.Id, bossesValue.Id, 1, 40));
                    }

                    if (infoSkill.Protect.IsProtect)
                    {
                        SendMessage(Service.SkillEffectPlayer(bossesValue.Id, bossesValue.Id, 1, 33));
                    }

                    if (infoSkill.ThoiMien.IsThoiMien)
                    {
                        SendMessage(Service.SkillEffectPlayer(bossesValue.Id, bossesValue.Id, 1, 41));
                    }
                }
            }

            lock (zone.MonsterPets)
            {
                zone.MonsterPets.Values.Where(m => m is {IsDie: false} && m.IdMap != Character.Id).ToList().ForEach(m =>
                {
                    SendMessage(Service.UpdateMonsterMe0(m));
                }); 
            }

            zone.MonsterMaps.Where(m => !m.IsDie).ToList().ForEach(m =>
            {
                var infoSkill = m.InfoSkill;
                if (infoSkill.ThaiDuongHanSan.IsStun)
                {
                    SendMessage(Service.SkillEffectMonster(-1, m.IdMap, 1, 40));
                }

                if (infoSkill.DichChuyen.IsStun)
                {
                    SendMessage(Service.SkillEffectMonster(-1, m.IdMap, 1, 40));
                }

                if (infoSkill.ThoiMien.IsThoiMien)
                {
                    SendMessage(Service.SkillEffectMonster(-1, m.IdMap, 1, 41));
                }
            });
            
            // Xử lý trứng Ma bư, dưa hấu tại đây
            if (Character.InfoChar.ThoiGianTrungMaBu > 0 && Character.InfoChar.MapId - 21 == Character.InfoChar.Gender)
            {
                SendMessage(Service.TrungMaBu(Character));
            }
            // Gửi status trứng ma bư qua cmd duahau
        }
        
        public void AddItemToBody(Model.Item.Item item, int index)
        {
            if (item == null) return;
            item.IndexUI = index;
            Character.ItemBody[index] = item;

            UpdateAntiChangeServerTime();
            Character.Delay.NeedToSaveBody = true;
            
        }

        #region ItemBag
        public Model.Item.Item GetItemBagByIndex(int index)
        {
            return Character.ItemBag.FirstOrDefault(item => item.IndexUI == index);
        }

        public Model.Item.Item GetItemBagById(int id)
        {
            return Character.ItemBag.FirstOrDefault(item => item.Id == id);
        }

        private int IndexItemBagNotMaxQuantity(short id)
        {
            var item = Character.ItemBag.FirstOrDefault(item => (item.Quantity < 99 || ItemCache.IsUnlimitItem(id)) && item.Id == id);
            return item?.IndexUI ?? -1;
        }

        public Model.Item.Item ItemBagNotMaxQuantity(short id)
        {
            return Character.ItemBag.FirstOrDefault(item => (item.Quantity < 99 || ItemCache.IsUnlimitItem(id)) && item.Id == id);
        }

        public Model.Item.Item ItemBagNotMaxQuantity(short id, int indexUi)
        {
            return Character.ItemBag.FirstOrDefault(item => item.IndexUI != indexUi && (item.Quantity < 99 || ItemCache.IsUnlimitItem(id)) && item.Id == id);
        }

        public bool AddItemToBag(bool isUpToUp, Model.Item.Item item, string reason = "")
        {
            try
            {
                if (item == null) return false;
                var index = IndexItemBagNotMaxQuantity(item.Id);
                var itemTemplate = ItemCache.ItemTemplate(item.Id);
                if (isUpToUp && itemTemplate.IsUpToUp && index != -1)
                {
                    var itemBag = GetItemBagByIndex(index);
                    var quantity = itemBag.Quantity + item.Quantity;
                    if (quantity > 99 && !ItemCache.IsUnlimitItem(item.Id))
                    {
                        var itemClone = ItemHandler.Clone(itemBag);
                        itemClone.Quantity = quantity - 99;
                        if (!AddItemToBag(itemClone, reason)) return false;
                        quantity = 99;
                    }

                    ServerUtils.WriteLog("additem/" + Character.Id, $"BAG:{Character.Name} add {quantity}x{itemTemplate.Name} (old: {itemBag.Quantity}) lydo: " + reason);
                    itemBag.Quantity = quantity;
                    UpdateAntiChangeServerTime(reason);
                    Character.Delay.NeedToSaveBag = true;
                    
                    return true;
                }
                else
                {
                    return AddItemToBag(item, reason);
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error AddItemToBag in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return false;
            }
        }

        private bool AddItemToBag(Model.Item.Item item, string reason)
        {
            if (item != null)
            {
                if (Character.LengthBagNull() > 0)
                {
                    var itemTemplate = ItemCache.ItemTemplate(item.Id);
                    lock (Character.ItemBag)
                    {
                        var index = Character.ItemBag.Count;
                        item.IndexUI = index;
                        Character.ItemBag.Add(item);
                    }
                    
                    ServerUtils.WriteLog("additem/" + Character.Id, $"BAG:{Character.Name} add {item.Quantity}x{itemTemplate.Name} lydo: " + reason);
                    UpdateAntiChangeServerTime(reason);
                    Character.Delay.NeedToSaveBag = true;
                    
                    if(itemTemplate.Type is 23 or 24) UpdateMountId();
                    if(itemTemplate.Type == 11) UpdatePhukien();
                    return true;
                }
                else
                {
                    SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        
        public void RemoveItemBagById(short id, int quantity, string reason = "")
        {
            var num = 0;
            var lengthOld = Character.ItemBag.Count;
            Character.ItemBag.ToList().ForEach(itemBag =>
            {
                if (itemBag == null || itemBag.Id != id) return;
                if (num + itemBag.Quantity >= quantity)
                {
                    RemoveItemBagByIndex(itemBag.IndexUI, quantity - num, false, reason);
                    id = -1;
                    return;
                }
                num += itemBag.Quantity;
                RemoveItemBagByIndex(itemBag.IndexUI, itemBag.Quantity, false, reason);
                var itemTemplate = ItemCache.ItemTemplate(itemBag.Id);
                if(itemTemplate.Type is 23 or 24) UpdateMountId();
                if(itemTemplate.Type == 11) UpdatePhukien();
            });
            if (lengthOld == Character.ItemBag.Count) return;
            num = 0;
            Character.ItemBag.ForEach(item => item.IndexUI = num++);
        }

        public void RemoveItemBagByIndex(int index, int quantity, bool reset = true, string reason = "")
        {
            lock (Character.ItemBag)
            {
                var itemBag = GetItemBagByIndex(index);
                if(itemBag == null) return;
                itemBag.Quantity -= quantity;
                
                if (itemBag.Quantity <= 0) Character.ItemBag.RemoveAll(item => item.IndexUI == index);

                UpdateAntiChangeServerTime(reason);
                
                Character.Delay.NeedToSaveBag = true;
                
                var itemTemplate = ItemCache.ItemTemplate(itemBag.Id);
                ServerUtils.WriteLog("removeitem/" + Character.Id, $"BAG:{Character.Name} remove {quantity}x{itemTemplate.Name} lydo: " + reason);
                if(itemTemplate.Type == 11) UpdatePhukien();
                if(itemTemplate.Type is 23 or 24) UpdateMountId();
                if (!reset || index >= Character.ItemBag.Count) return;
                {
                    var count = 0;
                    Character.ItemBag.ForEach(item => item.IndexUI = count++);
                }
                if(itemTemplate.Type is 23 or 24) UpdateMountId();
            }
        }

        public Model.Item.Item RemoveItemBag(int index, bool isReset = true, string reason = "")
        {
            var itemBag = GetItemBagByIndex(index);
            lock (Character.ItemBag)
            {
                if (itemBag == null) return null;
                Character.ItemBag.RemoveAll(item => item.IndexUI == index);
                if (isReset && index < Character.ItemBag.Count)
                {
                    var count = 0;
                    Character.ItemBag.ForEach(item => item.IndexUI = count++);
                }
                SendMessage(Service.SendBag(Character));
                var itemTemplate = ItemCache.ItemTemplate(itemBag.Id);
                ServerUtils.WriteLog("removeitem/" + Character.Id, $"BAG:{Character.Name} remove {itemTemplate.Name} lydo: " + reason);
                if(itemTemplate.Type is 23 or 24) UpdateMountId();
                if(itemTemplate.Type == 11) UpdatePhukien();
                UpdateAntiChangeServerTime(reason);
                Character.Delay.NeedToSaveBag = true;
                
            }
            return itemBag;
        }
        #endregion

        #region Item Box
        public bool AddItemToBox(bool isUpToUp, Model.Item.Item item)
        {
            try
            {
                if (item == null) return false;
                var index = IndexItemBoxNotMaxQuantity(item.Id);
                var itemTemplate = ItemCache.ItemTemplate(item.Id);
                if (isUpToUp && itemTemplate.IsUpToUp && index != -1)
                {
                    var itemBox = GetItemBoxByIndex(index);
                    var quantity = itemBox.Quantity + item.Quantity;
                    if (quantity > 99)
                    {
                        var itemClone = ItemHandler.Clone(itemBox);
                        itemClone.Quantity = quantity - 99;
                        if (!AddItemToBox(itemClone)) return false;
                        quantity = 99;
                    }

                    itemBox.Quantity = quantity;
                    UpdateAntiChangeServerTime();
                    Character.Delay.NeedToSaveBox = true;
                    
                    return true;
                }
                else
                {
                    return AddItemToBox(item);
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error AddItemToBox in Service.cs: {e.Message} \n {e.StackTrace}", e);
                return false;
            }
        }

        private bool AddItemToBox(Model.Item.Item item)
        {
            if (item != null)
            {
                if (Character.LengthBoxNull() > 0)
                {
                    lock (Character.ItemBox)
                    {
                        var index = Character.ItemBox.Count;
                        item.IndexUI = index;
                        Character.ItemBox.Add(item);
                    }
                    UpdateAntiChangeServerTime();
                    Character.Delay.NeedToSaveBox = true;
                    
                    return true;
                }
                else
                {
                    SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BOX));
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public Model.Item.Item GetItemBoxByIndex(int index)
        {
            return Character.ItemBox.FirstOrDefault(item => item.IndexUI == index);
        }
        public Model.Item.Item GetItemLuckyBoxByIndex(int index)
        {
            return Character.LuckyBox.FirstOrDefault(item => item.IndexUI == index);
        }
        public Model.Item.Item GetItemBoxById(int id)
        {
            return Character.ItemBox.FirstOrDefault(item => item.Id == id);
        }

        public void RemoveMonsterMe()
        {
            var skillEgg = Character.InfoSkill.Egg;
            if (skillEgg.Monster is {IsDie: true})
            {
                SendZoneMessage(Service.UpdateMonsterMe7(skillEgg.Monster.Id));
                Character.Zone.ZoneHandler.RemoveMonsterMe(skillEgg.Monster.Id);
                SkillHandler.RemoveMonsterPet(Character);
            }
        }

        public void PlusPoint(IMonster monster, int damage)
        {
            if(monster.Character != null) return;
            if(damage <= 0) return;
            var timeServer = ServerUtils.CurrentTimeMillis();
            long fixDmg = (long) ((damage) + (monster.OriginalHp * 0.00125));
            long damagePlusPoint = fixDmg;

            if (Character.InfoChar.Power > DatabaseManager.Manager.gI().LimitPowerExpUp)
            {
                damagePlusPoint /= 2;
            }

            if (Character.InfoChar.Power > 100000000000)
            {
                damagePlusPoint /= 2;
            }

            if (damagePlusPoint <= 0)
            {
                damagePlusPoint = 2;
            }

            if (monster.Id != 0)
            {
                var levelChar = Character.InfoChar.Level;
                var levelMonster = monster.Level;
                var checkLevel = Math.Abs(levelChar - levelMonster);
                if (checkLevel > 5 && levelChar > levelMonster)
                {
                    damagePlusPoint = 1;
                } else if (levelChar < levelMonster && checkLevel <= 5)
                {
                    damagePlusPoint *= (monster.LvBoss * 2 + 2);
                }
                else
                {
                    damagePlusPoint *= monster.LvBoss * 2 + 1;
                }
            }
            // else
            // {
            //     damagePlusPoint = 1;//ServerUtils.RandomNumber(100) < 3 ? 1 : 0;
            // }

            switch (Character.Flag)
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
                if (Character.InfoChar.Power < DatabaseManager.Manager.gI().LimitPowerExpUp)
                {
                    damagePlusPoint *= DatabaseManager.Manager.gI().ExpUp;
                }
                else 
                {
                    damagePlusPoint *= (DatabaseManager.Manager.gI().ExpUp/2);
                }
            }

            // Nội tại tăng tiềm năng sức mạnh đánh quái
            var specialId = Character.SpecialSkill.Id;
            if (specialId != -1 && (specialId == 8 || specialId == 19 || specialId == 29))
            {
                damagePlusPoint += damagePlusPoint*Character.SpecialSkill.Value/100;
            }
            // Option Sao pha lê
            var OptionPhanTramTNSM = Character.InfoOption.PhanTramTNSM;
            if (OptionPhanTramTNSM > 0)
            {
                damagePlusPoint += damagePlusPoint*OptionPhanTramTNSM/100;
            }
            
            // // Bùa Trí Tuệ x4
            if (Character.InfoMore.BuaTriTueX4)
            {
                if (Character.InfoMore.BuaTriTueX4Time > timeServer)
                {
                    damagePlusPoint *= 4;
                }
                else 
                {
                    Character.InfoMore.BuaTriTueX4 = false;
                }
            } 
            else if (Character.InfoMore.BuaTriTueX3) // Bùa Trí Tuệ x3
            {
                if (Character.InfoMore.BuaTriTueX3Time > timeServer)
                {
                    damagePlusPoint *= 3;
                }
                else 
                {
                    Character.InfoMore.BuaTriTueX3 = false;
                }
            }
            // Bùa Trí Tuệ 213
            else if (Character.InfoMore.BuaTriTue)
            {
                if (Character.InfoMore.BuaTriTueTime > timeServer)
                {
                    damagePlusPoint *= 2;
                }
                else 
                {
                    Character.InfoMore.BuaTriTue = false;
                }
            }
            // Vệ tinh Trí tuệ
            if (Character.InfoMore.IsNearAuraTriTueItem)
            {
                if (Character.InfoMore.AuraTriTueTime > timeServer)
                {
                    damagePlusPoint += damagePlusPoint*20/100;
                }
                else 
                {
                    Character.InfoMore.IsNearAuraTriTueItem = false;
                }
            }

            if (Character.InfoChar.IsPower)
            {
                PlusPoint(damagePlusPoint, damagePlusPoint, true);
            }
            else
            {
                PlusPoint(0, damagePlusPoint, false);
            }

            foreach (var clanChar in Character.Zone.Characters.Values.ToList().Where(c => c.ClanId != -1 && c.ClanId == Character.ClanId && c.Id != Character.Id))
            {
                clanChar.CharacterHandler.PlusPoint(0, damagePlusPoint/2, false);
            }
        }

        public void PlusPoint(long power, long potential, bool isAll)
        {
            // if (!Character.InfoChar.IsPremium && Character.InfoChar.Power >= DataCache.PREMIUM_LIMIT_UP_POWER)
            // {
            //     SendMessage(Service.ServerMessage(TextServer.gI().NOT_PREMIUM_LIMIT_POWER));
            //     return;
            // }

            if (isAll && power > 0 && potential > 0)
            {
                PlusPower(power);
                PlusPotential(potential);
                SendMessage(Service.UpdateExp(2, power));
            }
            else
            {
                if (power > 0)
                {
                    PlusPower(power);
                    SendMessage(Service.UpdateExp(0, power));
                }

                if (potential > 0)
                {
                    PlusPotential(potential);
                    SendMessage(Service.UpdateExp(1, potential));
                }
            }
        }

        public void LeaveFromDead(bool isHeal = false)
        {
            lock (Character)
            {
                if (!isHeal)
                {
                    if (Character.AllDiamond() < 5)
                    {
                        SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_DIAMOND));
                        return;
                    }
                    Character.MineDiamond(5);
                }
                SendMessage(Service.MeLoadInfo(Character));
                Character.InfoChar.IsDie = false;
                Character.InfoChar.Hp = Character.HpFull;
                Character.InfoChar.Mp = Character.MpFull;
                SendMessage(Service.MeLive());
                SendZoneMessage(Service.ReturnPointMap(Character));
                SendZoneMessage(Service.PlayerLoadLive(Character));
            }
        }

        public void BackHome()
        {
            lock (Character)
            {
                SendZoneMessage(Service.SendTeleport(Character.Id, Character.InfoChar.Teleport));
                Character.Zone.Map.OutZone(Character, Character.InfoChar.Gender+21);
                Character.InfoChar.IsDie = false;
                Character.InfoChar.Hp = 1;
                SendMessage(Service.MeLive());
                SendMessage(Service.PlayerLevel(Character));
                SendMessage(Service.MeLoadInfo(Character));
                var home = new Home(Character.InfoChar.Gender);
                home.Maps[0].JoinZone(Character, 0, true, true, Character.InfoChar.Teleport);

            }
        }

        private int IndexItemBoxNotMaxQuantity(short id)
        {
            var item = Character.ItemBox.FirstOrDefault(item => item.Quantity < 99 && item.Id == id);
            return item?.IndexUI ?? -1;
        }

        public void RemoveItemBoxByIndex(int index, int quantity, bool reset = true)
        {
            lock (Character.ItemBox)
            {
                var itemBox = GetItemBoxByIndex(index);
                if(itemBox == null) return;
                itemBox.Quantity -= quantity;
                if (itemBox.Quantity <= 0) Character.ItemBox.RemoveAll(item => item.IndexUI == index);
                UpdateAntiChangeServerTime();
                Character.Delay.NeedToSaveBox = true;
                
                if (!reset || index >= Character.ItemBox.Count) return;
                {
                    var count = 0;
                    Character.ItemBox.ForEach(item => item.IndexUI = count++);
                }
            }
        }

        public Model.Item.Item RemoveItemBox(int index, bool isReset = true)
        {
            lock (Character.ItemBox)
            {
                var itemBox = Character.ItemBox.FirstOrDefault(item => item.IndexUI == index);
                if(itemBox == null) return null;
                Character.ItemBox.RemoveAll(item => item.IndexUI == index);
                if (isReset && index < Character.ItemBox.Count)
                {
                    var count = 0;
                    Character.ItemBox.ForEach(item => item.IndexUI = count++);
                }
                SendMessage(Service.SendBox(Character));
                UpdateAntiChangeServerTime();
                Character.Delay.NeedToSaveBox = true;
                
                return itemBox; 
            }
            
        }
        public Model.Item.Item RemoveItemLuckyBox(int index, bool isReset = true)
        {
            lock (Character.LuckyBox)
            {
                var itemBox = Character.LuckyBox.FirstOrDefault(item => item.IndexUI == index);
                if(itemBox == null) return null;
                Character.LuckyBox.RemoveAll(item => item.IndexUI == index);
                if (isReset && index < Character.LuckyBox.Count)
                {
                    var count = 0;
                    Character.LuckyBox.ForEach(item => item.IndexUI = count++);
                }
                UpdateAntiChangeServerTime();
                Character.Delay.NeedToSaveLucky = true;
                
                return itemBox; 
            }
            
        }
        public void MoveMap(short toX, short toY, int type = 0)
        {
            Character.InfoChar.X = toX;
            Character.InfoChar.Y = toY;
            if (type == 1)
            {
                var mpMine = (int) Character.InfoChar.OriginalMp / 100 *
                             (Character.InfoSkill.Monkey.MonkeyId > 0 ? 2 : 1);
                if (Character.InfoChar.Mp > mpMine)
                {
                    if (Character.InfoChar.MountId == -1)
                    {
                        MineMp(mpMine);
                    }
                }
            }
            SendZoneMessage(Service.PlayerMove(Character.Id, Character.InfoChar.X, Character.InfoChar.Y));
            if (Character.InfoSkill.MeTroi.IsMeTroi &&
                Character.InfoSkill.MeTroi.DelayStart <= ServerUtils.CurrentTimeMillis())
            {
                SkillHandler.RemoveTroi(Character);
            }

            var disciple = Character.Disciple;
            if (disciple != null && Character.InfoChar.IsHavePet && !Character.InfoChar.Fusion.IsFusion)
            {
                if(disciple.Status == 0 || disciple.Status == 1 && Math.Abs(Character.InfoChar.X - disciple.InfoChar.X) > 60 ||disciple.Status == 2 && Math.Abs(Character.InfoChar.X - disciple.InfoChar.X) > 300 || disciple.Status == 3 && Math.Abs(Character.InfoChar.X - disciple.InfoChar.X) > 600)
                {
                    Character.Disciple.CharacterHandler.MoveMap(Character.InfoChar.X, Character.InfoChar.Y); 
                }
            }

            var pet = Character.Pet;
            if (pet != null)
            {
                if(Math.Abs(Character.InfoChar.X - pet.InfoChar.X) > 60)
                {
                    Character.Pet.CharacterHandler.MoveMap(Character.InfoChar.X, Character.InfoChar.Y); 
                }
            }
        }

        #endregion

        public void PlusHp(int hp)
        {
            lock (Character.InfoChar)
            {
                if(Character.InfoChar.IsDie) return;
                Character.InfoChar.Hp += hp;
                if (Character.InfoChar.Hp >= Character.HpFull) Character.InfoChar.Hp = Character.HpFull;
            }
        }

        public void MineHp(long hp)
        {
            lock (Character.InfoChar)
            {
                if(Character.InfoChar.IsDie || hp <= 0) return;
                
                if (hp > Character.InfoChar.Hp)
                {
                    Character.InfoChar.Hp = 0;
                }
                else 
                {
                    Character.InfoChar.Hp -= hp;
                }

                if (Character.InfoChar.Hp <= 0)
                {
                    Character.InfoChar.IsDie = true;
                    Character.InfoChar.Hp = 0;
                }
            }
        }

        public void PlusMp(int mp)
        {
            lock (Character.InfoChar)
            {
                if(Character.InfoChar.IsDie) return;
                Character.InfoChar.Mp += mp;
                if (Character.InfoChar.Mp >= Character.MpFull) Character.InfoChar.Mp = Character.MpFull;
            }
        }

        public void MineMp(int mp)
        {
            lock (Character.InfoChar)
            {
                if(Character.InfoChar.IsDie || mp < 0) return;
                Character.InfoChar.Mp -= mp;
                if (Character.InfoChar.Mp <= 0) Character.InfoChar.Mp = 0;
            }
        }

        public void PlusStamina(int stamina)
        {
            lock (Character.InfoChar)
            {
                Character.InfoChar.Stamina += (short)stamina;
                if (Character.InfoChar.Stamina > 10000) Character.InfoChar.Stamina = 10000;
            }
        }

        public void MineStamina(int stamina)
        {
            // Bùa Dẻo Dai 218
            if (Character.InfoMore.BuaDeoDai)
            {
                if (Character.InfoMore.BuaDeoDaiTime > ServerUtils.CurrentTimeMillis())
                {
                    return;
                }
                else 
                {
                    Character.InfoMore.BuaDeoDai = false;
                }
            }
            // 
            lock (Character.InfoChar)
            {
                if (stamina < 0) return;
                Character.InfoChar.Stamina -= (short)stamina;
                if (Character.InfoChar.Stamina <= 0) Character.InfoChar.Stamina = 0;
            }
        }

        public void PlusPower(long power)
        {
            lock (Character.InfoChar)
            {
                Character.InfoChar.Power += power;
                Character.InfoChar.Level = (sbyte)(Cache.Gi().EXPS.Count(exp => exp < Character.InfoChar.Power) - 1);
                if (Cache.Gi().LIMIT_POWERS[Character.InfoChar.LitmitPower].Power > Character.InfoChar.Power)
                {
                    Character.InfoChar.IsPower = true;
                }
                else 
                {
                    Character.InfoChar.IsPower = false;
                }
            }
        }

        public void PlusPotential(long potential)
        {
            lock (Character.InfoChar)
            {
                Character.InfoChar.Potential += potential;
            }
        }

        public Model.Item.Item RemoveItemBody(int index)
        {
            Model.Item.Item item;
            lock (Character.ItemBody)
            {
                item = Character.ItemBody[index];
                if (item == null) return null;
                Character.ItemBody[index] = null;
                UpdateInfo();
                UpdateAntiChangeServerTime();
                Character.Delay.NeedToSaveBody = true;
                
                if (Character.ItemBody[5] != null) return item;

                SendMessage(Service.SendBody(Character));
                SendMessage(Service.PlayerLoadVuKhi(Character));
                switch (index)
                {
                    case 0:
                        SendMessage(Service.PlayerLoadAo(Character));
                        break;
                    case 1:
                        SendMessage(Service.PlayerLoadQuan(Character));
                        break;
                }
            }
            return item;
        }

        public void DropItemBody(int index)
        {
            var item = RemoveItemBody(index);
            var zone = MapManager.Get(Character.InfoChar.MapId)?.GetZoneById(Character.InfoChar.ZoneId);
            if(item == null || zone == null)return;
            zone.ZoneHandler.LeaveItemMap(new ItemMap()
            {
                PlayerId = Character.Id,
                X = Character.InfoChar.X,
                Y = Character.InfoChar.Y,
                Item = item,
            });
            
        }

        public void DropItemBag(int index)
        {
            var item = RemoveItemBag(index, reason: "Vứt vật phẩm");
            var zone = MapManager.Get(Character.InfoChar.MapId)?.GetZoneById(Character.InfoChar.ZoneId);
            if(item == null || zone == null)return;
            zone.ZoneHandler.LeaveItemMap(new ItemMap()
            {
                PlayerId = Character.Id,
                X = Character.InfoChar.X,
                Y = Character.InfoChar.Y,
                Item = item,
            });
            
        }

        public void PickItemMap(short id)
        {
            try
            {
                var zone = DataCache.IdMapCustom.Contains(Character.InfoChar.MapId) 
                    ? MapManager.GetMapCustom(Character.InfoChar.MapCustomId)?.GetMapById(Character.InfoChar.MapId)?.GetZoneById(Character.InfoChar.ZoneId) 
                    : MapManager.Get(Character.InfoChar.MapId)?.GetZoneById(Character.InfoChar.ZoneId);
                var itemMap = zone?.ItemMaps.Values.FirstOrDefault(item => item.Id == id);
                if(itemMap == null) return;
                if (itemMap.PlayerId == -2) return;
                if (itemMap.PlayerId != -1 && itemMap.PlayerId != Character.Id)
                {
                    SendMessage(Service.ServerMessage(TextServer.gI().ITEM_OF_ORTHER));
                    return;
                }

                if (Math.Abs(itemMap.X - Character.InfoChar.X) >= 70 && !Character.InfoMore.BuaThuHut)
                {
                    SendMessage(Service.ServerMessage(TextServer.gI().SO_FAR));
                    return;
                }

                lock (zone.ItemMaps)
                {
                    var itemNew = itemMap.Item;
                    if(itemNew == null) return;
                    switch (itemNew.Id)
                    {
                        case 516:
                        {
                            PlusHp((int)Character.HpFull/10);
                            PlusMp((int)Character.MpFull/10);
                            SendMessage(Service.SendHp((int)Character.InfoChar.Hp));
                            SendMessage(Service.SendMp((int)Character.InfoChar.Mp));
                            zone.ZoneHandler.SendMessage(Service.PlayerLevel(Character), Character.Id);
                            zone.ZoneHandler.SendMessage(Service.ItemMapMePick(itemMap.Id, itemNew.Quantity, "a"));
                            break;
                        }
                        case 74:
                        {
                            if(Character.InfoChar.MapId - 21 != Character.InfoChar.Gender) return;
                            PlusHp((int)Character.HpFull);
                            PlusMp((int)Character.MpFull);
                            PlusStamina((int)Character.InfoChar.MaxStamina);
                            SendMessage(Service.SendHp((int)Character.InfoChar.Hp));
                            SendMessage(Service.SendMp((int)Character.InfoChar.Mp));
                            SendMessage(Service.SendStamina(Character.InfoChar.Stamina));
                            zone.ZoneHandler.SendMessage(Service.PlayerLevel(Character), Character.Id);
                            zone.ZoneHandler.SendMessage(Service.ItemMapMePick(itemMap.Id, itemNew.Quantity, "a"));
                            break;
                        }
                        case 568: //Trứng ma bư
                        {
                            if (Character.InfoChar.ThoiGianTrungMaBu > 0)
                            {
                                SendMessage(Service.ServerMessage(TextServer.gI().DA_CO_TRUNG_MABU));
                                return;
                            }
                            Character.InfoChar.ThoiGianTrungMaBu = (DataCache.TRUNG_MA_BU_TIME + ServerUtils.CurrentTimeMillis());
                            zone.ZoneHandler.SendMessage(Service.ItemMapMePick(itemMap.Id, itemNew.Quantity, "a"));
                            SendMessage(Service.ServerMessage("Bạn đã nhặt được một quả trứng Ma Bư\nHãy về nhà kiểm tra"));
                            break;
                        }
                        case 933:
                        {
                            // if(Character.LengthBagNull() < 1) {
                            //     SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                            //     return;
                            // }
                            // mảnh vỡ
                            var itemManhVoBongTai = Character.CharacterHandler.GetItemBagById(933);
                            if (itemManhVoBongTai != null) 
                            {
                                var soLuongManhVoBongTaiHT = itemManhVoBongTai.Options.FirstOrDefault(opt => opt.Id == 31); //Số lượng bông tai
                                var soLuongManhVoBongTaiDrop = itemNew.Options.FirstOrDefault(opt => opt.Id == 31);
                                if (soLuongManhVoBongTaiHT != null && soLuongManhVoBongTaiDrop != null)
                                {
                                    soLuongManhVoBongTaiHT.Param += soLuongManhVoBongTaiDrop.Param;
                                }
                                else 
                                {
                                    soLuongManhVoBongTaiHT.Param += 1;//default
                                }
                            }
                            else 
                            {
                                if (!Character.CharacterHandler.AddItemToBag(true, itemNew, "Nhặt từ map")) return;
                            }
                            Character.CharacterHandler.SendMessage(Service.SendBag(Character));
                            SendMessage(Service.ItemMapMePick(itemMap.Id, itemNew.Quantity, TextServer.gI().EMPTY));
                            break;
                        }
                        case 992://nhẫn thời không
                        {
                            var itemNhanThoiKhongBag = Character.CharacterHandler.GetItemBagById(992);
                            var itemNhanThoiKhongBox = Character.CharacterHandler.GetItemBoxById(992);
                            if (itemNhanThoiKhongBag != null || itemNhanThoiKhongBox != null)
                            {
                                SendMessage(Service.ServerMessage("Bạn đã có Nhẫn thời không sai lệch, không thể nhặt thêm"));
                                return;
                            }
                            SendMessage(Service.ItemMapMePick(itemMap.Id, itemNew.Quantity, TextServer.gI().EMPTY));
                            if(AddItemToBag(false, itemNew, "Nhặt từ map")) SendMessage(Service.SendBag(Character));
                            else return;
                            break;
                        }
                        case 1049://trứng linh thú
                        {
                            var timeServer = ServerUtils.CurrentTimeSecond();
                            var expireHours = 12;
                            var expireTime = timeServer + (expireHours*3600);
                            itemNew.Options.Add(new OptionItem()
                            {
                                Id = 211,
                                Param = expireHours,
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
                            SendMessage(Service.ItemMapMePick(itemMap.Id, itemNew.Quantity, TextServer.gI().EMPTY));
                            if(AddItemToBag(false, itemNew, "Nhặt từ map")) SendMessage(Service.SendBag(Character));
                            else return;

                            break;
                        }
                        default:
                        {
                            var itemTemplate = ItemCache.ItemTemplate(itemNew.Id);
                            var text = TextServer.gI().EMPTY;
                            switch (itemTemplate.Type)
                            {
                                case 9:
                                {
                                    Character.PlusGold(itemNew.Quantity);
                                    // Character.InfoChar.Gold += itemNew.Quantity;
                                    SendMessage(Service.MeLoadInfo(Character));
                                    if (itemNew.Quantity > 32767)
                                    {
                                        text = "Bạn nhặt được " + ServerUtils.GetMoney(itemNew.Quantity) + " vàng";
                                    }
                                    break;
                                }
                                case 10: {
                                    Character.PlusDiamond(itemNew.Quantity);
                                    
                                    // Character.InfoChar.Diamond += itemNew.Quantity;
                                    SendMessage(Service.MeLoadInfo(Character));
                                    if (itemNew.Quantity > 32767)
                                    {
                                        text = "Bạn nhặt được " + ServerUtils.GetMoney(itemNew.Quantity) + " ngọc";
                                    }
                                    break;
                                }
                                case 34: {
                                    Character.PlusDiamondLock(itemNew.Quantity);
                                    // Character.InfoChar.DiamondLock += itemNew.Quantity;
                                    SendMessage(Service.MeLoadInfo(Character));
                                    if (itemNew.Quantity > 32767)
                                    {
                                        text = "Bạn nhặt được " + ServerUtils.GetMoney(itemNew.Quantity) + " ruby";
                                    }
                                    break;
                                }
                                default: {
                                    if (itemTemplate.IsTypeBody() && itemTemplate.Require <= Character.InfoChar.Power && Character.ItemBody[itemTemplate.Type] == null
                                    && (itemTemplate.Gender == 3 || itemTemplate.Gender == Character.InfoChar.Gender))
                                    {
                                       AddItemToBody(itemNew, itemTemplate.Type);
                                       UpdateInfo();
                                    }
                                    else
                                    {
                                        // if(Character.LengthBagNull() < 1) {
                                        //     SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_BAG));
                                        //     return;
                                        // }
                                        if(AddItemToBag(true, itemNew, "Nhặt từ map")) SendMessage(Service.SendBag(Character));
                                        else return;
                                    }
                                    break;
                                }  
                            }
                            SendMessage(Service.ItemMapMePick(itemMap.Id, itemNew.Quantity, text));
                            break;
                        }
                    }
                    // zone.ZoneHandler.SendMessage(Service.ItemMapPlayerPick(itemMap.Id, Character.Id), Character.Id);
                    zone.ZoneHandler.RemoveItemMap(itemMap.Id);
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Send Handshake Message in Service.cs: {e.Message} \n {e.StackTrace}", e);
            }
        }

        public void LeaveItem(ICharacter character)
        {
            // Ignore
        }

        public void Dispose()
        {
            SuppressFinalize(this);
        }
    }
}
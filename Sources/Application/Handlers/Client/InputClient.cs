using System;
using System.Collections.Generic;
using NRO_Server.Main.Menu;
using NRO_Server.Application.Constants;
using NRO_Server.Application.IO;
using NRO_Server.Application.Main;
using NRO_Server.Application.Threading;
using NRO_Server.Application.Manager;
using NRO_Server.DatabaseManager;
using NRO_Server.DatabaseManager.Player;
using NRO_Server.Model.Template;

namespace NRO_Server.Application.Handlers.Client
{
    public static class InputClient
    {
        public static void HanleInputClient(Model.Character.Character character, Message message)
        {
            if(message == null) return;
            try
            {
                var lengthInput = message.Reader.ReadByte();
                var listInput = new List<string>();
                for (var i = 0; i < lengthInput; i++)
                {
                    listInput.Add(message.Reader.ReadUTF());
                }
                if(listInput.Count <= 0) return;
                switch (character.TypeInput)
                {
                    case 0://Nạp thẻ
                    {
                        var soSeriText = listInput[0];
                        var maPinText = listInput[1];

                        Console.WriteLine("Loai the " + character.NapTheTemp.LoaiThe + " menh gia " + character.NapTheTemp.MenhGia + " So Seri " + soSeriText + " ma pin " + maPinText);
                        GachThe.SendCard(character, character.NapTheTemp.LoaiThe, character.NapTheTemp.MenhGia, soSeriText, maPinText);
                        break;
                    }
                    case 1://Gift code 
                    {
                        var codeInput = listInput[0];
                        Giftcode.HandleUseGiftcode(character, listInput[0]);
                        break;
                    }
                    case 2://đổi mật khẩu
                    {
                        var timeServer = ServerUtils.CurrentTimeMillis();
                        character.Delay.UseGiftCode = timeServer + 30000;
                        var oldPass = listInput[0];
                        var newPass = listInput[1];
                        // var sdt = listInput[2];
                        var checkData = UserDB.CheckBeforeChangePass(character.Player.Id, oldPass);
                        if (!checkData)
                        {
                            character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, "Thông tin tài khoản không chính xác, vui lòng nhập lại."));
                            return;
                        }
                        UserDB.DoiMatKhau(character.Player.Id, newPass);
                        character.CharacterHandler.SendMessage(Service.OpenUiSay((short)character.ShopId, "Đổi mật khẩu thành công, vui lòng thoát game và đăng nhập lại"));
                        break;
                    }
                    case 3: //khoa tai khoan
                    {
                        var tenNhanVat = listInput[0];
                        var banReason = listInput[1];
                        var @char = (Model.Character.Character) ClientManager.Gi().GetCharacter(tenNhanVat);
                        if (@char != null)
                        {
                            UserDB.BanUser(@char.Player.Id);
                            ClientManager.Gi().SendMessageCharacter(Service.ServerChat("Nhân vật " + tenNhanVat + " đã bị khóa tài khoản với lý do: " + banReason));
                            ClientManager.Gi().KickSession(@char.Player.Session);
                        }
                        else 
                        {
                            character.CharacterHandler.SendMessage(Service.DialogMessage("Không tìm thấy tên nhân vật này đang online"));
                        }
                        Console.WriteLine("ten TK: " + tenNhanVat + " ly do ban: " + banReason);
                        break;
                    }
                    case 1999: //đổi vnd sang vàng
                    {
                        Console.WriteLine(listInput[0]);
                        // kiểm tra có phải là số không
                        int n;
                        bool isNumeric = int.TryParse(listInput[0], out n);
                        if (!isNumeric) 
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().INPUT_CORRECT_NUMBER));
                            return;
                        }
                        var inputValue = Int32.Parse(listInput[0]);

                        if (inputValue < 0)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().INPUT_CORRECT_NUMBER));
                            return;
                        }
                        // Kiểm tra có đủ VNĐ không
                        int vnd = UserDB.GetVND(character.Player);
                        if (vnd < inputValue)
                        {
                            character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().NOT_ENOUGH_VND));
                            return;
                        }
                        // Kiểm tra giới hạn vàng trên người
                        long quyDoi = inputValue*550;
                        if (character.InfoChar.Gold + quyDoi > character.InfoChar.LimitGold)
                        {
                            var quyDoiToiDa = (character.InfoChar.LimitGold - character.InfoChar.Gold)/550;
                            character.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().VND_TO_GOLD_LIMIT, ServerUtils.GetMoneys(quyDoiToiDa))));
                            return;
                        }
                        // Oke hết thì trừ VNĐ và cộng vàng
                        if (UserDB.MineVND(character.Player, inputValue))
                        {
                            character.PlusGold(quyDoi);
                            character.CharacterHandler.SendMessage(Service.MeLoadInfo(character));

                            if (inputValue >= 20000 && !character.InfoChar.IsPremium)
                            {
                                character.InfoChar.IsPremium = true;
                                character.CharacterHandler.SendMessage(Service.ServerMessage(TextServer.gI().UPGRADE_TO_PREMIUM));
                            }
                        }
                        character.TypeInput = 0;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error HanleInputClient in Service.cs: {e.Message} \n {e.StackTrace}", e);
            }
            finally
            {
                message?.CleanUp();
            }
        }

        public static void HandleNapThe(Model.Character.Character character, Message message)
        {
            var gender = character.InfoChar.Gender;
            character.CharacterHandler.SendMessage(Service.OpenUiSay(5, string.Format("Hãy đến gặp {0} để nạp thẻ bạn nhé.", TextTask.NameNpc[gender]), false, gender));
        }
    }
}
using System.Linq;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.IO;
using NRO_Server.Application.Main;
using NRO_Server.Application.Manager;
using NRO_Server.DatabaseManager.Player;
using NRO_Server.Model.Clan;
using NRO_Server.Model.Info;

namespace NRO_Server.Application.Handlers.Character
{
    public class ClanHandler : IClanHandler
    {
        public Clan Clan { get; set; }

        public ClanHandler(Clan clan)
        {
            Clan = clan;
        }

        public void Update(int id)
        {
            lock (Clan)
            {
                switch (id)
                {
                    case 0:
                    {
                        Clan.Messages.Clear();
                        SendUpdateClan();
                        Flush();
                        break;
                    }
                    //Check Pea
                    case 1:
                    {
                        if (Clan.CharacterPeas.Count > 0)
                        {
                            Clan.CharacterPeas.ToList().ForEach(cp =>
                            {
                                if (Clan.Members.FirstOrDefault(check => check.Id == cp.PlayerRevice) != null) return;
                                var mem = ClientManager.Gi().GetCharacter(cp.PlayerRevice);
                                if (mem == null) return;
                                var itemNew = ItemCache.GetItemDefault((short)cp.PeaId);
                                itemNew.Quantity = cp.Quantity;
                                if (mem.CharacterHandler.AddItemToBag(true, itemNew, "Nhận đậu từ clan"))
                                {
                                    mem.CharacterHandler.SendMessage(Service.SendBag(mem));
                                };
                                mem.CharacterHandler.SendMessage(Service.ServerMessage(string.Format(TextServer.gI().RECEIVE_PEA_CLAN, ItemCache.ItemTemplate(itemNew.Id).Name, cp.PlayerGive)));
                                Clan.CharacterPeas.Remove(cp);
                            });
                        }
                        break;
                    }
                    //Update clan
                    case 2:
                    {
                        SendUpdateClan();
                        break;
                    }
                }
            }
        }

        public void Flush()
        {
            ClanDB.Update(Clan);
        }

        public bool AddMember(Model.Character.Character character, int role = 0, bool isFlush = true)
        {

            lock (Clan.Members)
            {
                if (Clan.Members.FirstOrDefault(m => m.Id == character.Id) == null)
                {
                    var member = new ClanMember()
                    {
                        Id = character.Id,
                        Name = character.Name,
                        Head = character.GetHead(false),
                        Leg = character.GetLeg(false),
                        Body = character.GetBody(false),
                        Role = role,
                        Power = character.InfoChar.Power,
                        Donate = 0,
                        ReceiveDonate = 0,
                        ClanPoint = 0,
                        CurClanPoint = 0,
                        LastRequest = 0,
                        JoinTime = ServerUtils.CurrentTimeSecond()
                    };
                    Clan.Members.Add(member);
                    Clan.CurrMember++;
                    SendMessage(Service.AddMemberClan(member));
                    if(isFlush) Flush();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void AddCharacterPea(CharacterPea characterPea)
        {
            lock (Clan.CharacterPeas)
            {
                var check = Clan.CharacterPeas.FirstOrDefault(c =>
                    c.PlayerRevice == characterPea.PlayerRevice && c.PlayerGive.Equals(characterPea.PlayerGive) && c.PeaId == characterPea.PeaId);
                if (check == null)
                {
                    Clan.CharacterPeas.Add(characterPea);
                }
                else
                {
                    check.Quantity += characterPea.Quantity;
                }
            }
        }

        public bool RemoveMember(int id)
        {
            lock (Clan.Members)
            {
                var mem = Clan.Members.FirstOrDefault(m => m.Id == id);
                if (mem != null)
                {
                    var index = Clan.Members.IndexOf(mem);
                    Clan.Members.RemoveAt(index);
                    Clan.CurrMember-=1;
                    SendMessage(Service.RemoveMemberClan(index));
                    Flush();
                    return true;
                }
                return false;
            }
        }

        public ClanMember GetMember(int id)
        {
            return Clan.Members.FirstOrDefault(member => member.Id == id);
        }

        public ClanMessage GetMessage(int id)
        {
            return Clan.Messages.FirstOrDefault(message => message.Id == id);
        }

        public void SendMessage(Message message)
        {
            lock (Clan.Members)
            {
                Clan.Members.ToList().ForEach(member =>
                {
                    var charMem = (Model.Character.Character)ClientManager.Gi().GetCharacter(member.Id);
                    charMem?.CharacterHandler.SendMessage(message);
                });
            }
        }

        public void UpdateClanId()
        {
            lock (Clan.Members)
            {
                Clan.Members.ToList().ForEach(member =>
                {
                    var charMem = (Model.Character.Character)ClientManager.Gi().GetCharacter(member.Id);
                    charMem?.CharacterHandler.SendZoneMessage(Service.UpdateClanId(charMem.Id, Clan.Id));
                });
            }
        }

        public void SendUpdateClan()
        {
            lock (Clan.Members)
            {
                Clan.Members.ToList().ForEach(member =>
                {
                    var charMem = (Model.Character.Character)ClientManager.Gi().GetCharacter(member.Id);
                    if (charMem != null)
                    {
                        member.Head = charMem.GetHead(false);
                        member.Body = charMem.GetBody(false);
                        member.Leg = charMem.GetLeg(false);
                        member.Power = charMem.InfoChar.Power;
                        charMem.CharacterHandler.SendMessage(Service.MyClanInfo(charMem));
                    }
                });
            }
        }

        public void Chat(ClanMessage message)
        {
            lock (Clan.Messages)
            {
                var check = Clan.Messages.FirstOrDefault(msg => msg.Id == message.Id);
                if (check != null)
                {
                    var index = Clan.Messages.IndexOf(check);
                    Clan.Messages.RemoveAt(index);
                    if (message.Recieve < message.MaxCap)
                    {
                        Clan.Messages.Insert(Clan.Messages.Count, message);
                    }
                }   
                else
                {
                    Clan.Messages.Add(message);
                    if (Clan.Messages.Count < 4)
                    {
                        SendUpdateClan();
                    }
                    else if(Clan.Messages.Count > 15)
                    {
                        var list = Clan.Messages.FirstOrDefault(msg => msg.Type != 1);
                        Clan.Messages.Remove(list);
                    }
                }
                SendMessage(Service.ClanMessage(message));
            }
        }
    }
}
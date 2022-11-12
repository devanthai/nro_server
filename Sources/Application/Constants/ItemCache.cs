using System;
using System.Linq;
using NRO_Server.DatabaseManager;
using NRO_Server.Model.Item;
using NRO_Server.Model.Template;
using NRO_Server.Application.Constants;
using NRO_Server.Application.Interfaces.Map;
using NRO_Server.Application.Handlers.Item;

namespace NRO_Server.Application.Constants
{
    public static class ItemCache
    {
        public static Item GetItemDefault(short id, int quantity = 1)
        {
            var itemTemplate = Cache.Gi().ITEM_TEMPLATES.Values.FirstOrDefault(item => item.Id == id);
            if (itemTemplate == null) return null;
            var item = new Item();
            item.Id = itemTemplate.Id;
            item.Quantity = quantity;
            item.BuyPotential = 0;
            item.SaleCoin = itemTemplate.SaleCoin;
            item.Options.AddRange(itemTemplate.Options.ToList());
            return ItemHandler.Clone(item);
        }

        public static bool IsTypeBody(short id)
        {
            var itemTemplate = ItemTemplate(id);
            return itemTemplate.Type is < 6 or 32;
        }

        public static bool IsPetItem(short id)
        {
            switch(id)
            {
                case 892 : return true;//Thỏ xám
                case 893 : return true;//Thỏ trắng
                case 908 : return true;//Ma phong ba
                case 909 : return true;//Thần chết cute
                case 910 : return true;//Bí ngô nhí nhảnh
                case 916 : return true;//Lính Tam Giác
                case 917 : return true;//lính vuông
                case 918 : return true;//lính tròn
                case 919 : return true;//búp bê
                case 936 : return true;//tuần lộc nhí
                case 942 : return true;//hổ mặp vàng
                case 943 : return true;//hổ mặp trắng
                case 944 : return true;//hỏ mặp xanh
                case 967 : return true;//sao la
                case 1008 : return true;//cua đỏ
                case 1039 : return true;//Thỏ ốm
                case 1040 : return true;//Thỏ mập
                case 1046 : return true;//Khỉ bong bóng
                default: return false;
            }
        }

        public static bool IsSpecialAmountItem(short id)
        {
            switch(id)
            {
                case 933:
                {
                    return true;
                }
                default: return false;
            }
        }

        public static bool IsUnlimitItem(short id)
        {
            switch(id)
            {
                case 457:
                {
                    return true;
                }
                default: return false;
            }
        }

        public static bool IsItemSellOnlyOne(short id)
        {
            switch(id)
            {
                case 457:
                {
                    return true;
                }
                default: return false;
            }
        }

        public static ItemTemplate ItemTemplate(short id)
        {
            return Cache.Gi().ITEM_TEMPLATES.Values.FirstOrDefault(item => item.Id == id);
        }

        public static ItemOptionTemplate ItemOptionTemplate(int id)
        {
            return Cache.Gi().ITEM_OPTION_TEMPLATES.FirstOrDefault(option => option.Id == id);
        }

        public static bool IsItemAvtNotPart(int id)
        {
            return DataCache.AvatarNotPart.Contains(id);
        }
        
        public static bool IsItemAvtNotHead(int id)
        {
            return DataCache.AvatarNotHead.Contains(id);
        }
        
        public static short PartNotAvatar(int id) {
            switch (id) {
                case 282: {
                    return 98;
                }
                case 283: {
                    return 77;
                }
                case 285: {
                    return 89;
                }
                case 286: {
                    return 86;
                }
                case 287: {
                    return 83;
                }
                case 288: {
                    return 180;
                }
                case 289: {
                    return 162;
                }
                case 291: {
                    return 123;
                }
                case 290:
                case 431: {
                    return 171;
                }
                case 292:
                case 430: {
                    return 174;
                }
            }
            return -1;
        }
        
        public static short PartHeadToBody(short partHead) {
            switch (partHead)
            {
                case >= 192 and <= 200:
                    return 193;
                case 309 or 310:
                    return 307;
                case 460 or 461:
                    return 458;
                case >= 526 and <= 529:
                    return 525;
                case 536:
                    return 476;
                case 538 or 539 or 542 or 543:
                    return 474;
            }

            switch (partHead)
            {
                case 543:
                    return 523;
                case 545 or 546:
                    return 548;
                case 553:
                    return 555;
                case 569:
                    return 472;
                case 808 or 809 or 810:
                    return 806;
                case 831 or 832:
                    return 829;
                case 836 or 837:
                    return 834;
                case 906:
                    return 880;
            }

            if(!IsItemAvtNotPart(partHead))
                return (short)(partHead + 1);
            return -1;
        }
        
        public static short PartHeadToLeg(short partHead) {
            switch (partHead)
            {
                case >= 192 and <= 200:
                    return 194;
                case 309 or 310:
                    return 308;
                case 460 or 461:
                    return 459;
                case >= 526 and <= 529 or 543:
                    return 524;
                case 536:
                    return 477;
                case 538 or 539 or 542 or 543:
                    return 475;
                case 545 or 546:
                    return 549;
                case 553:
                    return 556;
                case 569:
                    return 473;
                case 808 or 809 or 810:
                    return 807;
                case 831 or 832:
                    return 830;
                case 836 or 837:
                    return 835;
                case 906:
                    return 881;
            }

            if(!IsItemAvtNotPart(partHead))
                return (short)(partHead + 2);
            return -1;
        }

        #region Giap Luyen Tap
        public static bool ItemIsGiapLuyenTap(int itemId) {
            return ((itemId >= 529 && itemId <= 531) || (itemId >= 534 && itemId <= 536));
        }

        public static int GetGiapLuyenTapLevel(int itemId)
        {
            switch (itemId)
            {
                case 529:
                case 534:
                {
                    return 1;
                }
                case 530:
                case 535:
                {
                    return 2;
                }
                case 531:
                case 536:
                {
                    return 3;
                }
            }
            return 1;
        }

        public static int GetGiapLuyenTapPTSucManh(int itemId)
        {
            switch (itemId)
            {
                case 529:
                case 534:
                {
                    return 10;
                }
                case 530:
                case 535:
                {
                    return 20;
                }
                case 531:
                case 536:
                {
                    return 30;
                }
            }
            return 10;
        }

        public static int GetGiapLuyenTapLimit(int itemId)
        {
            switch (itemId)
            {
                case 529:
                case 534:
                {
                    return 100;
                }
                case 530:
                case 535:
                {
                    return 1000;
                }
                case 531:
                case 536:
                {
                    return 10000;
                }
            }
            return 100;
        }
        #endregion

    }
}
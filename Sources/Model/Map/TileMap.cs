using System;
using System.Collections.Generic;
using System.Text;
using NRO_Server.Application.IO;
using NRO_Server.DatabaseManager;
using NRO_Server.Model.Monster;

namespace NRO_Server.Model.Map
{
    public class TileMap
    {
        public int Tmw { get; set; }
        public int Tmh { get; set; }
        public int Pxw { get; set; }
        public int Pxh { get; set; }
        public int[] Maps { get; set; }
        public int[] Types { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int TileID{ get; set; }
        public int Type { get; set; }
        public bool IsMapDouble { get; set; }
        public byte PlanetID { get; set; }
        public int BgID { get; set; }
        public int BgType { get; set; }
        public int Teleport { get; set; }
        public short X0 { get; set; }
        public short Y0 { get; set; }
        public int MaxPlayers { get; set; }
        public int ZoneNumbers { get; set; }
        public List<Npc> Npcs { get; set; }
        public List<WayPoint> WayPoints { get; set; }
        public List<MonsterMap> MonsterMaps { get; set; }
        public List<BackgroundItem> BackgroundItems { get; set; }
        public List<ActionItem> Actions { get; set; }

        public TileMap()
        {
            Npcs = new List<Npc>();
            WayPoints = new List<WayPoint>();
            MonsterMaps = new List<MonsterMap>();
            BackgroundItems = new List<BackgroundItem>();
            Actions = new List<ActionItem>();
        }

        public void LoadMapFromResource()
        {
            var path = ServerUtils.ProjectDir(string.Format(Manager.gI().MapResource, Id));
            var bytes = ServerUtils.ReadFileBytes(path);
            var reader = new MyReader(ServerUtils.ConvertArrayByteToSByte(bytes));
            Tmw = ServerUtils.UShort(reader.ReadUnsignedByte());
            Tmh = ServerUtils.UShort(reader.ReadUnsignedByte());
            Maps = Id == 78 ? new int[Tmw*Tmh] : new int[reader.Available()];

            for (var i = 0; i < Tmw * Tmh; i++)
            {
                Maps[i] = reader.Read();
            }
            Types = new int[Maps.Length];
            LoadMap(TileID);
            reader.Close();
        }

        private void LoadMap(int tileId) {
            Pxh = Tmh * 24;
            Pxw = Tmw * 24;
            var num = tileId - 1;
            try {
                for (var i = 0; i < Tmw * Tmh; i++) {
                    for (var j = 0; j < Cache.Gi().TILE_TYPES[num].Length; j++) {
                        SetTile(i, Cache.Gi().TILE_INDEXS[num][j], Cache.Gi().TILE_TYPES[num][j]);
                    }
                }
            } catch (Exception e) {
                Console.WriteLine($"Check ------------------------------------- error at: TileMap {e.Message}:\n{e.StackTrace}");
            }
        }

        private void SetTile(int index, IReadOnlyList<int> mapsArr, int type)
        {
            for (var i = 0; i < (int)mapsArr.Count; i++)
            {
                if (Maps[index] != mapsArr[i]) continue;
                Types[index] |= type;
                return;
            }
        }

        public short TouchY(short x, short y) {
            var yOld = y;

            while (y < Pxh) {
                if (TileTypeAt(x, y, 2))
                    return y;
                y = (short)(y + 1);

            }
            if((short)Pxh != 0) {
                return (short)Pxh;
            }
            return (short)(yOld+24);
        }

        private bool TileTypeAt(int px, int py, int t) {
            bool result;
            try {
                result = ((Types[py / 24 * Tmw + px / 24] & t) == t);
            } catch (Exception) {
                result = false;
            }
            return result;
        }
    }
}
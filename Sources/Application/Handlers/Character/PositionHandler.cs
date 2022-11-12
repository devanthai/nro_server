using System;
using NRO_Server.Application.Interfaces.Character;

namespace NRO_Server.Application.Handlers.Character
{
    public class PositionHandler
    {
        public PositionHandler()
        {

        }

        private static void SetupPositionXy(ICharacter character, short x, short y)
        {
            character.InfoChar.X = x;
            character.InfoChar.Y = y;
        }

        public static void SetUpPosition(ICharacter character, int oldMap, int newMap)
        {
            switch (oldMap)
            {
                case 0:
                {
                    switch (newMap)
                    {
                        case 1:
                        {
                            SetupPositionXy(character, 35, 384);
                            break;
                        }
                        case 42:
                        {
                            SetupPositionXy(character, 1405, 432);
                            break;
                        }
                    }

                    break;
                }
                case 1:
                {
                    switch (newMap)
                    {
                        case 0:
                        {
                            SetupPositionXy(character, 1213, 432);
                            break;
                        }
                        case 2:
                        {
                            SetupPositionXy(character, 35, 360);
                            break;
                        }
                        case 47:
                        {
                            SetupPositionXy(character, 35, 336);
                            break;
                        }
                    }

                    break;
                }
                case 2:
                {
                    switch (newMap)
                    {
                        case 1:
                        {
                            SetupPositionXy(character, 1500, 360);
                            break;
                        }
                        case 3:
                        {
                            SetupPositionXy(character, 35, 408);
                            break;
                        }
                        case 24:
                        {
                            SetupPositionXy(character, 300, 336);
                            break;
                        }
                    }

                    break;
                }
                case 3:
                {
                    switch (newMap)
                    {
                        case 2:
                        {
                            SetupPositionXy(character, 1213, 288);
                            break;
                        }
                        case 4:
                        {
                            SetupPositionXy(character, 35, 312);
                            break;
                        }
                        case 27:
                        {
                            SetupPositionXy(character, 35, 192);
                            break;
                        }
                    }

                    break;
                }
                case 4:
                {
                    switch (newMap)
                    {
                        case 3:
                        {
                            SetupPositionXy(character, 1595, 336);
                            break;
                        }
                        case 5:
                        {
                            SetupPositionXy(character, 35, 288);
                            break;
                        }
                    }

                    break;
                }
                case 5:
                {
                    switch (newMap)
                    {
                        case 4:
                        {
                            SetupPositionXy(character, 1595, 336);
                            break;
                        }
                        case 6:
                        {
                            SetupPositionXy(character, 35, 336);
                            break;
                        }
                        case 29:
                        {
                            SetupPositionXy(character, 735, 360);
                            break;
                        }
                    }

                    break;
                }
                case 6:
                {
                    switch (newMap)
                    {
                        case 5:
                        {
                            SetupPositionXy(character, 1693, 288);
                            break;
                        }
                    }

                    break;
                }
                case 7:
                {
                    switch (newMap)
                    {
                        case 8:
                        {
                            SetupPositionXy(character, 56, 432);
                            break;
                        }
                        case 22:
                        {
                            SetupPositionXy(character, 187, 336);
                            break;
                        }
                        case 43:
                        {
                            SetupPositionXy(character, 1390, 432);
                            break;
                        }
                    }

                    break;
                }
                case 8:
                {
                    switch (newMap)
                    {
                        case 7:
                        {
                            SetupPositionXy(character, 1159, 432);
                            break;
                        }
                        case 9:
                        {
                            SetupPositionXy(character, 61, 216);
                            break;
                        }
                    }

                    break;

                }
                case 9:
                {
                    switch (newMap)
                    {
                        case 8:
                        {
                            SetupPositionXy(character, 1175, 408);
                            break;
                        }
                        case 11:
                        {
                            SetupPositionXy(character, 35, 384);
                            break;
                        }
                        case 25:
                        {
                            SetupPositionXy(character, 232, 336);
                            break;
                        }
                    }

                    break;
                }
                case 10:
                {
                    switch (newMap)
                    {
                        case 13:
                        {
                            SetupPositionXy(character, 1668, 336);
                            break;
                        }
                    }

                    break;
                }
                case 11:
                {
                    switch (newMap)
                    {
                        case 9:
                        {
                            SetupPositionXy(character, 1194, 264);
                            break;
                        }
                        case 12:
                        {
                            SetupPositionXy(character, 48, 408);
                            break;
                        }
                        case 31:
                        {
                            SetupPositionXy(character, 35, 240);
                            break;
                        }
                    }

                    break;
                }
                case 12:
                {
                    switch (newMap)
                    {
                        case 11:
                        {
                            SetupPositionXy(character, 1508, 312);
                            break;
                        }
                        case 13:
                        {
                            SetupPositionXy(character, 43, 264);
                            break;
                        }
                    }

                    break;
                }
                case 13:
                {
                    switch (newMap)
                    {
                        case 12:
                        {
                            SetupPositionXy(character, 1520, 384);
                            break;
                        }
                        case 10:
                        {
                            SetupPositionXy(character, 68, 288);
                            break;
                        }
                        case 33:
                        {
                            SetupPositionXy(character, 629, 360);
                            break;
                        }
                    }

                    break;
                }
                case 14:
                {
                    switch (newMap)
                    {
                        case 15:
                        {
                            SetupPositionXy(character, 35, 408);
                            break;
                        }
                        case 23:
                        {
                            SetupPositionXy(character, 430, 336);
                            break;
                        }
                        case 44:
                        {
                            SetupPositionXy(character, 1405, 432);
                            break;
                        }
                    }

                    break;
                }
                case 15:
                {
                    switch (newMap)
                    {
                        case 14:
                        {
                            SetupPositionXy(character, 1213, 408);
                            break;
                        }
                        case 16:
                        {
                            SetupPositionXy(character, 35, 288);
                            break;
                        }
                    }

                    break;
                }
                case 16:
                {
                    switch (newMap)
                    {
                        case 15:
                        {
                            SetupPositionXy(character, 1213, 264);
                            break;
                        }
                        case 17:
                        {
                            SetupPositionXy(character, 35, 312);
                            break;
                        }
                        case 26:
                        {
                            SetupPositionXy(character, 455, 336);
                            break;
                        }
                    }

                    break;
                }
                case 17:
                {
                    switch (newMap)
                    {
                        case 16:
                        {
                            SetupPositionXy(character, 1213, 240);
                            break;
                        }
                        case 18:
                        {
                            SetupPositionXy(character, 35, 408);
                            break;
                        }
                        case 35:
                        {
                            SetupPositionXy(character, 35, 216);
                            break;
                        }
                    }

                    break;
                }
                case 18:
                {
                    switch (newMap)
                    {
                        case 17:
                        {
                            SetupPositionXy(character, 1549, 312);
                            break;
                        }
                        case 20:
                        {
                            SetupPositionXy(character, 35, 288);
                            break;
                        }
                    }

                    break;
                }
                case 19:
                {
                    switch (newMap)
                    {
                        case 20:
                        {
                            SetupPositionXy(character, 1645, 288);
                            break;
                        }
                    }

                    break;
                }
                case 20:
                {
                    switch (newMap)
                    {
                        case 18:
                        {
                            SetupPositionXy(character, 1549, 432);
                            break;
                        }
                        case 19:
                        {
                            SetupPositionXy(character, 35, 360);
                            break;
                        }
                        case 37:
                        {
                            SetupPositionXy(character, 680, 384);
                            break;
                        }
                    }

                    break;
                }
                case 21:
                {
                    switch (newMap)
                    {
                        case 0:
                        {
                            SetupPositionXy(character, 330, 432);
                            break;
                        }
                    }

                    break;
                }
                case 22:
                {
                    switch (newMap)
                    {
                        case 7:
                        {
                            SetupPositionXy(character, 395, 432);
                            break;
                        }
                    }

                    break;
                }
                case 23:
                {
                    switch (newMap)
                    {
                        case 14:
                        {
                            SetupPositionXy(character, 552, 408);
                            break;
                        }
                    }

                    break;

                }
                case 24:
                {
                    switch (newMap)
                    {
                        case 2:
                        {
                            SetupPositionXy(character, 530, 385);
                            break;
                        }
                    }

                    break;
                }
                case 25:
                {
                    switch (newMap)
                    {
                        case 9:
                        {
                            SetupPositionXy(character, 728, 456);
                            break;
                        }
                    }

                    break;

                }
                case 26:
                {
                    switch (newMap)
                    {
                        case 16:
                        {
                            SetupPositionXy(character, 295, 288);
                            break;
                        }
                    }

                    break;
                }
                case 27:
                {
                    switch (newMap)
                    {
                        case 3:
                        {
                            SetupPositionXy(character, 1375, 336);
                            break;
                        }
                        case 28:
                        {
                            SetupPositionXy(character, 35, 360);
                            break;
                        }
                    }

                    break;
                }
                case 28:
                {
                    switch (newMap)
                    {
                        case 27:
                        {
                            SetupPositionXy(character, 1735, 336);
                            break;
                        }
                        case 29:
                        {
                            SetupPositionXy(character, 35, 168);
                            break;
                        }
                    }

                    break;
                }
                case 29:
                {
                    switch (newMap)
                    {
                        case 5:
                        {
                            SetupPositionXy(character, 880, 408);
                            break;
                        }
                        case 28:
                        {
                            SetupPositionXy(character, 1405, 168);
                            break;
                        }
                        case 30:
                        {
                            SetupPositionXy(character, 35, 312);
                            break;
                        }
                    }

                    break;
                }
                case 30:
                {
                    switch (newMap)
                    {
                        case 29:
                        {
                            SetupPositionXy(character, 1525, 360);
                            break;
                        }
                    }

                    break;
                }
                case 31:
                {
                    switch (newMap)
                    {
                        case 11:
                        {
                            SetupPositionXy(character, 1280, 384);
                            break;
                        }
                        case 32:
                        {
                            SetupPositionXy(character, 55, 432);
                            break;
                        }
                    }

                    break;
                }

                // 31 1285 384
                case 32:
                {
                    switch (newMap)
                    {
                        case 33:
                        {
                            SetupPositionXy(character, 57, 216);
                            break;
                        }
                        case 31:
                        {
                            SetupPositionXy(character, 1285, 384);
                            break;
                        }
                    }

                    break;
                }
                case 33:
                {
                    switch (newMap)
                    {
                        case 13:
                        {
                            SetupPositionXy(character, 378, 384);
                            break;
                        }
                        case 32:
                        {
                            SetupPositionXy(character, 1250, 288);
                            break;
                        }
                        case 34:
                        {
                            SetupPositionXy(character, 61, 312);
                            break;
                        }
                    }

                    break;
                }
                case 34:
                {
                    switch (newMap)
                    {
                        case 33:
                        {
                            SetupPositionXy(character, 1615, 360);
                            break;
                        }

                    }

                    break;
                }

                case 35:
                {
                    switch (newMap)
                    {
                        case 17:
                        {
                            SetupPositionXy(character, 1320, 312);
                            break;
                        }
                        case 36:
                        {
                            SetupPositionXy(character, 35, 456);
                            break;
                        }
                    }

                    break;
                }
                case 36:
                {
                    switch (newMap)
                    {
                        case 35:
                        {
                            SetupPositionXy(character, 1285, 336);
                            break;
                        }
                        case 37:
                        {
                            SetupPositionXy(character, 35, 192);
                            break;
                        }
                    }

                    break;
                }
                case 37:
                {
                    switch (newMap)
                    {
                        case 20:
                        {
                            SetupPositionXy(character, 1190, 360);
                            break;
                        }
                        case 36:
                        {
                            SetupPositionXy(character, 1285, 168);
                            break;
                        }
                        case 38:
                        {
                            SetupPositionXy(character, 35, 456);
                            break;
                        }
                    }

                    break;
                }
                case 38:
                {
                    switch (newMap)
                    {
                        case 37:
                        {
                            SetupPositionXy(character, 1500, 216);
                            break;
                        }
                    }

                    break;
                }
                case 39:
                case 40:
                case 41:
                {
                    switch (newMap)
                    {
                        case 21:
                        case 22:
                        case 23:
                        {
                            SetupPositionXy(character, 35, 336);
                            break;
                        }
                    }

                    break;
                }
                case 42:
                {
                    switch (newMap)
                    {
                        case 0:
                        {
                            SetupPositionXy(character, 35, 432);
                            break;
                        }
                    }

                    break;
                }
                case 43:
                {
                    switch (newMap)
                    {
                        case 7:
                        {
                            SetupPositionXy(character, 36, 408);
                            break;
                        }
                    }

                    break;
                }

                case 44:
                {
                    switch (newMap)
                    {
                        case 14:
                        {
                            SetupPositionXy(character, 35, 408);
                            break;
                        }
                        case 52:
                        {
                            SetupPositionXy(character, 550, 336);
                            break;
                        }
                    }

                    break;
                }
                case 45:
                {
                    switch (newMap)
                    {
                        case 46:
                        {
                            SetupPositionXy(character, 300, 140);
                            break;
                        }
                        case 48:
                        {
                            SetupPositionXy(character, 357, 240);
                            break;
                        }
                    }

                    break;
                }
                case 46:
                {
                    switch (newMap)
                    {
                        case 45:
                        {
                            SetupPositionXy(character, 300, 500);
                            break;
                        }
                        case 47:
                        {
                            SetupPositionXy(character, 554, 140);
                            break;
                        }
                    }

                    break;
                }
                case 47:
                {
                    switch (newMap)
                    {
                        case 1:
                        {
                            SetupPositionXy(character, 1220, 360);
                            break;
                        }
                        case 46:
                        {
                            SetupPositionXy(character, 300, 500);
                            break;
                        }
                        case 111:
                        {
                            SetupPositionXy(character, 35, 336);
                            break;
                        }
                    }

                    break;
                }
                case 48:
                {
                    switch (newMap)
                    {
                        case 45:
                        {
                            SetupPositionXy(character, 300, 500);
                            break;
                        }
                    }
                    break;
                }
                case 52:
                {
                    switch (newMap)
                    {
                        case 44:
                        {
                            SetupPositionXy(character, 1000, 432);
                            break;
                        }
                    }
                    break;
                }
                case 63:
                {
                    switch (newMap)
                    {
                        case 65:
                        {
                            SetupPositionXy(character, 44, 312);
                            break;
                        }
                        case 66:
                        {
                            SetupPositionXy(character, 1631, 360);
                            break;
                        }
                    }

                    break;
                }
                case 64:
                {
                    switch (newMap)
                    {
                        case 65:
                        {
                            SetupPositionXy(character, 1604, 312);
                            break;
                        }
                        case 72:
                        {
                            SetupPositionXy(character, 75, 312);
                            break;
                        }
                    }

                    break;
                }
                case 65:
                {
                    switch (newMap)
                    {
                        case 64:
                        {
                            SetupPositionXy(character, 60, 312);
                            break;
                        }
                        case 63:
                        {
                            SetupPositionXy(character, 1102, 816);
                            break;
                        }
                    }

                    break;
                }
                case 66:
                {
                    switch (newMap)
                    {
                        case 63:
                        {
                            SetupPositionXy(character, 35, 336);
                            break;
                        }
                        case 67:
                        {
                            SetupPositionXy(character, 1145, 720);
                            break;
                        }
                    }

                    break;
                }
                case 67:
                {
                    switch (newMap)
                    {
                        case 66:
                        {
                            SetupPositionXy(character, 42, 360);
                            break;
                        }
                        case 73:
                        {
                            SetupPositionXy(character, 1066, 504);
                            break;
                        }
                    }

                    break;
                }
                case 68:
                {
                    switch (newMap)
                    {
                        case 69:
                        {
                            SetupPositionXy(character, 35, 192);
                            break;
                        }
                    }

                    break;
                }
                case 69:
                {
                    switch (newMap)
                    {
                        case 68:
                        {
                            SetupPositionXy(character, 1640, 408);
                            break;
                        }
                        case 70:
                        {
                            SetupPositionXy(character, 41, 696);
                            break;
                        }
                    }

                    break;
                }
                case 70:
                {
                    switch (newMap)
                    {
                        case 69:
                        {
                            SetupPositionXy(character, 1640, 408);
                            break;
                        }
                        case 71:
                        {
                            SetupPositionXy(character, 1015, 1080);
                            break;
                        }
                    }

                    break;
                }
                case 71:
                {
                    switch (newMap)
                    {
                        case 70:
                        {
                            SetupPositionXy(character, 34, 192);
                            break;
                        }
                        case 72:
                        {
                            SetupPositionXy(character, 1603, 312);
                            break;
                        }
                    }

                    break;
                }
                case 72:
                {
                    switch (newMap)
                    {
                        case 71:
                        {
                            SetupPositionXy(character, 62, 1056);
                            break;
                        }
                        case 64:
                        {
                            SetupPositionXy(character, 1635, 312);
                            break;
                        }
                    }

                    break;
                }
                case 73:
                {
                    switch (newMap)
                    {
                        case 74:
                        {
                            SetupPositionXy(character, 60, 336);
                            break;
                        }
                        case 67:
                        {
                            SetupPositionXy(character, 46, 336);
                            break;
                        }
                    }

                    break;
                }
                case 74:
                {
                    switch (newMap)
                    {
                        case 73:
                        {
                            SetupPositionXy(character, 1025, 168);
                            break;
                        }
                        case 75:
                        {
                            SetupPositionXy(character, 50, 336);
                            break;
                        }
                    }

                    break;
                }
                case 75:
                {
                    switch (newMap)
                    {
                        case 74:
                        {
                            SetupPositionXy(character, 1630, 336);
                            break;
                        }
                        case 76:
                        {
                            SetupPositionXy(character, 55, 336);
                            break;
                        }
                    }

                    break;
                }
                case 76:
                {
                    switch (newMap)
                    {
                        case 75:
                        {
                            SetupPositionXy(character, 1610, 336);
                            break;
                        }
                        case 77:
                        {
                            SetupPositionXy(character, 50, 216);
                            break;
                        }
                    }

                    break;
                }
                case 77:
                {
                    switch (newMap)
                    {
                        case 76:
                        {
                            SetupPositionXy(character, 1645, 336);
                            break;
                        }
                        case 81:
                        {
                            SetupPositionXy(character, 53, 336);
                            break;
                        }
                    }

                    break;
                }
                case 79:
                {
                    switch (newMap)
                    {
                        case 83:
                        {
                            SetupPositionXy(character, 1634, 336);
                            break;
                        }
                        case 80:
                        {
                            SetupPositionXy(character, 35, 144);
                            break;
                        }
                    }

                    break;
                }
                case 80:
                {
                    switch (newMap)
                    {
                        case 79:
                        {
                            SetupPositionXy(character, 1638, 360);
                            break;
                        }
                        case 105:
                        {
                            SetupPositionXy(character, 35, 408);
                            break;
                        }
                    }

                    break;
                }
                case 81:
                {
                    switch (newMap)
                    {
                        case 82:
                        {
                            SetupPositionXy(character, 46, 384);
                            break;
                        }
                        case 77:
                        {
                            SetupPositionXy(character, 1632, 316);
                            break;
                        }
                    }

                    break;
                }
                case 82:
                {
                    switch (newMap)
                    {
                        case 81:
                        {
                            SetupPositionXy(character, 1388, 336);
                            break;
                        }
                        case 83:
                        {
                            SetupPositionXy(character, 42, 336);
                            break;
                        }
                    }

                    break;
                }
                case 83:
                {
                    switch (newMap)
                    {
                        case 82:
                        {
                            SetupPositionXy(character, 1610, 384);
                            break;
                        }
                        case 79:
                        {
                            SetupPositionXy(character, 62, 360);
                            break;
                        }
                    }

                    break;
                }
                case 92:
                {
                    switch (newMap)
                    {
                        case 102:
                        {
                            SetupPositionXy(character, 652, 360);
                            break;
                        }
                        case 93:
                        {
                            SetupPositionXy(character, 60, 360);
                            break;
                        }
                    }

                    break;
                }
                case 93:
                {
                    switch (newMap)
                    {
                        case 92:
                        {
                            SetupPositionXy(character, 1374, 360);
                            break;
                        }
                        case 94:
                        {
                            SetupPositionXy(character, 85, 384);
                            break;
                        }
                    }

                    break;
                }
                case 94:
                {
                    switch (newMap)
                    {
                        case 93:
                        {
                            SetupPositionXy(character, 1630, 288);
                            break;
                        }
                        case 96:
                        {
                            SetupPositionXy(character, 48, 144);
                            break;
                        }
                    }

                    break;
                }
                case 96:
                {
                    switch (newMap)
                    {
                        case 94:
                        {
                            SetupPositionXy(character, 1592, 384);
                            break;
                        }
                        case 97:
                        {
                            SetupPositionXy(character, 57, 384);
                            break;
                        }
                    }

                    break;
                }
                case 97:
                {
                    switch (newMap)
                    {
                        case 98:
                        {
                            SetupPositionXy(character, 35, 384);
                            break;
                        }
                        case 96:
                        {
                            SetupPositionXy(character, 1607, 168);
                            break;
                        }
                    }

                    break;
                }
                case 98:
                {
                    switch (newMap)
                    {
                        case 97:
                        {
                            SetupPositionXy(character, 1627, 384);
                            break;
                        }
                        case 99:
                        {
                            SetupPositionXy(character, 55, 144);
                            break;
                        }
                    }

                    break;
                }
                case 99:
                {
                    switch (newMap)
                    {
                        case 98:
                        {
                            SetupPositionXy(character, 1600, 384);
                            break;
                        }
                        case 100:
                        {
                            SetupPositionXy(character, 45, 168);
                            break;
                        }
                    }

                    break;
                }
                case 100:
                {
                    switch (newMap)
                    {
                        case 99:
                        {
                            SetupPositionXy(character, 1612, 144);
                            break;
                        }
                        case 103:
                        {
                            SetupPositionXy(character, 59, 312);
                            break;
                        }
                    }

                    break;
                }
                case 103:
                {
                    switch (newMap)
                    {
                        case 100:
                        {
                            SetupPositionXy(character, 1594, 192);
                            break;
                        }
                    }

                    break;
                }
                case 102:
                {
                    switch (newMap)
                    {
                        case 92:
                        {
                            SetupPositionXy(character, 50, 360);
                            break;
                        }
                    }

                    break;
                }
                // cánh đồng tuyết
                case 105:
                {
                    switch (newMap)
                    {
                        case 80:
                        {
                            SetupPositionXy(character, 1645, 144);
                            break;
                        }
                        case 109:
                        {
                            SetupPositionXy(character, 69, 168);
                            break;
                        }
                        case 108:
                        {
                            SetupPositionXy(character, 80, 144);
                            break;
                        }

                    }

                    break;
                }
                //rừng tuyết
                case 106:
                {
                    switch (newMap)
                    {
                        case 107:
                        {
                            SetupPositionXy(character, 35, 144);
                            break;
                        }
                        case 110:
                        {
                            SetupPositionXy(character, 1635, 168);
                            break;
                        }
                        case 109:
                        {
                            SetupPositionXy(character, 86, 432);
                            break;
                        }
                    }

                    break;
                }
                // núi tuyết
                case 107:
                {
                    switch (newMap)
                    {
                        case 106:
                        {
                            SetupPositionXy(character, 1602, 168);
                            break;
                        }
                        case 110:
                        {
                            SetupPositionXy(character, 1611, 432);
                            break;
                        }
                        case 108:
                        {
                            SetupPositionXy(character, 675, 696);
                            break;
                        }


                    }

                    break;
                }
//            dòng sông băng
                case 108:
                {
                    switch (newMap)
                    {
                        case 105:
                        {
                            SetupPositionXy(character, 1592, 192);
                            break;
                        }
                        case 107:
                        {
                            SetupPositionXy(character, 853, 696);
                            break;
                        }
                        case 109:
                        {
                            SetupPositionXy(character, 1620, 432);
                            break;
                        }


                    }

                    break;
                }
                //rừng băng
                case 109:
                {
                    switch (newMap)
                    {
                        case 105:
                        {
                            SetupPositionXy(character, 1593, 408);
                            break;
                        }
                        case 106:
                        {
                            SetupPositionXy(character, 35, 408);
                            break;
                        }

                        case 108:
                        {
                            SetupPositionXy(character, 57, 552);
                            break;
                        }


                    }

                    break;
                }
                //hang băng
                case 110:
                {
                    switch (newMap)
                    {
                        case 106:
                        {
                            SetupPositionXy(character, 1645, 108);
                            break;
                        }
                        case 107:
                        {
                            SetupPositionXy(character, 62, 456);
                            break;
                        }
                    }

                    break;
                }
                case 111:
                {
                    switch (newMap)
                    {
                        case 47:
                        {
                            SetupPositionXy(character, 925, 336);
                            break;
                        }
                    }
                    break;
                }
                case 156:
                {
                    switch (newMap)
                    {
                        case 157:
                        {
                            SetupPositionXy(character, 106, 312);
                            break;
                        }
                    }
                    break;
                }
                case 157:
                {
                    switch (newMap)
                    {
                        case 156:
                        {
                            SetupPositionXy(character, 1362, 624);
                            break;
                        }
                        case 158:
                        {
                            SetupPositionXy(character, 98, 456);
                            break;
                        }
                    }
                    break;
                }
                case 158:
                {
                    switch (newMap)
                    {
                        case 157:
                        {
                            SetupPositionXy(character, 823, 1360);
                            break;
                        }
                        case 159:
                        {
                            SetupPositionXy(character, 104, 1464);
                            break;
                        }
                    }
                    break;
                }
                case 159:
                {
                    switch (newMap)
                    {
                        case 158:
                        {
                            SetupPositionXy(character, 1571, 432);
                            break;
                        }
                    }
                    break;
                }
                case 160:
                {
                    switch (newMap)
                    {
                        case 161:
                        {
                            SetupPositionXy(character, 116, 312);
                            break;
                        }
                    }
                    break;
                }
                case 161:
                {
                    switch (newMap)
                    {
                        case 160:
                        {
                            SetupPositionXy(character, 1293, 1584);
                            break;
                        }
                    }
                    break;
                }
                default:
                {
                    SetupPositionXy(character, 100, 100);
                    break;
                }
            }
        }
    }
}
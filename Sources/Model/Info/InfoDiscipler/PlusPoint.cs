using System;
using NRO_Server.Application.Constants;
using NRO_Server.Application.IO;
using NRO_Server.DatabaseManager;
using NRO_Server.Model.Character;

namespace NRO_Server.Model.Info.InfoDiscipler
{
    public class PlusPoint
    {
        public int TypeNext { get; set; }
        public long PointNext { get; set; }

        public PlusPoint()
        {
            TypeNext = -1;
            PointNext = 0;
        }

        public void RandomPoint(Disciple disciple)
        {
            var list = DataCache.RandomPointDisciple;
            var type = list[ServerUtils.RandomNumber(list.Count)];
            long point = 0;
            do
            {
                TypeNext = type;
                PointNext = point = GetMinePoint(disciple, type);
                type = list[ServerUtils.RandomNumber(list.Count)];
            } while (point == 0);
        }

        public long GetMinePoint(Disciple disciple, int type)
        {
            
            long minePoint = 0;
            switch (type) {
                //Hp gốc
                case 0: {
                    var hpOld = disciple.InfoChar.OriginalHp; 
                    minePoint = hpOld + 1000;
                    break;
                }
                //Mp gốc
                case 1: {
                    var mpOld = disciple.InfoChar.OriginalMp;
                    minePoint = mpOld + 1000;
                    break;
                }
                //Dam gốc
                case 2: {
                    var damageOld = disciple.InfoChar.OriginalDamage;
                    minePoint = damageOld * 100;
                    break;
                }
                //Def gốc
                case 3: {
                    if (disciple.InfoChar.Level >= 17)
                    {
                        var defOld = disciple.InfoChar.OriginalDefence;
                        minePoint = 2 * (defOld + 5) / 2 * 15000;
                    }
                    break;
                }
                //Crit gốc
                case 4: {
                    var critOld = disciple.InfoChar.OriginalCrit;
                    if (critOld < 10 && disciple.InfoChar.Level >= 17)
                    {
                        minePoint = 5000000*critOld;//50000000;
                        // for(var i = 0; i < critOld; i++) {
                        //     minePoint *= 5;
                        // }  
                    }
                    break;
                }
            }
            return minePoint;
        }

        public bool CheckPlusPoint(Disciple disciple)
        {
            LimitPower limitPower;
            limitPower = 
                (disciple.InfoChar.IsPower 
                    ? Cache.Gi().LIMIT_POWERS[disciple.Character.InfoChar.LitmitPower] 
                    : Cache.Gi().LIMIT_POWERS[disciple.Character.InfoChar.LitmitPower - 1]) ?? Cache.Gi().LIMIT_POWERS[DataCache.MAX_LIMIT_POWER_LEVEL-1];
            switch (TypeNext) {
                //Hp gốc
                case 0: {
                    var hpOld = disciple.InfoChar.OriginalHp;
                    if(hpOld > limitPower.Hp) {
                        return false;
                    }
                    break;
                }
                //Mp gốc
                case 1: {
                    var mpOld = disciple.InfoChar.OriginalMp;
                    if(mpOld >= limitPower.Ki) {
                        return false;
                    }
                    break;
                }
                //Dam gốc
                case 2: {
                    var damageOld = disciple.InfoChar.OriginalDamage;
                    if(damageOld >= limitPower.Damage) {
                        return false;
                    }
                    break;
                }
                //Def gốc
                case 3: {
                    var defOld = disciple.InfoChar.OriginalDefence;
                    if(defOld+1 > limitPower.Def) return false;
                    break;
                }
                //Crit gốc
                case 4: {
                    var critOld = disciple.InfoChar.OriginalCrit;
                    if(critOld == 10) return false;
                    if(critOld >= limitPower.Crit) return false;
                    break;
                }
            }
            return true;
        }
    }
}
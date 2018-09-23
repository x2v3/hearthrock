using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hearthrock.Server.Score
{
    public class PlayerScore:SabberStoneCoreAi.Score.Score
    {
        public const int HeroHpScore = 1000;
        public const int MinionHpScore = 500;
        public const int TauntMinionHpAdditionalScore = 200;
        public const int MinionAttackScore = 500;
        public const int HeroAttackScore = 500;

        public override int Rate()
        {
            try
            {
                
                var rate = 0;
                rate += HeroRate();
                rate += BoardRate();
                return rate;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        public int BoardRate()
        {
            var rate = 0;

            var health = this.MinionTotHealth * MinionHpScore + this.MinionTotHealthTaunt* TauntMinionHpAdditionalScore;
            var attack = this.MinionTotAtk * MinionAttackScore;
            var opHealth = this.OpMinionTotHealth * MinionTotHealth +
                           this.OpMinionTotHealthTaunt * TauntMinionHpAdditionalScore;
            var opAttack = this.OpMinionTotAtk * MinionAttackScore;
            rate += health;
            rate += health;
            rate -= opHealth;
            rate -= opAttack;
            return rate;
        }

        public int HeroRate()
        {
            if (HeroHp <= 0)
            {
                return -999999;
            }

            if (OpHeroHp <= 0)
            {
                return 999999;
            }

            var healthRate = (HeroHp+ Controller.Hero.Armor + (30 - OpHeroHp - Controller.Opponent.Hero.Armor)) * HeroHpScore;
            var attackRate = HeroAtk * HeroAttackScore;
            return healthRate + attackRate;
        }
    }
}

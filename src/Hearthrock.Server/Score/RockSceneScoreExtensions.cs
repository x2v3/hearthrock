using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;

namespace Hearthrock.Server.Score
{
    public static class RockSceneScoreExtensions
    {
        private const double HeroHealthFactor=2;
        private const double LifeStealFactor = 0.1;
        private const double WindfuryFactor = 0.2;
        private const double TauntFactor = 0.1;
        private const double DivineShieldFactor = 0.3;

        public static double GetScore(this RockScene scene)
        {
            return scene.Self.GetScore() - scene.Opponent.GetScore();
        }

        public static double GetScore(this RockPlayer player)
        {
            var score = 0d;
            score += player.HasWeapon ? player.Weapon.Damage : 0;
            score += player.Hero.GetScore() + player.Minions.Sum(m => m.GetScore());
            return score;
        }

        public static double GetScore(this RockHero hero)
        {
            return hero.Damage + hero.Health*HeroHealthFactor;
        }

        public static double GetScore(this RockMinion minion)
        {
            var score = 0d;
            var damageFactor = 1d;
            damageFactor += minion.HasLifesteal ? LifeStealFactor : 0d;
            damageFactor += minion.HasWindfury ? WindfuryFactor : 0d;
            var healthFactor = 1d;
            healthFactor += minion.HasTaunt ? TauntFactor : 0;
            healthFactor += minion.HasDivineShield ? DivineShieldFactor : 0;

            score += minion.Damage * damageFactor;
            score += minion.Health* healthFactor;
            return score;
        }
    }
}

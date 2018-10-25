using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Hearthrock.Contracts;
using Hearthrock.Server.ML;
using Newtonsoft.Json;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;

namespace Hearthrock.Server.Score
{
    public static class UtilityExtensions
    {
        public static object GetObjectById(this RockPlayer player, int id)
        {
            if (player.Hero.RockId == id)
                return player.Hero;
            var minion = player.Minions.FirstOrDefault(m => m.RockId == id);
            if (minion != null)
                return minion;
            var card = player.Cards.FirstOrDefault(c => c.RockId == id);
            if (card != null)
                return card;
            var power = player.Power.RockId == id ? player.Power : null;
            if (power != null)
                return power;
            return null;
        }

        public static T DeepCopy<T>(this T other)
        {
            var json = JsonConvert.SerializeObject(other);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T Dulpicate<T>(this T src) where T : class
        {
            var type = src.GetType();
            var d = type.Assembly.CreateInstance(type.FullName) as T;
            foreach (var propertyInfo in type.GetProperties())
            {
                try
                {
                    var srcValue = propertyInfo.GetValue(src);
                    if (srcValue.GetType().IsValueType || srcValue is string)
                    {
                        propertyInfo.SetValue(d, srcValue);
                    }
                    else
                    {
                        var newValue = srcValue.Dulpicate();
                        propertyInfo.SetValue(d, newValue);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return d;
        }

        public static CardClass ToSabberCardClass(this RockHeroClass c)
        {
            var rc = CardClass.INVALID;
            switch (c)
            {
                case RockHeroClass.Druid:
                    rc = CardClass.DRUID;
                    break;
                case RockHeroClass.Hunter:
                    rc = CardClass.HUNTER;
                    break;
                case RockHeroClass.Mage:
                    rc = CardClass.MAGE;
                    break;
                case RockHeroClass.Paladin:
                    rc = CardClass.PALADIN;
                    break;
                case RockHeroClass.Priest:
                    rc = CardClass.PRIEST;
                    break;
                case RockHeroClass.Rogue:
                    rc = CardClass.ROGUE;
                    break;
                case RockHeroClass.Shaman:
                    rc = CardClass.SHAMAN;
                    break;
                case RockHeroClass.Warlock:
                    rc = CardClass.WARLOCK;
                    break;
                case RockHeroClass.Warrior:
                    rc = CardClass.WARRIOR;
                    break;
            }

            return rc;
        }

        public static SceneData GetSceneData(this RockScene scene)
        {
            var data = new SceneData();
            data.OpHasLifeSteal = scene.Opponent.Minions.Any(m => m.HasLifesteal) ? 1 : 0;
            data.OpHasWindFury = scene.Opponent.Minions.Any(m => m.HasWindfury) ? 1: 0;
            data.OpHeroAttackDamage = scene.Opponent.Hero.Damage;
            data.OpHeroClass = (int) scene.Opponent.Hero.Class;
            data.OpHeroHealth = scene.Opponent.Hero.Health;
            data.OpMinionsAttackDamage = scene.Opponent.Minions.Sum(m => m.Damage);
            data.OpMinionsHealth = scene.Opponent.Minions.Sum(m => m.Health);
            data.OpTauntMinionHealth = scene.Opponent.Minions.Where(m => m.HasTaunt).Sum(m => m.Health);
            data.Round = scene.Turn;
            data.SelfHasLifeSteal=scene.Self.Minions.Any(m=>m.HasLifesteal)?1:0;
            data.SelfHasWindFury = scene.Self.Minions.Any(m => m.HasWindfury) ? 1 : 0;
            data.SelfHeroAttackDamage = scene.Self.Hero.Damage;
            data.SelfHeroClass = (int) scene.Self.Hero.Class;
            data.SelfHeroHealth = scene.Self.Hero.Health;
            data.SelfMinionsAttackDamage = scene.Self.Minions.Sum(m => m.Damage);
            data.SelfMinionsHealth = scene.Self.Minions.Sum(m => m.Health);
            data.SelfTauntMinionHealth = scene.Self.Minions.Where(m => m.HasTaunt).Sum(m => m.Health);
            return data;
        }

        public static SceneData GetSceneData(this Game game)
        {
            //todo  check hero class code.
            var data = new SceneData();
            var p1Minions = game.Player1.BoardZone.GetAll();
            var p2Minions = game.Player2.BoardZone.GetAll();
            data.OpHasLifeSteal = p2Minions.Any(m=>m.HasLifeSteal) ? 1 : 0;
            data.OpHasWindFury = p2Minions.Any(m => m.HasWindfury) ? 1: 0;
            data.OpHeroAttackDamage = game.Player2.Hero.AttackDamage;
            //data.OpHeroClass = (int) scene.Opponent.Hero.Class;
            data.OpHeroHealth = game.Player2.Hero.Health + game.Player2.Hero.Armor;
            data.OpMinionsAttackDamage = p2Minions.Sum(m => m.Damage);
            data.OpMinionsHealth = p2Minions.Sum(m => m.Health);
            data.OpTauntMinionHealth = p2Minions.Where(m => m.HasTaunt).Sum(m => m.Health);
            //data.Round = scene.Turn;
            data.SelfHasLifeSteal= p1Minions.Any(m=>m.HasLifeSteal)?1:0;
            data.SelfHasWindFury = p1Minions.Any(m => m.HasWindfury) ? 1 : 0;
            data.SelfHeroAttackDamage = game.Player1.Hero.Damage;
            //data.SelfHeroClass = (int) scene.Self.Hero.Class;
            data.SelfHeroHealth = game.Player1.Hero.Health;
            data.SelfMinionsAttackDamage = p1Minions.Sum(m => m.Damage);
            data.SelfMinionsHealth = p1Minions.Sum(m => m.Health);
            data.SelfTauntMinionHealth = p1Minions.Where(m => m.HasTaunt).Sum(m => m.Health);
            return data;
        }
    }
}

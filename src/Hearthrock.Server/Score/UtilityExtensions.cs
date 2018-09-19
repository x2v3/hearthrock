using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Hearthrock.Contracts;
using Newtonsoft.Json;
using SabberStoneCore.Enums;

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
    }
}

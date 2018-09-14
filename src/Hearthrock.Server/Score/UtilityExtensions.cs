using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;

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
            return null;
        }

        public static T Dulpicate<T>(this T src) where T:class
        {
            var type = src.GetType();
            var d = type.Assembly.CreateInstance(type.Name) as T;
            foreach (var propertyInfo in type.GetProperties())
            {
                var srcValue = propertyInfo.GetValue(src);
                if (srcValue.GetType().IsValueType)
                {
                    propertyInfo.SetValue(d,srcValue);
                }
                else
                {
                    var newValue = srcValue.Dulpicate();
                    propertyInfo.SetValue(d,newValue);
                }
            }
            return d;
        }
    }
}

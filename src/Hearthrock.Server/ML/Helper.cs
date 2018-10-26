using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hearthrock.Server.ML
{
    public class Helper
    {
        public static List<SceneData> MatchToSceneData(IEnumerable<Match> matches,bool swapPlayers=false)
        {
            var list=new List<SceneData>();
            foreach (var match in matches)
            {
                foreach (var turn in match.Turns)
                {
                    var self = turn.Self;
                    var op = turn.Opponent;
                    var win = match.Win;
                    if (swapPlayers)
                    {
                        self = turn.Opponent;
                        op = turn.Self;
                        win = !match.Win;
                    }
                    
                    var sd=new SceneData()
                    {
                        Win = win?1:0,
                        Round = turn.Turn,

                        OpHasLifeSteal = op.Minions.Any(m=>m.HasLifesteal)?1:0,
                        OpHeroHealth = op.Hero.Health,
                        OpMinionsHealth = op.Minions.Sum(m=>m.Health),
                        OpHeroAttackDamage = op.Hero.Damage,
                        OpHasWindFury = op.Minions.Any(m=>m.HasWindfury)?1:0,
                        OpTauntMinionHealth = op.Minions.Where(m=>m.HasTaunt).Sum(m=>m.Health),
                        OpHeroClass = (int)op.Hero.Class,
                        OpMinionsAttackDamage = op.Minions.Sum(m=>m.Damage),
                        OpCardsInHand = op.Cards?.Count??0,

                        SelfHasLifeSteal = self.Minions.Any(m=>m.HasLifesteal)?1:0,
                        SelfHasWindFury = self.Minions.Any(m=>m.HasWindfury)?1:0,
                        SelfMinionsHealth = self.Minions.Sum(m=>m.Health),
                        SelfTauntMinionHealth = self.Minions.Where(m=>m.HasTaunt).Sum(m=>m.Health),
                        SelfMinionsAttackDamage = self.Minions.Sum(m=>m.Damage),
                        SelfHeroHealth = self.Hero.Health,
                        SelfHeroAttackDamage = op.Hero.Damage,
                        SelfHeroClass = (int)self.Hero.Class,
                        SelfCardsInHand = self.Cards?.Count??0
                        
                    };
                    list.Add(sd);
                }
            }
            return list;
        }
    }
}

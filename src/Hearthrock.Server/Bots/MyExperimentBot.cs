using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;
using Hearthrock.Server.Score;

namespace Hearthrock.Server.Bots
{
    public class MyExperimentBot:IRockBot
    {
        public RockAction GetMulliganAction(RockScene scene)
        {
            var cards = new List<int>();
            foreach (var card in scene.Self.Cards)
            {
                if (card.Cost >= 4)
                {
                    cards.Add(card.RockId);
                }
            }

            return RockAction.Create(cards);
        }

        public RockAction GetPlayAction(RockScene scene)
        {
            try
            {
                var engine = new SimulationEngine(scene);
                var actionScores=new List<KeyValuePair<RockAction,int>>();
                foreach (var option in scene.PlayOptions)
                {
                    var action = RockAction.Create(option);
                    var score = engine.SimulateAction(action);
                    actionScores.Add(new KeyValuePair<RockAction, int>(action,score));
                }

                var bestaction = actionScores.OrderByDescending(a=>a.Value).FirstOrDefault();
                return bestaction.Key;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (scene.PlayOptions.Any())
                {
                    // return a random action
                    return RockAction.Create(scene.PlayOptions[new Random().Next(0,scene.PlayOptions.Count-1)]);
                }
                else
                {
                    return RockAction.Create();
                }
            }
        }

        public void ReportActionResult(RockScene scene)
        {
            //todo log action.
        }
    }
}

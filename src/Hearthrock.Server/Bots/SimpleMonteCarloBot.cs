using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;

namespace Hearthrock.Server.Bots
{
    public class SimpleMonteCarloBot:IRockBot
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

                if (scene.PlayOptions.Any())
                {
                    // return a random action
                    return RockAction.Create(scene.PlayOptions[new Random().Next(0, scene.PlayOptions.Count - 1)]);
                }
                else
                {
                    return RockAction.Create();
                }
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

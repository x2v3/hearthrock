using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;

namespace Hearthrock.Server.Bots
{
    public class PunchFaceBot:IRockBot
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
            if (!scene.PlayOptions.Any())
            {
                return RockAction.Create();
            }
            var heroid = GetOpponentHeroId(scene);
            var punchAction = scene.PlayOptions.FirstOrDefault(o => o.Count == 2 && o[1] == heroid);
            if (punchAction != null)
            {
                return RockAction.Create(punchAction);
            }
            else
            {
                return RockAction.Create(scene.PlayOptions[new Random().Next(0, scene.PlayOptions.Count - 1)]);
            }
        }

        public void ReportActionResult(RockScene scene)
        {
            
        }

        private int GetOpponentHeroId(RockScene scene)
        {
            return scene.Opponent.Hero.RockId;
        }
    }
}

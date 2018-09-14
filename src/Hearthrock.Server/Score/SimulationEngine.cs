using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;

namespace Hearthrock.Server.Score
{
    public class SimulationEngine
    {
        public RockScene SimulateAction(RockScene currentScene, RockAction action)
        {
            var newScene = currentScene.Dulpicate();

            if (action.Objects.Count == 1)
            {
                // play out a card.
                var obj = currentScene.Self.GetObjectById(action.Objects[0]);
                if (obj != null)
                {
                    if (obj is RockCard card)
                    {
                        switch (card.CardType)
                        {
                            case RockCardType.Minion:
                                var m =new RockMinion()
                                {
                                    BaseHealth = card.Health,
                                    Damage = card.Damage,
                                    HasTaunt = card.HasTaunt,
                                    
                                };
                                break;

                        }
                    }
                }
            }

            return newScene;
        }

    }
}

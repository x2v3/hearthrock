using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;
using Hearthrock.Server.Score;
using Hearthrock.Server.Services;
using Microsoft.Extensions.Logging;

namespace Hearthrock.Server.Bots
{
    public class MyExperimentBot : IRockBot
    {
        public MyExperimentBot(ActionLogService logService, ILoggerFactory loggerFactory,IScoringService scoreService)
        {
            actionLog = logService;
            logger = loggerFactory.CreateLogger(this.GetType());
            this.scoreService = scoreService;
        }

        private ActionLogService actionLog;
        private ILogger logger;
        private IScoringService scoreService;

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
                var actionScores = new List<KeyValuePair<RockAction, int>>();
                scene.PlayOptions.Add(new List<int>()); //add end-turn option
                foreach (var option in scene.PlayOptions)
                {
                    if (option.Count == 0)
                    {
                        // end turn.
                        var engine = new SimulationEngine(scene,null,scoreService);
                        var action = RockAction.Create(option);
                        var score = engine.SimulateAction(action);
                        actionScores.Add(new KeyValuePair<RockAction, int>(action, score));
                    }
                    else if (scene.Self.GetObjectById(option[0]) is RockCard c &&c.CardType== RockCardType.Minion)
                    {
                        for (int i = 0; i <= scene.Self.Minions.Count; i++)
                        {

                            var engine = new SimulationEngine(scene,null,scoreService);
                            engine.ResetHeroMana();
                            var action = RockAction.Create(option);
                            action.Slot = i;
                            var score = engine.SimulateAction(action);
                            logger.LogInformation($"action:[{string.Join("=>",action.Objects)},slot {action.Slot}] got score:{score}");
                            actionScores.Add(new KeyValuePair<RockAction, int>(action, score));
                        }
                    }
                    else
                    {
                        var engine = new SimulationEngine(scene,null,scoreService);
                        engine.ResetHeroMana();
                        var action = RockAction.Create(option);
                        var score = engine.SimulateAction(action);
                        logger.LogInformation($"action:[{string.Join("=>",action.Objects)},slot {action.Slot}] got score:{score}");
                        actionScores.Add(new KeyValuePair<RockAction, int>(action, score));

                    }
                }

                var bestaction = actionScores.OrderByDescending(a => a.Value).FirstOrDefault();
                actionLog.AddPlayLogAsyncNoReturn(scene, bestaction.Key);
                return bestaction.Key;
            }
            catch (Exception e)
            {
                logger.LogError(e, "GetPlayAction Error");
                actionLog.AddErrorLogAsnycNoResult(scene, e);
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
        }

        public RockAction GetPlayAction2(RockScene scene)
        {
            throw new NotImplementedException();
        }

        public void ReportActionResult(RockScene scene)
        {
            //todo log action.
        }

    }
}

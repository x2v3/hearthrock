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
    public class MyExperimentBot:IRockBot
    {
        public MyExperimentBot(ActionLogService logService,ILoggerFactory loggerFactory)
        {
            actionLog = logService;
            consoleLogger = loggerFactory.CreateLogger(this.GetType());
        }

        private ActionLogService actionLog;
        private ILogger consoleLogger;

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
                    engine.ResetHeroMana();
                    var action = RockAction.Create(option);
                    var score = engine.SimulateAction(action);
                    actionScores.Add(new KeyValuePair<RockAction, int>(action,score));
                }

                var bestaction = actionScores.OrderByDescending(a=>a.Value).FirstOrDefault();
                actionLog.AddPlayLogAsyncNoReturn(scene, bestaction.Key);
                return bestaction.Key;
            }
            catch (Exception e)
            {
                consoleLogger.LogError(e,"GetPlayAction Error");
                actionLog.AddErrorLogAsnycNoResult(scene,e);
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

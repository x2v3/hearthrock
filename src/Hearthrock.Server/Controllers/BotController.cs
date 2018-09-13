using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hearthrock.Server.Controllers
{
    [Route("")]
    public class BotController:Controller
    {
        public BotController(IRockBot rockBot,ILoggerFactory loggerFactory)
        {
            bot = rockBot;
            logger = loggerFactory.CreateLogger(this.GetType());
        }

        private readonly IRockBot bot;
        private readonly ILogger logger;

        [HttpPost("mulligan"), HttpGet("mulligan")]
        public async Task<ActionResult> Mulligan([FromBody] RockScene scene)
        {
            return Json(await Task.FromResult(GetMulliganAction(scene)));
        }

        /// <summary>
        /// do play
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        [HttpGet("play"),HttpPost("play")]
        public async Task<ActionResult> Play([FromBody]RockScene scene)
        {
            return Json(await Task.FromResult(GetBestPlayAction(scene)));
        }

        [HttpGet("trace"), HttpPost("trace")]
        public async Task<ActionResult> Trace([FromBody]RockScene scene)
        {
            await Task.Run(() => DoTrace(scene));
            return Ok();
        }

        private void DoTrace(RockScene scene)
        {
            bot.ReportActionResult(scene);
        }

        private RockAction GetBestPlayAction(RockScene scene)
        {
            var a = bot.GetPlayAction(scene)??RockAction.Create();
            logger.LogWarning($"Play:{a.Objects.ToItemsString()}");
            return a;
        }
        private RockAction GetMulliganAction(RockScene scene)
        {
            var a = bot.GetMulliganAction(scene)??RockAction.Create();
            logger.LogWarning($"Mulligan:{a.Objects}");
            return a;
        }
    }
}

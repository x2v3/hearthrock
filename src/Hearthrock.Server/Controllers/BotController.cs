using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
            return Rock(await Task.FromResult(GetMulliganAction(scene)));
        }

        /// <summary>
        /// do play
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        [HttpGet("play"),HttpPost("play")]
        public async Task<ActionResult> Play([FromBody]RockScene scene)
        {
            return Rock(await Task.FromResult(GetBestPlayAction(scene)));
        }

        [HttpGet("trace"), HttpPost("trace")]
        public async Task<ActionResult> Trace([FromBody]Dictionary<string,string> scene)
        {
            await Task.Run(() => DoTrace(scene));
            return Ok();
        }

        private void DoTrace(Dictionary<string,string> info)
        {
            foreach (var k in info.Keys)
            {
                logger.LogDebug($"Trace:{k}:{info[k]}");
            }
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

        protected ContentResult Rock(object o)
        {
            var s = JsonConvert.SerializeObject(o);
            var r = new ContentResult
            {
                Content = s,
                ContentType = "applicatioin/json",
                StatusCode = 200
            };
            return r;
        }
        
    }
}

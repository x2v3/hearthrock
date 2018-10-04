using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Hearthrock.Contracts;
using Hearthrock.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hearthrock.Server.Controllers
{
    [Route("[controller]")]
    public class DataReportController : Controller
    {
        public DataReportController(ILoggerFactory loggerFactory, ActionLogService actionLogService)
        {
            logger = loggerFactory.CreateLogger(this.GetType());
            logDbService = actionLogService;
        }

        private ILogger logger;
        private ActionLogService logDbService;

        [HttpPost("playresult")]
        public ActionResult PlayResult([FromBody]PlayResult result)
        {
            logger.LogWarning($"GAME OVER {result.PlayerName}.{result.Session} {result.Won}");
            logDbService.AddPlayResultAsyncNoResult(result);
            return Ok();
        }

        [HttpGet("")]
        public ActionResult Show()
        {
            var r = new ContentResult();
            var sb = new StringBuilder();
            foreach (var o in logDbService.GetPlaySummary())
            {
                try
                {
                    var player = (o.Player as string).Split('#')[1];
                    sb.AppendLine($"{player} {o.Total}场{o.Win}胜，胜率{o.Rate}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            r.Content = sb.ToString();
            return r;
        }
    }
}
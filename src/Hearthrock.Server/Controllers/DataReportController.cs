using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;
using Hearthrock.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hearthrock.Server.Controllers
{
    [Route("[controller]")]
    public class DataReportController:Controller
    {
        public DataReportController(ILoggerFactory loggerFactory,ActionLogService actionLogService)
        {
            logger = loggerFactory.CreateLogger(this.GetType());
            logDbService = actionLogService;
        }

        private ILogger logger;
        private ActionLogService logDbService;

        [HttpPost("playresult")]
        public ActionResult PlayResult([FromBody]PlayResult result)
        {
            logger.LogWarning($"{result.PlayerName}.{result.Session} game over:won? {result.Won}");
            logDbService.AddPlayResultAsyncNoResult(result);
            return Ok();
        }
    }
}

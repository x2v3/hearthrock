using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [Route("playresult")]
        public ActionResult PlayResult(string session, bool win)
        {
            logger.LogWarning($"{session} game over:won? {win}");
            logDbService.AddPlayResultAsyncNoResult(session,win);
            return Ok();
        }
    }
}

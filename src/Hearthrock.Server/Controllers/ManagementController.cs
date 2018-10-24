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
    public class ManagementController:Controller
    {
        public ManagementController(ILoggerFactory loggerFactory, IScoringService scoringService)
        {
            logger = loggerFactory.CreateLogger(this.GetType());
            this.scoringService = scoringService as ScoringService;
        }

        private readonly ILogger logger;
        private readonly ScoringService scoringService;

        [Route("retrain/{swap}")]
        public ActionResult RetrainModel([FromRoute]bool swap=false)
        {
            scoringService.TrainAsync(swap);
            logger.LogWarning("Start training from API call.");
            return Ok(null);
        }
    }
}

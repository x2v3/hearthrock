using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hearthrock.Server.Controllers
{
    [Route("[controller]")]
    public class ApiTestController:Controller
    {
        public ApiTestController(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger(this.GetType());
        }

        private ILogger logger;

        [Route("")]
        public ActionResult Test(string p)
        {
            logger.LogWarning(p);
            return new ContentResult(){Content = p,ContentType = "plain/text"};
        }
    }
}

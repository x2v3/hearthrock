using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Hearthrock.Server.Controllers
{
    [Route("[controller]")]
    public class PlayLogController:Controller
    {
        [Route("")]
        public ViewResult Show()
        {
            return View("~/View/PlayInfo/Index.cshtml");
        }
    }
}

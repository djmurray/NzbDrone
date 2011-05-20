﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class HealthController : Controller
    {
        //
        // GET: /Health/

        [HttpGet]
        public JsonResult Index()
        {
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Controllers
{
    public class CommandController : Controller
    {
        // GET: Command
        public ActionResult Index(string id)
        {
            return View();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DrustveneMrezev3.Managers;
using DrustveneMrezev3.Models;
using DrustveneMrezev3.MongoDB_objects;
using DrustveneMrezev3.Twitter;
using Facebook;
using Microsoft.AspNet.Identity;

namespace DrustveneMrezev3.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            return View();
        }

        
    }
}
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
        public ActionResult Index()
        {
            return View();
        }
        
        public async Task<string> FillMovie(int page)
        {
            TMDbManager tmdb = new TMDbManager();
            string output = await tmdb.FillMovies(page);
            if (output == "ok")
                return "ok";
            else
                return "nok";
        }

        public ActionResult Fill()
        {
            return View();
        }
    }
}
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
using PagedList;

namespace DrustveneMrezev3.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index(int? page)
        {
            MongoDBManager mm = new MongoDBManager();
            var movies = await mm.GetMovies();

            List<MovieListModel> listMovies = new List<MovieListModel>();

            foreach (var m in movies)
            {
                MovieListModel newMovie = new MovieListModel()
                {
                    AvgUserRating = m.AvgUserRating,
                    ID = m.ID,
                    Title = m.Title
                };
                var movie = await mm.GetMovie(m.ID);
                newMovie.Poster = movie.Poster;
                listMovies.Add(newMovie);
            }

            return View(listMovies.ToPagedList(page ?? 1, 12));
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
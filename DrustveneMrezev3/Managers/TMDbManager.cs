using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;

namespace DrustveneMrezev3.Managers
{
    public class TMDbManager
    {
        private static string TheMovieDB = ConfigurationManager.AppSettings["TheMovieDB"];
        TMDbClient client;
        MongoDBManager mm;

        public TMDbManager()
        {
            client = new TMDbClient(ConfigurationManager.AppSettings["TheMovieDB"]);
            mm = new MongoDBManager();
        }

        public void FillMovies()
        {
            TMDbClient client = new TMDbClient(ConfigurationManager.AppSettings["TheMovieDB"]);

            List<MongoDB_objects.Movie> movies = new List<MongoDB_objects.Movie>();

            int numMovies = 0;
            int page = 0;

            while (numMovies < 5)
            {
                var popular = client.GetMovieList(MovieListType.Popular, page);
                foreach (var movie in popular.Results)
                {
                    numMovies++;
                    var tmdb = client.GetMovie(movie.Id);
                    var movieCredits = client.GetMovie(movie.Id, MovieMethods.Credits);
                    List<string> actors = new List<string>();
                    foreach (var c in movieCredits.Credits.Cast)
                    {
                        actors.Add(c.Name);
                    }

                    MongoDB_objects.Movie m = new MongoDB_objects.Movie();
                    m.Actors = actors;
                    m.Director = movieCredits.Credits.Crew.Where(c => c.Job == "Director").First().Name;
                    foreach(var g in tmdb.Genres)
                    {
                        m.Genres.Add(g.Name);
                    }
                    m.TMDbRating = tmdb.VoteAverage;
                    m.Language = tmdb.OriginalLanguage;
                    m.Plot = tmdb.Overview;
                    m.Poster = tmdb.PosterPath;
                    m.Released = tmdb.ReleaseDate;
                    m.Runtime = tmdb.Runtime;
                    m.Title = tmdb.OriginalTitle;

                    movies.Add(m);
                }
                page++;
            }
            System.Diagnostics.Debug.WriteLine(numMovies);
            mm.InsertMovies(movies);
        }
    }
}
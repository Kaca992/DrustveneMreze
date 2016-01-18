using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;

namespace DrustveneMrezev3.Managers
{
    public class TMDbManager
    {
        private static string TheMovieDB = ConfigurationManager.AppSettings["TheMovieDB"];
        TMDbClient TMDbClient;
        MongoDBManager MongoManager;
        string ImageBase;
        TMDbClient Client;

        public TMDbManager()
        {
            TMDbClient = new TMDbClient(ConfigurationManager.AppSettings["TheMovieDB"]);
            MongoManager = new MongoDBManager();

            TMDbClient.GetConfig();
            TMDbConfig config = TMDbClient.Config;
            ImageBase = config.Images.BaseUrl + config.Images.PosterSizes.Where(o => o == "w342").First();
            Client = new TMDbClient(ConfigurationManager.AppSettings["TheMovieDB"]);
        }

        private async Task<MongoDB_objects.Movie> ParseMovie(int movieId)
        {
            var tmdb = Client.GetMovie(movieId);
            var movieCredits = Client.GetMovie(movieId, MovieMethods.Credits);
            List<string> actors = new List<string>();
            foreach (var c in movieCredits.Credits.Cast)
            {
                actors.Add(c.Name);
            }

            MongoDB_objects.Movie m = new MongoDB_objects.Movie();
            m.Actors = actors;
            m.Director = movieCredits.Credits.Crew.Where(c => c.Job == "Director").First().Name;
            foreach (var g in tmdb.Genres)
            {
                m.Genres.Add(g.Name);
            }
            m.TMDbRating = tmdb.VoteAverage;
            m.Language = tmdb.OriginalLanguage;
            m.Plot = tmdb.Overview;
            m.Poster = ImageBase + tmdb.PosterPath;
            m.Released = tmdb.ReleaseDate;
            m.Runtime = tmdb.Runtime;
            m.Title = tmdb.OriginalTitle;
            m.TMDbId = tmdb.Id;
            m.ImdbId = tmdb.ImdbId;
            MongoDB_objects.Movie omdb = await OMDbManager.GetData(name: m.Title);
            m.ImdbRating = omdb.ImdbRating;
            var videos = Client.GetMovie(movieId, MovieMethods.Videos).Videos;
            string youtubeLink = "";
            if (videos != null)
            {
                foreach (var video in videos.Results)
                {
                    if (video.Site == "YouTube")
                    {
                        youtubeLink = "https://www.youtube.com/watch?v=" + video.Key;
                    }
                }
            }
            m.YouTube = youtubeLink;

            return m;
        }

        public async Task<string> FillMovies(int page = 0)
        {
            List<MongoDB_objects.Movie> movies = new List<MongoDB_objects.Movie>();

            var popular = Client.GetMovieList(MovieListType.Popular, page);
            foreach (var movie in popular.Results)
            {
                movies.Add(await ParseMovie(movie.Id));
            }
            await MongoManager.InsertMovies(movies);
            return "ok";
        }

        public async Task<ObjectId?> InsertMovieByName(string name)
        {
            MongoDBManager mm = new MongoDBManager();
            var found = await mm.FindMoviesByName(name);
            if (found.Count == 0)
            {
                SearchContainer<SearchMovie> results = Client.SearchMovie(name);
                if (results.TotalResults >= 1)
                {
                    SearchMovie tmdb = results.Results.First();
                    var movie = await ParseMovie(tmdb.Id);
                    await MongoManager.InsertNewMovie(movie);
                    return movie.ID;
                }
            }
            return null;
        }
    }
}
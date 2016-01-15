using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Razor;
using DrustveneMrezev3.Managers;
using DrustveneMrezev3.MongoDB_objects;

namespace DrustveneMrezev3.MovieRecommendation
{
    public class GenreMovieRecomendations:IMovieRecomendationProvider
    {
        public List<Movie> GetRecommendedMovies(string userID)
        {
            MongoDBManager mm = new MongoDBManager();
            List<Movie> movies = new List<Movie>();

            Dictionary<string,int> genras = new Dictionary<string, int>();
            UserInformation user = mm.GetUserInformation(userID);

            if (user == null)
            {
                return movies;
            }


            /*foreach (var movieLike in user.MovieLikes) //get user favourite genra
            {
                var movie = mm.GetMovie(movieLike.Id);
                List<string> movieGenras = movie.Genre.Split(',').Select(x => x.Trim()).ToList();

                if (parameters.Valid(movieGenras))
                {
                    movies.Add(movie);
                }


            }
            return movies;*/
            foreach (var movieLike in user.MovieLikes)//get user favourite genra
            {
                var movie = mm.GetMovie(movieLike.Id);
                List<string> movieGenras = movie.Genre.Split(',').ToList();

                foreach (var movieGenra in movieGenras)
                {
                    if (!genras.ContainsKey(movieGenra))
                    {
                        genras.Add(movieGenra,0);
                    }
                    genras[movieGenra]++;
                }
            }

            var max = genras.Values.Max();
            string favouriteGenre = "";

            foreach (var genre in genras.Keys)
            {
                if (genras[genre] == max)
                {
                    favouriteGenre = genre;
                    break;
                }
            }

            movies = mm.GetMoviesWithGenre(favouriteGenre);

            MoviesSort ms = new MoviesSort();

            movies.Sort(ms);
            return movies;

        }
    }
}
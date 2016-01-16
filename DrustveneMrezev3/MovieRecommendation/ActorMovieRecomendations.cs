using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DrustveneMrezev3.Managers;
using DrustveneMrezev3.MongoDB_objects;

namespace DrustveneMrezev3.MovieRecommendation
{
    public class ActorMovieRecomendations : IMovieRecomendationProvider
    {
        public List<Movie> GetRecommendedMovies(string userID)
        {
            MongoDBManager mm = new MongoDBManager();
            List<Movie> movies = new List<Movie>();

            Dictionary<string, int> actors = new Dictionary<string, int>();
            UserInformation user = mm.GetUserInformation(userID);

            if (user == null)
            {
                return movies;
            }


            foreach (var movieLike in user.MovieLikes)//get user favourite genra
            {
                var movie = mm.GetMovie(movieLike.Id);
                List<string> movieActors = movie.Actors.Split(',').ToList();

                foreach (var movieActor in movieActors)
                {
                    if (!actors.ContainsKey(movieActor))
                    {
                        actors.Add(movieActor, 0);
                    }
                    actors[movieActor]++;
                }
            }

            var max = actors.Values.Max();
            string favouriteActor = "";

            foreach (var actor in actors.Keys)
            {
                if (actors[actor] == max)
                {
                    favouriteActor = actor;
                    break;
                }
            }

            movies = mm.GetMoviesWithActor(favouriteActor);
            List<Movie> moviesWithoutUserLiked = new List<Movie>();

            foreach (var movie in movies)
            {
                bool exist = false;
                foreach (var userLiked in user.MovieLikes)
                {
                    if (userLiked.Id == movie.ID)
                    {
                        exist = true;
                        break;
                    }
                }

                if (!exist)
                {
                    moviesWithoutUserLiked.Add(movie);
                }
            }

            MoviesSort ms = new MoviesSort();

            moviesWithoutUserLiked.Sort(ms);
            return moviesWithoutUserLiked;

        }
    }
}
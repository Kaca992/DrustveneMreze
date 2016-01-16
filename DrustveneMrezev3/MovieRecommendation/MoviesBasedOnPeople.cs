using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DrustveneMrezev3.Managers;
using DrustveneMrezev3.MongoDB_objects;
using MongoDB.Bson;

namespace DrustveneMrezev3.MovieRecommendation
{
    //returns movies that where liked by people that liked the same movies as us
    public class MoviesBasedOnPeople : IMovieRecomendationProvider
    {
        public List<Movie> GetRecommendedMovies(string userID)
        {
            MongoDBManager mm = new MongoDBManager();
            List<Movie> movies = new List<Movie>();

            Dictionary<ObjectId, int> similarMovies = new Dictionary<ObjectId, int>();
            UserInformation user = mm.GetUserInformation(userID);

            if (user == null)
            {
                return movies;
            }

            foreach (var movieLike in user.MovieLikes)
            {
                List<UserInformation> users = mm.GetAllUsersWithExceptCurrent(userID,movieLike);

                foreach (var userFromDb in users)
                {
                    foreach (var movie in userFromDb.MovieLikes)
                    {
                        if (!similarMovies.ContainsKey(movie.Id))
                        {
                            similarMovies.Add(movie.Id,0);
                        }

                        similarMovies[movie.Id]++;
                    }
                }

            }

            var sortedSimilarMovies = from pair in similarMovies
                        orderby pair.Value descending 
                        select pair;

            int counter = 0;
            foreach (var sortedSimilarMovie in sortedSimilarMovies)
            {
                if (user.MovieLikes.Exists(x => x.Id == sortedSimilarMovie.Key))
                {
                    continue;
                }

                counter++;
                movies.Add(mm.GetMovie(sortedSimilarMovie.Key));
            }

            return movies;

        }
    }
}
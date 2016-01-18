using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DrustveneMrezev3.Models;
using DrustveneMrezev3.MongoDB_objects;
using DrustveneMrezev3.Twitter;
using Facebook;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DrustveneMrezev3.Managers
{
    public class MongoDBManager
    {
        protected static IMongoClient _mongoClient;
        protected static IMongoDatabase _mongoUserInfoDatabase;
        protected static IMongoDatabase _mongoMoviesDatabase;
        protected static IMongoDatabase _mongoTweetsDatabase;

        public MongoDBManager()
        {
            _mongoClient = new MongoClient(ConfigurationManager.ConnectionStrings["DrustveneMrezeConnectionString"].ConnectionString);
            _mongoUserInfoDatabase = _mongoClient.GetDatabase(ConfigurationManager.AppSettings["DrustveneMrezeDatabaseName"]);
            _mongoMoviesDatabase = _mongoClient.GetDatabase(ConfigurationManager.AppSettings["DrustveneMrezeMoviesDatabaseName"]);
            _mongoTweetsDatabase = _mongoClient.GetDatabase(ConfigurationManager.AppSettings["DrustveneMrezeTweetsDatabaseName"]);
            
        }

        public IMongoDatabase UserInfoDatabase
        {
            get
            {
                return _mongoUserInfoDatabase;
            }
        }

        public IMongoDatabase MoviesDatabase
        {
            get
            {
                return _mongoMoviesDatabase;
            }
        }

        public IMongoDatabase TweetsDatabase
        {
            get
            {
                return _mongoTweetsDatabase;
            }
        }

        public IMongoCollection<UserInformation> Users
        {
            get
            {
                return UserInfoDatabase.GetCollection<UserInformation>("users");
            }
        }

        public IMongoCollection<Movie> Movies
        {
            get
            {
                return UserInfoDatabase.GetCollection<Movie>("movies");
            }
        }

        public IMongoCollection<Tweets> Tweets
        {
            get
            {
                return UserInfoDatabase.GetCollection<Tweets>("tweets");
            }
        }

        public void InsertNewUser(UserInformation user)
        {
            Users.InsertOneAsync(user);
        }

        public async Task InsertMovies(List<Movie> movies)
        {
            foreach (var movie in movies)
            {
                var found = Movies.Find(m => m.TMDbId == movie.TMDbId).ToListAsync().Result;
                if (found.Count == 0)
                {
                    await Movies.InsertOneAsync(movie);
                }
            }
        }

        public async Task<ObjectId?> InsertNewMovie(Movie movie)
        {
            await Movies.InsertOneAsync(movie);
            return movie.ID;
        }

        public void InsertNewTweets()
        {
            var filter = new BsonDocument();
            Tweets.DeleteManyAsync(filter);
            TwitterUserAuthorization.GetMostRecent10HomeTimeLine();
            foreach (var tweet in TwitterUserAuthorization.CurrentTweets)
            {
                Tweets newTweets = new Tweets()
                {
                    ID = new Guid(),
                    DateTweeted = tweet.CreatedAt.ToShortDateString(),
                    Retweeted = tweet.RetweetCount,
                    Tweet = tweet.Text,
                    UserImageUrl = tweet.User.ProfileImageUrl,
                    UserName = tweet.User.Name
                };
                Tweets.InsertOneAsync(newTweets);
            }
        }

        public async Task<List<Movie>> FindMoviesByName(string name)
        {
            return await Movies.Find(x => x.Title == name).ToListAsync();
        }

        /*public async Task InsertNewMovie(string id, string title)
        {
            Movie movie = await OMDbManager.GetData(id, title);
            if (movie != null)
            {
                if (movie.IsValid())
                {
                    var task = InsertNewMovie(movie);
                    Task.WaitAll(task);
                }
            }
        }*/

        public List<Tweets> GetAllTweets()
        {
            var filter = new BsonDocument();
            return Tweets.Find(filter).Limit(10).ToListAsync().Result;
        }

        public async Task<UserInformation> GetUserInformation(string id)
        {
            return await Users.Find(x => x.Id == id).FirstAsync();
        }

        public async Task<Movie> GetMovie(ObjectId id)
        {
            
            return await Movies.Find(x => x.ID == id).FirstAsync();
        }

        public async Task<List<Movie>> GetMovies()
        {
            return await Movies.Find(new BsonDocument()).Limit(48).ToListAsync();
        }

        public List<Movie> GetMoviesWithGenre(string genre)
        {
            return Movies.Find(x => x.Genres.Contains(genre)).ToListAsync().Result.ToList();
        }

        public async Task<List<Movie>> GetMoviesWithActor(string actor)
        {
            return await Movies.Find(x => x.Actors.Contains(actor)).ToListAsync();
        }

        public List<UserInformation> GetAllUsersWithExceptCurrent(string ID, MovieLike movie)
        {
            return Users.Find(x => x.Id != ID && x.MovieLikes.Contains(movie)).ToListAsync().Result;
        }

        public MovieLike GetUserRating(string userID, ObjectId movieID)
        {
            UserInformation user = Users.Find(x => x.Id == userID).Limit(1).ToListAsync().Result.FirstOrDefault();
            if (user == null)
            {
                return null;
            }
            foreach (var movieLike in user.MovieLikes)
            {
                if (movieLike.Id == movieID)
                {
                    return movieLike;
                }
            }

            return null;
        }

        public async Task UpdateUserRating(string userID, ObjectId movieID, int rating)
        {
            UserInformation user = await GetUserInformation(userID);
            int previousRanking = 0;

            if (user != null)
            {
                foreach (var movieLike in user.MovieLikes)
                {
                    if (movieLike.Id == movieID)
                    {
                        previousRanking = movieLike.UserRating;
                        movieLike.UserRating = rating;
                        break;
                    }
                }

                var update = Builders<UserInformation>.Update.Set(x => x.MovieLikes, user.MovieLikes);


                Movie movie = await GetMovie(movieID);
                Double temp;
                if (previousRanking != 0)
                {
                    temp = movie.AvgUserRating*movie.NumberOfUsersRated - previousRanking;
                    movie.NumberOfUsersRated--;
                }
                else
                {
                    temp = movie.AvgUserRating*movie.NumberOfUsersRated;
                }

                movie.AvgUserRating = (temp + rating)/(movie.NumberOfUsersRated + 1);
                movie.NumberOfUsersRated++;

                var update2 = Builders<Movie>.Update.Set(x => x.AvgUserRating, movie.AvgUserRating);
                var update3 = Builders<Movie>.Update.Set(x => x.NumberOfUsersRated, movie.NumberOfUsersRated);
                await Movies.UpdateOneAsync(x => x.ID == movieID, update2);
                await Movies.UpdateOneAsync(x => x.ID == movieID, update3);
                await Users.UpdateOneAsync(x => x.Id == userID, update);
            }
        }

        public async Task DislikeMovie(string userID, ObjectId movieID)
        {
            UserInformation user = await GetUserInformation(userID);
            List<MovieLike> newMovieLikes = new List<MovieLike>();
            int userRanking = 0;

            if (user != null)
            {
                foreach (var movieLike in user.MovieLikes)
                {
                    if (movieLike.Id == movieID)
                    {                       
                        userRanking = movieLike.UserRating;                        
                    }
                    else
                    {
                        newMovieLikes.Add(movieLike);
                    }
                }

                var update = Builders<UserInformation>.Update.Set(x => x.MovieLikes, newMovieLikes);

                Movie movie = await GetMovie(movieID);
                Double temp;
                if (userRanking != 0)//user hasnt rated this movie so no need to refresh movie ranking
                {
                    temp = movie.AvgUserRating * movie.NumberOfUsersRated - userRanking;

                    if ((movie.NumberOfUsersRated - 1) == 0)
                    {
                        movie.AvgUserRating = 0;
                    }
                    else
                    {
                        movie.AvgUserRating = (temp) / (movie.NumberOfUsersRated - 1);
                    }

                    movie.NumberOfUsersRated--;

                    var update2 = Builders<Movie>.Update.Set(x => x.AvgUserRating, movie.AvgUserRating);
                    var update3 = Builders<Movie>.Update.Set(x => x.NumberOfUsersRated, movie.NumberOfUsersRated);
                    await Movies.UpdateOneAsync(x => x.ID == movieID, update2);
                    await Movies.UpdateOneAsync(x => x.ID == movieID, update3);
                }
                
                
                await Users.UpdateOneAsync(x => x.Id == userID, update);
            }
        }

        public async Task LikeMovie(string userID, ObjectId movieID)
        {
            System.Diagnostics.Debug.WriteLine(movieID);
            UserInformation user = await GetUserInformation(userID);
            MovieLike newMovieLike = new MovieLike();

            Movie movie = await GetMovie(movieID);

            newMovieLike.Id = movie.ID;
            newMovieLike.Name = movie.Title;
            user.MovieLikes.Add(newMovieLike);

            var update = Builders<UserInformation>.Update.Set(x => x.MovieLikes, user.MovieLikes);
            await Users.UpdateOneAsync(x => x.Id == userID, update);
        }

        public async Task<List<MovieLike>> UpdateUserLikes(string id, string tokenFB)
        {
            
            UserInformation user = await GetUserInformation(id);

            if (user != null && tokenFB != null)
            {
                List<MovieLike> movieLikes = new List<MovieLike>();
                
                var fb = new FacebookClient(tokenFB);
                dynamic data = new System.Dynamic.ExpandoObject();
                data.movies = null;
                data = fb.Get("/me?fields=movies");
                if (data.movies != null)
                {
                    foreach (dynamic movie in data.movies.data)
                    {
                        movieLikes.Add(new MovieLike() { Id = movie.id, Name = movie.name });
                    }
                }
                
                user.MovieLikes = movieLikes;
                var update = Builders<UserInformation>.Update.Set(x => x.MovieLikes,movieLikes);
                await Users.UpdateOneAsync(x => x.Id == id,update);
                //var result = await UpdateMoviesDatabase(movieLikes);

                return movieLikes;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> UpdateMoviesDatabase(List<MovieLike> movieLikes)
        {

            foreach (var item in movieLikes)
            {
                Movie movie = await OMDbManager.GetData(item.Id, item.Name);
                if (movie != null && movie.IsValid())
                {
                    await InsertNewMovie(movie);
                }
            }
           
            return true;

        }

    }
}
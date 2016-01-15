﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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

        public void InsertNewMovie(Movie movie)
        {
            Movies.InsertOneAsync(movie);
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

        public UserInformation GetUserInformation(string id)
        {
            return Users.Find(x => x.Id == id).Limit(1).ToListAsync().Result.FirstOrDefault();
        }

        public Movie GetMovie(string id)
        {
            
            return Movies.Find(x => x.ID == id).Limit(1).ToListAsync().Result.FirstOrDefault();
        }

        public int GetUserRating(string userID,string movieID)
        {
            UserInformation user = Users.Find(x => x.Id == userID).Limit(1).ToListAsync().Result.FirstOrDefault();
            if (user == null)
            {
                return 0;
            }
            foreach (var movieLike in user.MovieLikes)
            {
                if (movieLike.Id == movieID)
                {
                    return movieLike.UserRating;
                }
            }

            return 0;
        }

        public void UpdateUserRating(string userID, string movieID, int rating)
        {
            UserInformation user = GetUserInformation(userID);

            if (user != null)
            {
                foreach (var movieLike in user.MovieLikes)
                {
                    if (movieLike.Id == movieID)
                    {
                        movieLike.UserRating = rating;
                        break;
                    }
                }

                var update = Builders<UserInformation>.Update.Set(x => x.MovieLikes, user.MovieLikes);
                Users.UpdateOneAsync(x => x.Id == userID, update);
                
            }
        }

        public async Task<List<MovieLike>> UpdateUserLikes(string id, string tokenFB)
        {
            
            UserInformation user = GetUserInformation(id);

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
                Users.UpdateOneAsync(x => x.Id == id,update);
                var result = await UpdateMoviesDatabase(movieLikes);

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
                    InsertNewMovie(movie);
                }
            }
           
            return true;

        }

    }
}
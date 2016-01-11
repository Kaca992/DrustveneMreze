using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LinqToTwitter;

namespace DrustveneMrezev3.Twitter
{
    public class TwitterUserAuthorization
    {
        public static List<Status> CurrentTweets;
        private static SingleUserAuthorizer authorizer =
         new SingleUserAuthorizer
         {
             CredentialStore =
                new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = "Q3KhfOBjpQT1YaTwkddgmiyzN",
                    ConsumerSecret = "VEji8E7RjOXL0GQqA6G2g13k6nrnNYIO4jEzFSLsZKMN8qXZc5",
                    AccessToken = "4180483576-MlDoTFdDqiNqnYXh3X6gvJJXln9buvlFMiWQyos",
                    AccessTokenSecret = "nNotwvrPGDHGLDdQaSpdgLIN7T63EioBAvLb3xCOuRLBk"
                }
         };

        public static void GetMostRecent10HomeTimeLine()
        {
            var twitterContext = new TwitterContext(authorizer);

            var tweets = from tweet in twitterContext.Status
                         where tweet.Type == StatusType.Home &&
                         tweet.Count == 10
                         select tweet;

            CurrentTweets = tweets.ToList();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;

namespace DrustveneMrezev3.MongoDB_objects
{
    public class Tweets
    {
        [BsonId]
        public Guid ID { get; set; }

        public string DateTweeted { get; set; }

        public string UserName { get; set; }

        public string UserImageUrl { get; set; }

        public string Tweet { get; set; }

        public int Retweeted { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace DrustveneMrezev3.MongoDB_objects
{
    public class MovieLike
    {
        
        public string Id { get; set; }

        public string Name { get; set; }

        public int UserRating { get; set; }
    }
}

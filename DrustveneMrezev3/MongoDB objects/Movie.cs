using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DrustveneMrezev3.MongoDB_objects
{
    public class Movie
    {
        [BsonId]
        public ObjectId ID { get; set; }
        public string Title { get; set; }
        public string Director { get; set; }
        public List<string> Genres { get; set; }
        public List<string> Actors { get; set; }
        public DateTime? Released { get; set; }
        public string ImdbRating { get; set; }
        public Double TMDbRating { get; set; }
        public string MetascoreRating { get; set; }

        public string Language { get; set; }
        public int? Runtime { get; set; }
        public string Plot { get; set; }
        public string Poster { get; set; }

        public Double AvgUserRating { get; set; }
        public int NumberOfUsersRated { get; set; }

        public bool IsValid()
        {
            if (Title == "" || Director == "" || Runtime == null)
            {
                return false;
            }

            return true;
        }

        public Movie()
        {
            Genres = new List<string>();
            Actors = new List<string>();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;

namespace DrustveneMrezev3.MongoDB_objects
{
    public class Movie
    {
        [BsonId]
        public string ID { get; set; }
        public string Title { get; set; }
        public string Director { get; set; }
        public string Genre { get; set; }
        public string Actors { get; set; }
        public string Released { get; set; }
        public string ImdbRating { get; set; }
        public string MetascoreRating { get; set; }

        public string Language { get; set; }
        public string Runtime { get; set; }
        public string Plot { get; set; }
        public string Poster { get; set; }

        public bool IsValid()
        {
            if (Title == "" || Director == "" || Runtime == "")
            {
                return false;
            }

            return true;
        }

    }
}
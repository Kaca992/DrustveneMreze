using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrustveneMrezev3.Models
{
    public class MovieListModel
    {
        public ObjectId ID { get; set; }
        public string Title { get; set; }
        public string Poster { get; set; }
        public Double AvgUserRating { get; set; }
    }
}
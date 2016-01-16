using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrustveneMrezev3.Models
{
    public class MovieListModel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Poster { get; set; }
        public decimal AvgUserRating { get; set; }
    }
}
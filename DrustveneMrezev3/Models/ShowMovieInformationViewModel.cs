using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DrustveneMrezev3.Models
{
    public class ShowMovieInformationViewModel
    {
        public ObjectId ID { get; set; }
        [Display(Name = "Title")]
        public string Title { get; set; }
        [Display(Name = "Director")]
        public string Director { get; set; }
        [Display(Name = "Genre")]
        public List<string> Genres { get; set; }
        [Display(Name = "Stars")]
        public List<string> Actors { get; set; }
        [Display(Name = "Released")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy}")]
        public DateTime Released { get; set; }
        [Display(Name = "TMDB")]
        public Double TmdbRating { get; set; }
        [Display(Name = "IMDB")]
        public string ImdbRating { get; set; }
        [Display(Name = "Metascore")]
        public string MetascoreRating { get; set; }
        [Display(Name = "Language")]
        public string Language { get; set; }
        [Display(Name = "Runtime")]
        public int? Runtime { get; set; }
        [Display(Name = "Plot summary")]
        public string Plot { get; set; }
        public string Poster { get; set; }

        public Double AvgUserRating { get; set; }
        public int UserRating { get; set; }

        public bool Liked { get; set; }
    }
}
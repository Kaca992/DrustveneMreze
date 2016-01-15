using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DrustveneMrezev3.MongoDB_objects;

namespace DrustveneMrezev3.MovieRecommendation
{
    public class MoviesSort:IComparer<Movie>
    {
        public int Compare(Movie x, Movie y)
        {
            var xValue = Convert.ToDecimal(x.ImdbRating);
            var yValue = Convert.ToDecimal(y.ImdbRating);
            if (xValue > yValue)
            {
                return -1;
            }

            if (xValue < yValue)
            {
                return 1;
            }

            xValue = Convert.ToDecimal(x.MetascoreRating);
            yValue = Convert.ToDecimal(y.MetascoreRating);

            if (xValue > yValue)
            {
                return -1;
            }

            if (xValue < yValue)
            {
                return 1;
            }

            return 0;
        }
    }

}
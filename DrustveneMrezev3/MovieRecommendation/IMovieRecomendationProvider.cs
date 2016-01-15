using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DrustveneMrezev3.MongoDB_objects;

namespace DrustveneMrezev3.MovieRecommendation
{
    public interface IMovieRecomendationProvider
    {
        List<Movie> GetRecommendedMovies(string userID);
    }
}

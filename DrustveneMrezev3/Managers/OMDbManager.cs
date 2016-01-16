using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using DrustveneMrezev3.MongoDB_objects;
using Microsoft.Ajax.Utilities;
using MongoDB.Bson;

namespace DrustveneMrezev3.Managers
{
    public class OMDbManager
    {
        public static async Task<Movie> GetData(ObjectId id = new ObjectId(), string name="Captain Phillips")
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://www.omdbapi.com");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var searchName = HttpUtility.UrlEncode(name);
                HttpResponseMessage response = await client.GetAsync(string.Format("?t={0}&r=json", searchName));

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic result = new ExpandoObject();
                    result.Actors = "";
                    result.Director= "";
                    result.Genre= "";
                    result.imdbRating= "";
                    result.Language= "";
                    result.Metascore= "";
                    result.Released= "";
                    result.Runtime = "";
                    result.Poster = "";
                    result.Plot = "";
                    result = Json.Decode(responseBody);
                    
                    Movie movie = new Movie()
                    {
                        ID = id,
                        Title = name,
                        Actors = result.Actors,
                        Director = result.Director,
                        Genres = result.Genre,
                        ImdbRating = result.imdbRating,
                        Language = result.Language,
                        MetascoreRating = result.Metascore,
                        Released = result.Released,
                        Runtime = result.Runtime,
                        Plot = result.Plot,
                        Poster = result.Poster
                        
                    };

                    /*if (result.Poster != "")
                    {
                        using (WebClient imgClient = new WebClient())
                        {
                            try
                            {
                                movie.Poster = imgClient.DownloadData(result.Poster);
                            }
                            catch (Exception e)
                            {
                                movie.Poster = null;
                            }
                            
                        }
                    }*/

                    return movie;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
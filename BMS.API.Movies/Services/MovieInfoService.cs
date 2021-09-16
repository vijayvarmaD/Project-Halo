using BMS.API.Movies.Models;
using BMS.API.Movies.Models.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BMS.API.Movies.Services
{
    public class MovieInfoService
    {
        private readonly IMongoCollection<MovieInfo> _movieInfo;

        public MovieInfoService(IMovieDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _movieInfo = database.GetCollection<MovieInfo>("MovieInfo");
        }

        public IList<MovieInfo> Read() => _movieInfo.Find(sub => true).ToList();
    }
}

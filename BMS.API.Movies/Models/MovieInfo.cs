using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BMS.API.Movies.Models
{
    public class MovieInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("title")]
        public string Title { get; set; }
        [BsonElement("year")]
        public int Year { get; set; }
        [BsonElement("releaseDate")]
        public ReleaseDate[] ReleaseDates { get; set; }
        [BsonElement("runtime")]
        public int Runtime { get; set; }
        [BsonElement("genre")]
        public string[] Genre { get; set; }
        [BsonElement("cast")]
        public string[] Cast { get; set; }
        [BsonElement("plot")]
        public string Plot { get; set; }
        [BsonElement("languages")]
        public string[] Languages { get; set; }
        [BsonElement("country")]
        public string Country { get; set; }
        [BsonElement("cbfcRating")]
        public string CBFCRating { get; set; }
        [BsonElement("format")]
        public string[] Format { get; set; }
        [BsonElement("crew")]
        public Crew Crew { get; set; }
        [BsonElement("ratings")]
        public Rating[] Ratings { get; set; }

    }
}

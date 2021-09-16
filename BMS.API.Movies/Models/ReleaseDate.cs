using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BMS.API.Movies.Models
{
    public class ReleaseDate
    {
        [BsonElement("country")]
        public string Country { get; set; }
        [BsonElement("date")]
        public DateTime Date { get; set; }
    }
}

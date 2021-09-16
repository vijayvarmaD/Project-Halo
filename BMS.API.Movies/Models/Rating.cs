using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BMS.API.Movies.Models
{
    public class Rating
    {
        [BsonElement("userId")]
        public string UserId { get; set; }
        [BsonElement("userName")]
        public string UserName { get; set; }
        [BsonElement("rating")]
        public int RatingPercent { get; set; }
        [BsonElement("review")]
        public Review Review { get; set; }
    }
}

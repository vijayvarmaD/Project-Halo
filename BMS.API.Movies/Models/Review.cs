using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BMS.API.Movies.Models
{
    public class Review
    {
        [BsonElement("header")]
        public string Header { get; set; }
        [BsonElement("description")]
        public string Description { get; set; }
        [BsonElement("likes")]
        public int LikesCount { get; set; }
        [BsonElement("dislikes")]
        public int DislikesCount { get; set; }
    }
}

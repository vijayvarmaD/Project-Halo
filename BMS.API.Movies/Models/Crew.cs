using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BMS.API.Movies.Models
{
    public class Crew
    {
        [BsonElement("director")]
        public string Director { get; set; }
        [BsonElement("producer")]
        public string[] Producers { get; set; }
        [BsonElement("writer")]
        public string Writer { get; set; }
        [BsonElement("musician")]
        public string Musician { get; set; }
        [BsonElement("cinematographer")]
        public string Cinematographer { get; set; }
        [BsonElement("editor")]
        public string Editor { get; set; }
    }
}

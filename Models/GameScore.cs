using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace API.Models
{
    public class GameScore
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("MatchId")]
        public string MatchId { get; set; } = null!;

        [BsonElement("Players")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Players { get; set; } = new List<string>();

        [BsonElement("EscapeTimeSeconds")]
        public int EscapeTimeSeconds { get; set; }

        [BsonElement("Result")]
        public string Result { get; set; } = null!; // e.g. Victory, Defeat
    }
}

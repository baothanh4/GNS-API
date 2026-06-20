using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models
{
    public class PlayerProfile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("UserId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;

        [BsonElement("Nickname")]
        public string Nickname { get; set; } = null!;

        [BsonElement("Level")]
        public int Level { get; set; } = 1;

        [BsonElement("Escapes")]
        public int Escapes { get; set; } = 0;

        [BsonElement("Fails")]
        public int Fails { get; set; } = 0;
    }
}

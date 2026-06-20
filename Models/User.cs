using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace API.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; } = null!;

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; } = null!;

        [BsonElement("Roles")]
        public List<string> Roles { get; set; } = new List<string> { "Player" };
    }
}

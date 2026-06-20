using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace API.Models
{
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("roomName")]
        public string RoomName { get; set; } = null!;

        [BsonElement("maxPlayers")]
        public int MaxPlayers { get; set; } = 4;

        [BsonElement("currentPlayers")]
        public List<string> CurrentPlayers { get; set; } = new List<string>();

        [BsonElement("hostPlayerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string HostPlayerId { get; set; } = null!;

        [BsonElement("hostAddress")]
        public string HostAddress { get; set; } = "127.0.0.1";

        [BsonElement("port")]
        public int Port { get; set; } = 7777;

        [BsonElement("status")]
        public string Status { get; set; } = "Waiting"; // Waiting, Playing, Finished

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace API.Models
{
    public class Inventory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("PlayerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PlayerId { get; set; } = null!;

        [BsonElement("Items")]
        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();
    }

    public class InventoryItem
    {
        [BsonElement("ItemId")]
        public string ItemId { get; set; } = null!;

        [BsonElement("Name")]
        public string Name { get; set; } = null!;

        [BsonElement("Quantity")]
        public int Quantity { get; set; }
    }
}

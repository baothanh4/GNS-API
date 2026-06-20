using API.Config;
using Microsoft.Extensions.Options;
using API.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public class RoomService
    {
        private readonly IMongoCollection<Room> _rooms;

        public RoomService(MongoDbService mongoDbService, IOptions<MongoDbSettings> settings)
        {
            _rooms = mongoDbService.GetCollection<Room>(settings.Value.RoomsCollectionName);
        }

        public async Task<List<Room>> GetRoomsAsync() =>
            await _rooms.Find(_ => true).ToListAsync();

        public async Task<Room?> GetRoomByIdAsync(string id) =>
            await _rooms.Find(r => r.Id == id).FirstOrDefaultAsync();

        public async Task<Room> CreateRoomAsync(string roomName, int maxPlayers)
        {
            var room = new Room
            {
                RoomName = roomName,
                MaxPlayers = maxPlayers,
                CurrentPlayers = new List<string>(),
                Status = "Waiting"
            };

            await _rooms.InsertOneAsync(room);
            return room;
        }

        public async Task<bool> JoinRoomAsync(string roomId, string userId)
        {
            var room = await GetRoomByIdAsync(roomId);
            if (room == null) return false;

            if (room.CurrentPlayers.Contains(userId)) return true;
            if (room.CurrentPlayers.Count >= room.MaxPlayers) return false;

            var update = Builders<Room>.Update.Push(r => r.CurrentPlayers, userId);
            var result = await _rooms.UpdateOneAsync(r => r.Id == roomId, update);

            return result.ModifiedCount > 0;
        }

        public async Task<bool> LeaveRoomAsync(string roomId, string userId)
        {
            var room = await GetRoomByIdAsync(roomId);
            if (room == null) return false;

            if (!room.CurrentPlayers.Contains(userId)) return true;

            var update = Builders<Room>.Update.Pull(r => r.CurrentPlayers, userId);
            var result = await _rooms.UpdateOneAsync(r => r.Id == roomId, update);

            if (result.ModifiedCount > 0)
            {
                // If room is empty, delete it
                var updatedRoom = await GetRoomByIdAsync(roomId);
                if (updatedRoom != null && updatedRoom.CurrentPlayers.Count == 0)
                {
                    await _rooms.DeleteOneAsync(r => r.Id == roomId);
                }

                return true;
            }

            return false;
        }

        public async Task UpdateStatusAsync(string roomId, string status)
        {
            var update = Builders<Room>.Update.Set(r => r.Status, status);
            await _rooms.UpdateOneAsync(r => r.Id == roomId, update);
        }
    }
}

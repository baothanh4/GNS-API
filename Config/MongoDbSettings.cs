namespace API.Config
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UsersCollectionName { get; set; } = null!;
        public string PlayerProfilesCollectionName { get; set; } = null!;
        public string InventoriesCollectionName { get; set; } = null!;
        public string GameScoresCollectionName { get; set; } = null!;
        public string RoomsCollectionName { get; set; } = null!;
    }
}

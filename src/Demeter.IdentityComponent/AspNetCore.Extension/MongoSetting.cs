namespace Demeter.IdentityComponent.AspNetCore.Extension
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string UserCollection { get; set; }
        public string RoleCollection { get; set; }
    }
}
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PeerLearn.Api.Models;

namespace PeerLearn.Api.Data;

public class MongoSettings { public string ConnectionString { get; set; } = ""; public string Database { get; set; } = ""; }

public class MongoContext
{
    public IMongoCollection<User> Users { get; }
    public IMongoCollection<TutoringSession> Sessions { get; }
    public IMongoCollection<Message> Messages { get; }
    public IMongoCollection<Review> Reviews { get; }
    public IMongoCollection<Notification> Notifications { get; }

    public MongoContext(IOptions<MongoSettings> options)
    {
        var db = new MongoClient(options.Value.ConnectionString).GetDatabase(options.Value.Database);
        Users = db.GetCollection<User>("users");
        Sessions = db.GetCollection<TutoringSession>("sessions");
        Messages = db.GetCollection<Message>("messages");
        Reviews = db.GetCollection<Review>("reviews");
        Notifications = db.GetCollection<Notification>("notifications");

        Users.Indexes.CreateOne(new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(u => u.Email), new CreateIndexOptions { Unique = true }));
        Sessions.Indexes.CreateOne(new CreateIndexModel<TutoringSession>(
            Builders<TutoringSession>.IndexKeys.Ascending(s => s.TutorId).Ascending(s => s.StartsAt)));
        Messages.Indexes.CreateOne(new CreateIndexModel<Message>(
            Builders<Message>.IndexKeys.Ascending(m => m.FromId).Ascending(m => m.ToId).Descending(m => m.SentAt)));
    }
}

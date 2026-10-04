using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PeerLearn.Api.Models;

public abstract class Entity
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
}

/// <summary>A student. Anyone can learn; users with IsTutor = true appear in tutor search.</summary>
public class User : Entity
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Year { get; set; } = "";
    public string Bio { get; set; } = "";
    public bool IsTutor { get; set; }
    public List<string> Courses { get; set; } = new();
    public double RatingAvg { get; set; }
    public int RatingCount { get; set; }
    public int SessionsTaught { get; set; }
    /// <summary>Weekly schedule the tutor chose. Empty = not bookable.</summary>
    public List<AvailabilityRule> Availability { get; set; } = new();
}

/// <summary>One weekly window, e.g. Monday 09:00-12:00 (Day: 0 = Sunday ... 6 = Saturday, times in UTC "HH:mm").</summary>
public class AvailabilityRule
{
    public int Day { get; set; }
    public string Start { get; set; } = "09:00";
    public string End { get; set; } = "12:00";
}

public enum SessionStatus { Pending, Confirmed, Declined, Completed }

public class TutoringSession : Entity
{
    public string LearnerId { get; set; } = "";
    public string LearnerName { get; set; } = "";
    public string TutorId { get; set; } = "";
    public string TutorName { get; set; } = "";
    public string Course { get; set; } = "";
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] public DateTime StartsAt { get; set; }
    [BsonRepresentation(BsonType.String)] public SessionStatus Status { get; set; } = SessionStatus.Pending;
    public bool Rated { get; set; }
}

public class Message : Entity
{
    public string FromId { get; set; } = "";
    public string ToId { get; set; } = "";
    public string Text { get; set; } = "";
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

public class Review : Entity
{
    public string SessionId { get; set; } = "";
    public string TutorId { get; set; } = "";
    public string LearnerId { get; set; } = "";
    public int Rating { get; set; }
    public string Comment { get; set; } = "";
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Notification : Entity
{
    public string UserId { get; set; } = "";
    public string Text { get; set; } = "";
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using PeerLearn.Api.Dtos;
using PeerLearn.Api.Models;

namespace PeerLearn.Api.Common;

public static class Extensions
{
    public static string Uid(this ClaimsPrincipal p) => p.FindFirstValue("uid")!;
    public static bool ValidId(string? id) => ObjectId.TryParse(id, out _);

    public static SessionDto ToDto(this TutoringSession s) =>
        new(s.Id, s.Course, s.TutorId, s.TutorName, s.LearnerId, s.LearnerName, s.StartsAt, s.Status, s.Rated);
}

/// <summary>Tells SignalR which user a connection belongs to (our "uid" claim).</summary>
public class UidProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext c) => c.User?.FindFirst("uid")?.Value;
}

/// <summary>Turns a tutor's weekly availability into bookable one-hour slots for the next 14 days (UTC).</summary>
public static class Slots
{
    public static List<DateTime> For(IEnumerable<AvailabilityRule> rules, int days = 14)
    {
        var now = DateTime.UtcNow;
        var set = new SortedSet<DateTime>();
        foreach (var rule in rules)
        {
            if (!TimeSpan.TryParse(rule.Start, out var from) || !TimeSpan.TryParse(rule.End, out var to)) continue;
            for (var d = 0; d < days; d++)
            {
                var date = now.Date.AddDays(d);
                if ((int)date.DayOfWeek != rule.Day) continue;
                for (var t = from; t + TimeSpan.FromHours(1) <= to; t += TimeSpan.FromHours(1))
                {
                    var start = DateTime.SpecifyKind(date + t, DateTimeKind.Utc);
                    if (start > now) set.Add(start);
                }
            }
        }
        return set.ToList();
    }
}

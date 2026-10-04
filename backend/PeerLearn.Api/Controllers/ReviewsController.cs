using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PeerLearn.Api.Common;
using PeerLearn.Api.Data;
using PeerLearn.Api.Dtos;
using PeerLearn.Api.Models;

namespace PeerLearn.Api.Controllers;

[ApiController, Authorize, Route("api/reviews")]
public class ReviewsController(MongoContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(ReviewRequest r)
    {
        var me = User.Uid();
        if (r.Rating is < 1 or > 5) return BadRequest(new { message = "Rating must be between 1 and 5." });
        if (!Extensions.ValidId(r.SessionId)) return NotFound();

        var s = await db.Sessions.Find(x => x.Id == r.SessionId && x.LearnerId == me).FirstOrDefaultAsync();
        if (s is null) return NotFound();
        if (s.Status != SessionStatus.Completed) return Conflict(new { message = "You can rate a session once it is complete." });
        if (s.Rated) return Conflict(new { message = "You already rated this session." });

        await db.Reviews.InsertOneAsync(new Review { SessionId = s.Id, TutorId = s.TutorId, LearnerId = me, Rating = r.Rating, Comment = r.Comment?.Trim() ?? "" });
        await db.Sessions.UpdateOneAsync(x => x.Id == s.Id, Builders<TutoringSession>.Update.Set(x => x.Rated, true));

        // Running average: (avg * n + rating) / (n + 1)
        var tutor = await db.Users.Find(u => u.Id == s.TutorId).FirstAsync();
        var count = tutor.RatingCount + 1;
        var avg = (tutor.RatingAvg * tutor.RatingCount + r.Rating) / count;
        await db.Users.UpdateOneAsync(u => u.Id == tutor.Id, Builders<User>.Update.Set(u => u.RatingAvg, avg).Set(u => u.RatingCount, count));
        await db.Notifications.InsertOneAsync(new Notification { UserId = tutor.Id, Text = $"{s.LearnerName} rated your {s.Course} session {r.Rating}★" });
        return NoContent();
    }
}

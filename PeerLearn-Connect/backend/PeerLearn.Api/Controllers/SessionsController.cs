using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PeerLearn.Api.Common;
using PeerLearn.Api.Data;
using PeerLearn.Api.Dtos;
using PeerLearn.Api.Models;

namespace PeerLearn.Api.Controllers;

[ApiController, Authorize, Route("api/sessions")]
public class SessionsController(MongoContext db) : ControllerBase
{
    Task Notify(string userId, string text) => db.Notifications.InsertOneAsync(new Notification { UserId = userId, Text = text });

    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var me = User.Uid();
        var list = await db.Sessions.Find(s => s.LearnerId == me || s.TutorId == me).SortBy(s => s.StartsAt).ToListAsync();
        return Ok(list.Select(s => s.ToDto()));
    }

    [HttpPost]
    public async Task<IActionResult> Book(BookRequest r)
    {
        var me = User.Uid();
        if (!Extensions.ValidId(r.TutorId) || r.TutorId == me) return BadRequest(new { message = "Choose a different tutor." });

        var tutor = await db.Users.Find(u => u.Id == r.TutorId && u.IsTutor).FirstOrDefaultAsync();
        if (tutor is null) return NotFound(new { message = "Tutor not found." });
        if (!tutor.Courses.Contains(r.Course)) return BadRequest(new { message = $"{tutor.Name} doesn't tutor {r.Course}." });

        var start = r.StartsAt.ToUniversalTime();
        if (!Slots.For(tutor.Availability).Contains(start)) return BadRequest(new { message = "That time is not available." });

        var clash = (await db.Sessions.Find(s => s.TutorId == tutor.Id && s.StartsAt == start).ToListAsync())
            .Any(s => s.Status is SessionStatus.Pending or SessionStatus.Confirmed);
        if (clash) return Conflict(new { message = "Someone just booked that time. Pick another slot." });

        var learner = await db.Users.Find(u => u.Id == me).FirstAsync();
        var session = new TutoringSession
        {
            LearnerId = me, LearnerName = learner.Name, TutorId = tutor.Id, TutorName = tutor.Name,
            Course = r.Course, StartsAt = start
        };
        await db.Sessions.InsertOneAsync(session);
        await Notify(tutor.Id, $"{learner.Name} requested {r.Course} on {start:ddd d MMM} at {start:HH:mm}");
        return Ok(session.ToDto());
    }

    [HttpPost("{id}/respond")]
    public async Task<IActionResult> Respond(string id, RespondRequest r)
    {
        var s = await FindFor(id, tutorOnly: true);
        if (s is null) return NotFound();
        if (s.Status != SessionStatus.Pending) return Conflict(new { message = "This request was already answered." });

        s.Status = r.Accept ? SessionStatus.Confirmed : SessionStatus.Declined;
        await db.Sessions.ReplaceOneAsync(x => x.Id == s.Id, s);
        await Notify(s.LearnerId, $"{s.TutorName} {(r.Accept ? "confirmed" : "declined")} your {s.Course} session");
        return Ok(s.ToDto());
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(string id)
    {
        var s = await FindFor(id, tutorOnly: true);
        if (s is null) return NotFound();
        if (s.Status != SessionStatus.Confirmed) return Conflict(new { message = "Only confirmed sessions can be completed." });

        s.Status = SessionStatus.Completed;
        await db.Sessions.ReplaceOneAsync(x => x.Id == s.Id, s);
        await db.Users.UpdateOneAsync(u => u.Id == s.TutorId, Builders<User>.Update.Inc(u => u.SessionsTaught, 1));
        await Notify(s.LearnerId, $"Your {s.Course} session is complete. Leave a rating for {s.TutorName}.");
        return Ok(s.ToDto());
    }

    async Task<TutoringSession?> FindFor(string id, bool tutorOnly)
    {
        if (!Extensions.ValidId(id)) return null;
        var me = User.Uid();
        return await db.Sessions.Find(s => s.Id == id && (tutorOnly ? s.TutorId == me : s.TutorId == me || s.LearnerId == me)).FirstOrDefaultAsync();
    }
}

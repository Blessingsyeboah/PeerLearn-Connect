using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using PeerLearn.Api.Common;
using PeerLearn.Api.Data;
using PeerLearn.Api.Dtos;
using PeerLearn.Api.Models;

namespace PeerLearn.Api.Controllers;

[ApiController, Authorize, Route("api/tutors")]
public class TutorsController(MongoContext db) : ControllerBase
{
    static bool Active(TutoringSession s) => s.Status == SessionStatus.Pending || s.Status == SessionStatus.Confirmed;

    /// <summary>Only tutors who switched tutoring on, set courses and a schedule, and still have a free slot.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? course, [FromQuery] string? q, [FromQuery] string sort = "rating")
    {
        var me = User.Uid();
        var f = Builders<User>.Filter;
        var filter = f.Eq(u => u.IsTutor, true) & f.SizeGt(u => u.Courses, 0) & f.SizeGt(u => u.Availability, 0) & f.Ne(u => u.Id, me);
        if (!string.IsNullOrWhiteSpace(course)) filter &= f.AnyEq(u => u.Courses, course);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var rx = new BsonRegularExpression(Regex.Escape(q.Trim()), "i");
            filter &= f.Or(f.Regex(u => u.Name, rx), f.Regex("Courses", rx));
        }
        var tutors = await db.Users.Find(filter).ToListAsync();

        var ids = tutors.Select(t => t.Id).ToList();
        var now = DateTime.UtcNow;
        var booked = (await db.Sessions.Find(s => ids.Contains(s.TutorId) && s.StartsAt > now).ToListAsync())
            .Where(Active).ToLookup(s => s.TutorId, s => s.StartsAt);

        var list = tutors.Select(t =>
        {
            var taken = booked[t.Id].ToHashSet();
            DateTime? next = Slots.For(t.Availability).Where(s => !taken.Contains(s)).Select(s => (DateTime?)s).FirstOrDefault();
            return new TutorDto(t.Id, t.Name, t.Year, t.Bio, t.Courses, Math.Round(t.RatingAvg, 1), t.SessionsTaught, next, t.Availability);
        }).Where(t => t.NextSlot is not null);

        list = sort switch
        {
            "soon" => list.OrderBy(t => t.NextSlot),
            "sessions" => list.OrderByDescending(t => t.Sessions),
            _ => list.OrderByDescending(t => t.Rating),
        };
        return Ok(list.ToList());
    }

    [HttpGet("courses")]
    public async Task<IActionResult> Courses()
    {
        var f = Builders<User>.Filter;
        var all = await db.Users.Find(f.Eq(u => u.IsTutor, true) & f.SizeGt(u => u.Availability, 0)).Project(u => u.Courses).ToListAsync();
        return Ok(all.SelectMany(c => c).Distinct().OrderBy(c => c).ToList());
    }

    [HttpGet("{id}/slots")]
    public async Task<IActionResult> GetSlots(string id)
    {
        if (!Extensions.ValidId(id)) return NotFound();
        var tutor = await db.Users.Find(u => u.Id == id && u.IsTutor).FirstOrDefaultAsync();
        if (tutor is null) return NotFound();

        var now = DateTime.UtcNow;
        var taken = (await db.Sessions.Find(s => s.TutorId == id && s.StartsAt > now).ToListAsync())
            .Where(Active).Select(s => s.StartsAt).ToHashSet();
        return Ok(Slots.For(tutor.Availability).Select(s => new SlotDto(s, !taken.Contains(s))).ToList());
    }
}

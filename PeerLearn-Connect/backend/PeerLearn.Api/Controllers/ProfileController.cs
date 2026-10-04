using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PeerLearn.Api.Common;
using PeerLearn.Api.Data;
using PeerLearn.Api.Dtos;
using PeerLearn.Api.Models;

namespace PeerLearn.Api.Controllers;

/// <summary>Lets any user become a tutor: courses, bio and a weekly schedule. The same account keeps learning as before.</summary>
[ApiController, Authorize, Route("api/profile")]
public class ProfileController(MongoContext db) : ControllerBase
{
    static ProfileDto ToDto(User u) => new(u.Id, u.Name, u.Email, u.IsTutor, u.Year, u.Bio, u.Courses, u.Availability);

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var me = User.Uid();
        var u = await db.Users.Find(x => x.Id == me).FirstOrDefaultAsync();
        return u is null ? Unauthorized() : Ok(ToDto(u));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateProfileRequest r)
    {
        var me = User.Uid();
        var courses = (r.Courses ?? new()).Select(c => c.Trim()).Where(c => c.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var rules = new List<AvailabilityRule>();
        foreach (var a in r.Availability ?? new())
        {
            if (a.Day is < 0 or > 6 || !TimeSpan.TryParse(a.Start, out var from) || !TimeSpan.TryParse(a.End, out var to) || to - from < TimeSpan.FromHours(1))
                return BadRequest(new { message = "Each time window needs a valid day and must be at least one hour long." });
            rules.Add(new AvailabilityRule { Day = a.Day, Start = from.ToString(@"hh\:mm"), End = to.ToString(@"hh\:mm") });
        }

        if (r.IsTutor && courses.Count == 0) return BadRequest(new { message = "Add at least one course you can teach." });
        if (r.IsTutor && rules.Count == 0) return BadRequest(new { message = "Add at least one time window when you're free." });

        await db.Users.UpdateOneAsync(x => x.Id == me, Builders<User>.Update
            .Set(x => x.IsTutor, r.IsTutor)
            .Set(x => x.Year, r.Year?.Trim() ?? "")
            .Set(x => x.Bio, r.Bio?.Trim() ?? "")
            .Set(x => x.Courses, courses)
            .Set(x => x.Availability, rules));

        var u = await db.Users.Find(x => x.Id == me).FirstAsync();
        return Ok(ToDto(u));
    }
}

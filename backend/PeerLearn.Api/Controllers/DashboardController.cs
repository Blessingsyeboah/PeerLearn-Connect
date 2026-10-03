using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PeerLearn.Api.Common;
using PeerLearn.Api.Data;
using PeerLearn.Api.Dtos;
using PeerLearn.Api.Models;

namespace PeerLearn.Api.Controllers;

[ApiController, Authorize, Route("api/dashboard")]
public class DashboardController(MongoContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var me = User.Uid();
        var now = DateTime.UtcNow.AddHours(-1);
        var all = await db.Sessions.Find(s => s.LearnerId == me || s.TutorId == me).SortBy(s => s.StartsAt).ToListAsync();

        var upcoming = all.Where(s => s.StartsAt >= now && s.Status is SessionStatus.Pending or SessionStatus.Confirmed).Select(s => s.ToDto()).ToList();
        var requests = all.Where(s => s.TutorId == me && s.Status == SessionStatus.Pending).Select(s => s.ToDto()).ToList();
        var done = all.Where(s => s.LearnerId == me && s.Status == SessionStatus.Completed).ToList();
        var toRate = done.FirstOrDefault(s => !s.Rated)?.ToDto();

        var notes = await db.Notifications.Find(n => n.UserId == me).SortByDescending(n => n.CreatedAt).Limit(6).ToListAsync();
        return Ok(new DashboardDto(upcoming, requests, done.Count, toRate, notes.Select(n => new NotificationDto(n.Id, n.Text, n.CreatedAt)).ToList()));
    }
}

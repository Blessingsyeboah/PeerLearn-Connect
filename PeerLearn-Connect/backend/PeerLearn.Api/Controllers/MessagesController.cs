using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PeerLearn.Api.Common;
using PeerLearn.Api.Data;
using PeerLearn.Api.Dtos;

namespace PeerLearn.Api.Controllers;

/// <summary>Chat history over REST. Sending and live delivery happen through ChatHub.</summary>
[ApiController, Authorize, Route("api/messages")]
public class MessagesController(MongoContext db) : ControllerBase
{
    [HttpGet("threads")]
    public async Task<IActionResult> Threads()
    {
        var me = User.Uid();
        var msgs = await db.Messages.Find(m => m.FromId == me || m.ToId == me).SortByDescending(m => m.SentAt).Limit(500).ToListAsync();
        string Other(Models.Message m) => m.FromId == me ? m.ToId : m.FromId;

        var latest = msgs.GroupBy(Other).Select(g => g.First()).ToList();
        var ids = latest.Select(Other).ToList();
        var names = (await db.Users.Find(u => ids.Contains(u.Id)).ToListAsync()).ToDictionary(u => u.Id, u => u.Name);
        return Ok(latest.Select(m => new ThreadDto(Other(m), names.GetValueOrDefault(Other(m), "Unknown"), m.Text, m.SentAt)).ToList());
    }

    [HttpGet("with/{userId}")]
    public async Task<IActionResult> History(string userId)
    {
        if (!Extensions.ValidId(userId)) return NotFound();
        var me = User.Uid();
        var msgs = await db.Messages.Find(m => (m.FromId == me && m.ToId == userId) || (m.FromId == userId && m.ToId == me))
            .SortBy(m => m.SentAt).Limit(200).ToListAsync();
        var names = (await db.Users.Find(u => u.Id == me || u.Id == userId).ToListAsync()).ToDictionary(u => u.Id, u => u.Name);
        return Ok(msgs.Select(m => new MessageDto(m.Id, m.FromId, m.ToId, names.GetValueOrDefault(m.FromId, ""), names.GetValueOrDefault(m.ToId, ""), m.Text, m.SentAt)).ToList());
    }
}

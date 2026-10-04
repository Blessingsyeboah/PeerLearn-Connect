using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using PeerLearn.Api.Common;
using PeerLearn.Api.Data;
using PeerLearn.Api.Dtos;
using PeerLearn.Api.Models;

namespace PeerLearn.Api.Hubs;

[Authorize]
public class ChatHub(MongoContext db) : Hub
{
    /// <summary>Saves the message, then pushes it to both the sender and recipient (every open tab).</summary>
    public async Task SendMessage(string toUserId, string text)
    {
        text = text?.Trim() ?? "";
        if (text.Length is 0 or > 2000) throw new HubException("Messages must be 1 to 2000 characters.");

        var me = Context.UserIdentifier!;
        if (me == toUserId || !Extensions.ValidId(toUserId)) throw new HubException("Choose someone else to message.");

        var users = await db.Users.Find(u => u.Id == me || u.Id == toUserId).ToListAsync();
        var from = users.FirstOrDefault(u => u.Id == me);
        var to = users.FirstOrDefault(u => u.Id == toUserId);
        if (from is null || to is null) throw new HubException("User not found.");

        var m = new Message { FromId = me, ToId = toUserId, Text = text };
        await db.Messages.InsertOneAsync(m);
        await Clients.Users(new[] { me, toUserId }).SendAsync("ReceiveMessage", new MessageDto(m.Id, me, toUserId, from.Name, to.Name, text, m.SentAt));
    }
}

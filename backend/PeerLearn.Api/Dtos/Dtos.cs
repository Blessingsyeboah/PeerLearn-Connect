using PeerLearn.Api.Models;

namespace PeerLearn.Api.Dtos;

public record RegisterRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record UserDto(string Id, string Name, string Email, bool IsTutor);
public record AuthResponse(string Token, UserDto User);

public record TutorDto(string Id, string Name, string Year, string Bio, List<string> Courses, double Rating, int Sessions, DateTime? NextSlot, List<AvailabilityRule> Schedule);
public record SlotDto(DateTime StartsAt, bool Available);

public record BookRequest(string TutorId, string Course, DateTime StartsAt);
public record RespondRequest(bool Accept);
public record ReviewRequest(string SessionId, int Rating, string? Comment);
public record SessionDto(string Id, string Course, string TutorId, string TutorName, string LearnerId, string LearnerName, DateTime StartsAt, SessionStatus Status, bool Rated);

public record MessageDto(string Id, string FromId, string ToId, string FromName, string ToName, string Text, DateTime SentAt);
public record ThreadDto(string UserId, string Name, string LastText, DateTime LastAt);

public record NotificationDto(string Id, string Text, DateTime CreatedAt);
public record DashboardDto(List<SessionDto> Upcoming, List<SessionDto> Requests, int Completed, SessionDto? ToRate, List<NotificationDto> Notifications);

public record ProfileDto(string Id, string Name, string Email, bool IsTutor, string Year, string Bio, List<string> Courses, List<AvailabilityRule> Availability);
public record UpdateProfileRequest(bool IsTutor, string? Year, string? Bio, List<string>? Courses, List<AvailabilityRule>? Availability);

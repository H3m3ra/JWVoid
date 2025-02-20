namespace PanelDiscussionManager.Domain.Entities;

public record Question(string Topic, string Content, TimeSpan ModerationTime);
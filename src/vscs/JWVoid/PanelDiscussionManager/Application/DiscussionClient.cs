using PanelDiscussionManager.Domain.BuisnessObjects;

namespace PanelDiscussionManager.Application;

public class DiscussionClient
{
    public Discussant Owner { get; protected set; }

    public QuestionRound? Round { set; get; }

    private TimeSpan speechTime;
    private DateTime lastStart;

    public DiscussionClient(Discussant owner)
    {
        Owner = owner;
        speechTime = default;
    }

    // methods
    public bool Start()
    {
        if (Round == null && Owner == Round.CurrentDiscussant) return false;
        lastStart = DateTime.Now;
        return true;
    }
}
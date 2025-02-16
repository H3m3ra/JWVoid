namespace PanelDiscussionManager.Interfaces;

public interface IDiscussion
{
    public IList<Discussant> Discussants { get; set; }
    public IList<Question> Questions { get; set; }

    // methods
    public bool Start();
    public bool Finish();

    // update-methods
    public bool UpdateQuestionRounds();
}
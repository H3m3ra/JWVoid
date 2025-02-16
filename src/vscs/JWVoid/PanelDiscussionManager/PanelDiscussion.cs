using PanelDiscussionManager.Interfaces;

namespace PanelDiscussionManager;

public class PanelDiscussion : IDiscussion
{
    protected readonly IDiscussantOrderCalculatorService discussantOrderCalculatorService;

    public IList<Discussant> Discussants
    {
        get { return started ? discussants.ToList() : discussants; }
        set { if (!started) discussants = value; }
    }
    public IList<Question> Questions
    {
        get { return started ? questions.ToList() : questions; }
        set { if (!started) questions = value; }
    }

    protected IList<Discussant> discussants = new List<Discussant>();
    protected IList<Question> questions = new List<Question>();
    protected bool started = false;

    public PanelDiscussion(IDiscussantOrderCalculatorService discussantOrderCalculatorService)
    {
        this.discussantOrderCalculatorService = discussantOrderCalculatorService;
    }

    // methods
    public bool Start()
    {
        started = true;
        return true;
    }

    public bool Finish()
    {
        started = false;
        return true;
    }

    // update-methods
    public bool UpdateQuestionRounds()
    {
        if (started) return false;

        var order = discussantOrderCalculatorService.CalculateDiscussantOrder(Discussants.Count(), Questions.Count());
        foreach (var ord in order)
        {
            Console.WriteLine(string.Join("-", ord.Select(o => o.ToString())));
        }

        return true;
    }
}
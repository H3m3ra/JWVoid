using PanelDiscussionManager.Domain.BuisnessObjects;
using PanelDiscussionManager.Domain.Entities;
using PanelDiscussionManager.Domain.Interfaces;

namespace PanelDiscussionManager.Application;

public class DiscussionServer
{
    protected readonly IDiscussantOrderCalculatorService discussantOrderCalculatorService;

    public ISet<Person> Persons
    {
        get { return started ? new HashSet<Person>(persons) : persons; }
        set { if (!started) persons = value; }
    }

    public IList<Question> Questions
    {
        get { return started ? questions.ToList() : questions; }
        set { if (!started) questions = value; }
    }

    public TimeSpan DiscussionEventDuration
    {
        get { return started ? new TimeSpan(discussionEventDuration.Ticks) : discussionEventDuration; }
        set { if (!started) discussionEventDuration = value; }
    }

    public TimeSpan DiscussionDuration
    {
        get { return discussionEventDuration-new TimeSpan(Questions.Select(q => q.ModerationTime.Ticks).Sum()); }
        set { if (!started) discussionEventDuration = value+new TimeSpan(Questions.Select(q => q.ModerationTime.Ticks).Sum()); }
    }

    public ISet<Discussant> Discussants { get; protected set; } = new HashSet<Discussant>();

    public QuestionRound[] Rounds { get; protected set; } = Array.Empty<QuestionRound>();

    protected ISet<Person> persons = new HashSet<Person>();
    protected IList<Question> questions = new List<Question>();
    protected TimeSpan discussionEventDuration = default;
    protected bool started = false;

    public DiscussionServer(IDiscussantOrderCalculatorService discussantOrderCalculatorService)
    {
        this.discussantOrderCalculatorService = discussantOrderCalculatorService;
    }

    // methods
    public bool Start()
    {
        if (started || !PreparedRoundsValid()) return false;

        started = true;

        return true;
    }

    // update-methods
    public bool Update()
    {
        Discussants = new HashSet<Discussant>();
        Rounds = Array.Empty<QuestionRound>();

        if (!started && questions.Count() == 0 || persons.Count() == 0 && DiscussionDuration > new TimeSpan(0)) return false;

        var min = 0.75;

        var ticksPerPosition = DiscussionDuration.Ticks / (Questions.Count() * Persons.Count());
        var ticksPerDiscussant = Questions.Count() * ticksPerPosition;

        var allowedDurationMin = new TimeSpan((long)Math.Ceiling(min * ticksPerPosition));
        if (allowedDurationMin < new TimeSpan(0, 0, 30)) return false;

        var allowedDuration = new TimeSpan(ticksPerDiscussant);
        Discussants = new HashSet<Discussant>(Persons.Select(p => new Discussant(p, allowedDuration)));

        var allowedDurationMax = allowedDuration - new TimeSpan((Questions.Count()-1) * allowedDurationMin.Ticks);
        Rounds = discussantOrderCalculatorService.CalculateDiscussantOrder(Discussants, questions.Count())
                                                 .Select((round, i) => new QuestionRound(questions[i], allowedDurationMin, allowedDurationMax, round.ToArray()))
                                                 .ToArray();
        return true;
    }

    // get-methods
    protected bool PreparedRoundsValid()
    {
        return Questions.Count() > 0 && Persons.Count() > 0 && Discussants.Count() == Persons.Count() && Rounds.Count() == Questions.Count()
            && Rounds.Select((r, i) => Tuple.Create(i, r))
                     .All(e => e.Item2.IsValid(Questions[e.Item1], Discussants));
    }
}
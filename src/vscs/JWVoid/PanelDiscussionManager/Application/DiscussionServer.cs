using PanelDiscussionManager.Domain.BuisnessObjects;
using PanelDiscussionManager.Domain.Entities;
using PanelDiscussionManager.Domain.Interfaces;

namespace PanelDiscussionManager.Application;

public class DiscussionServer
{
    protected readonly IDiscussantOrderCalculatorService discussantOrderCalculatorService;

    public ISet<Person> Persons
    {
        get { return HasCurrentRound() ? new HashSet<Person>(persons) : persons; }
        set { if (!HasCurrentRound()) persons = value; }
    }

    public IList<Question> Questions
    {
        get { return HasCurrentRound() ? questions.ToList() : questions; }
        set { if (!HasCurrentRound()) questions = value; }
    }

    public TimeSpan DiscussionEventDuration
    {
        get { return HasCurrentRound() ? new TimeSpan(discussionEventDuration.Ticks) : discussionEventDuration; }
        set { if (!HasCurrentRound()) discussionEventDuration = value; }
    }

    public TimeSpan DiscussionDuration
    {
        get { return discussionEventDuration - new TimeSpan(Questions.Select(q => q.ModerationTime.Ticks).Sum()); }
        set { if (!HasCurrentRound()) discussionEventDuration = value + new TimeSpan(Questions.Select(q => q.ModerationTime.Ticks).Sum()); }
    }

    public ISet<Discussant> Discussants { get; protected set; } = new HashSet<Discussant>();

    public QuestionRound[] Rounds { get; protected set; } = Array.Empty<QuestionRound>();

    public QuestionRound? CurrentRound { get { return HasCurrentRound() ? Rounds[questionRoundIndex] : null; } }

    protected ISet<Person> persons = new HashSet<Person>();
    protected IList<Question> questions = new List<Question>();
    protected TimeSpan discussionEventDuration = default;
    protected int questionRoundIndex = -1;

    public DiscussionServer(IDiscussantOrderCalculatorService discussantOrderCalculatorService)
    {
        this.discussantOrderCalculatorService = discussantOrderCalculatorService;

        Reset();
    }

    // methods
    public bool Start()
    {
        if (questionRoundIndex < 0 && !PreparedRoundsValid()) return false;
        return StartNextRound();
    }

    public Tuple<bool, bool> NextQuestion(TimeSpan usedDuration)
    {
        var currentDiscussant = CurrentRound!.CurrentDiscussant!;

        var result = CurrentRound!.Next(usedDuration);
        if (!result.Item1) return result;

        var remainingDurationPerQuestion = new TimeSpan(currentDiscussant.AllowedDuration.Ticks / (Questions.Count() - 1 - questionRoundIndex));

        foreach (var round in Rounds.Skip(questionRoundIndex + 1))
        {
            round.GetDiscussantTimesOf(currentDiscussant.Person)!.AllowedDurationMax = remainingDurationPerQuestion;
        }

        if (!CurrentRound!.HasChosenDiscussant())
        {
            if (!StartNextRound()) return Tuple.Create(true, false);
        }

        return Tuple.Create(true, true);
    }

    protected bool StartNextRound()
    {
        questionRoundIndex++;
        return HasCurrentRound() && CurrentRound!.Start();
    }

    // reset-methods
    public void Reset()
    {
        questionRoundIndex = -1;
        Update();
    }

    // update-methods
    public bool Update()
    {
        Discussants = new HashSet<Discussant>();
        Rounds = Array.Empty<QuestionRound>();

        if (!HasCurrentRound() && questions.Count() == 0 || persons.Count() == 0 && DiscussionDuration > new TimeSpan(0)) return false;

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
    public bool HasCurrentRound()
    {
        return 0 <= questionRoundIndex && questionRoundIndex < questions.Count();
    }

    protected bool PreparedRoundsValid()
    {
        return Questions.Count() > 0 && Persons.Count() > 0 && Discussants.Count() == Persons.Count() && Rounds.Count() == Questions.Count()
            && Rounds.Select((r, i) => Tuple.Create(i, r))
                     .All(e => e.Item2.IsValid(Questions[e.Item1], Discussants));
    }
}
using PanelDiscussionManager.Domain.Entities;

namespace PanelDiscussionManager.Domain.BuisnessObjects;

public class QuestionRound
{
    public Question Question { get; protected set; }

    public Discussant[] Discussants { get; protected set; }

    public DiscussantTimes[] DiscussantsTimes { get; protected set; }

    public Discussant? CurrentDiscussant { get { return (_discussantIndex < 0 || Discussants.Length <= _discussantIndex ? null : Discussants[_discussantIndex]); } }

    public DiscussantTimes? CurrentDiscussantTimes { get { return (_discussantIndex < 0 || Discussants.Length <= _discussantIndex ? null : DiscussantsTimes[_discussantIndex]); } }

    private int _discussantIndex = -1;

    public QuestionRound(Question question, TimeSpan AllowedDurationMin, TimeSpan AllowedDurationMax, Discussant[] discussants)
    {
        Question = question;
        Discussants = discussants;
        DiscussantsTimes = new DiscussantTimes[Discussants.Length].Select(v => new DiscussantTimes(AllowedDurationMin, AllowedDurationMax)).ToArray();
    }

    // methods
    public void Start()
    {
        _discussantIndex++;
        if(HasChosenDiscussant())
        {
            //var ticksPerPosition = CurrentDiscussant!.SpeechTime / (Questions.Count() * Discussants.Count());

            //var allowedTimeMin = new TimeSpan((long)Math.Ceiling(min * ticksPerPosition));

            //CurrentDiscussantTimes!
        }
    }

    // get-methods
    public bool HasChosenDiscussant()
    {
        return 0 <= _discussantIndex && _discussantIndex < Discussants.Length;
    }

    public bool IsValid(Question question, ISet<Discussant> discussants)
    {
        return Question == question
            && Discussants.Count() == discussants.Count()
            && discussants.All(d => Discussants.Contains(d));
    }
}
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

    public QuestionRound(Question question, TimeSpan allowedDurationMin, TimeSpan allowedDurationMax, Discussant[] discussants)
    {
        Reset(question, allowedDurationMin, allowedDurationMax, discussants);
    }

    // methods
    public bool Start()
    {
        if (_discussantIndex >= 0) return false;
        return NextDiscussant();
    }

    public Tuple<bool, bool> Next(TimeSpan usedDuration)
    {
        if (!HasChosenDiscussant() || CurrentDiscussantTimes!.AllowedDurationMax < usedDuration) return Tuple.Create(false, false);

        CurrentDiscussantTimes!.UsedDuration = (usedDuration < CurrentDiscussantTimes!.AllowedDurationMin ? CurrentDiscussantTimes!.AllowedDurationMin : usedDuration);
        CurrentDiscussant!.UsedDuration += usedDuration;
        CurrentDiscussant!.AllowedDuration -= usedDuration;

        return Tuple.Create(true, NextDiscussant());
    }

    protected bool NextDiscussant()
    {
        _discussantIndex++;
        return HasChosenDiscussant();
    }

    // reset-methods
    public void Reset(Question question, TimeSpan allowedDurationMin, TimeSpan allowedDurationMax, Discussant[] discussants)
    {
        Question = question;
        Discussants = discussants;
        DiscussantsTimes = new DiscussantTimes[Discussants.Length].Select(v => new DiscussantTimes(allowedDurationMin, allowedDurationMax)).ToArray();
        _discussantIndex = -1;
    }

    // get-methods
    public DiscussantTimes? GetDiscussantTimesOf(Person person)
    {
        var entry = Discussants.Select((d, i) => Tuple.Create(i, d.Person))
                               .Where(e => e.Item2.Name == person.Name)
                               .FirstOrDefault();
        return entry == null ? null : DiscussantsTimes[entry.Item1];
    }

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
using PanelDiscussionManager.Domain.Entities;

namespace PanelDiscussionManager.Domain.BuisnessObjects;

public class Discussant
{
    public Person Person { get; set; }

    public TimeSpan AllowedDuration { get; set; }

    public TimeSpan UsedDuration { get; set; }

    public Discussant(Person person, TimeSpan allowedDuration)
    {
        Person = person;
        AllowedDuration = allowedDuration;
        UsedDuration = default;
    }
}
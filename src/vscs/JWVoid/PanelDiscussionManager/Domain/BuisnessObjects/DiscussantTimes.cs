namespace PanelDiscussionManager.Domain.BuisnessObjects;

public class DiscussantTimes
{
    public TimeSpan AllowedDurationMin { get; set; }

    public TimeSpan AllowedDurationMax { get; set; }

    public TimeSpan UsedDuration { get; set; }

    public DiscussantTimes(TimeSpan allowedDurationMin, TimeSpan allowedDurationMax)
    {
        AllowedDurationMin = allowedDurationMin;
        AllowedDurationMax = allowedDurationMax;
        UsedDuration = default;
    }
}
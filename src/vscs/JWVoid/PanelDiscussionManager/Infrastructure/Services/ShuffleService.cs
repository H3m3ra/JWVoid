using PanelDiscussionManager.Infrastructure.Interfaces;

namespace PanelDiscussionManager.Infrastructure.Services;

public class ShuffleService : IShuffleService
{
    private readonly IRandomService _randomService;

    public ShuffleService(IRandomService randomService)
    {
        _randomService = randomService;
    }

    // init-methods
    public void Init() { }

    // get-methods
    public IEnumerable<T> GetShuffled<T>(IEnumerable<T> values)
    {
        var shuffledValues = new T[values.Count()];
        int vIndex = 0;
        foreach (var value in values)
        {
            // Wähle einen zufälligen Index und überspringe bereits gesetzte Indizes
            var randomIndex = _randomService.GenerateRandomInteger(0, shuffledValues.Count() - 1 - vIndex);
            var targetShuffleIndex = 0;
            while (shuffledValues[targetShuffleIndex] != null || randomIndex > 0)
            {
                if (shuffledValues[targetShuffleIndex] == null)
                {
                    randomIndex--;
                }
                targetShuffleIndex++;
            }

            shuffledValues[targetShuffleIndex] = value;
            vIndex++;
        }
        return shuffledValues;
    }
}
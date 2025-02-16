using PanelDiscussionManager.Interfaces;

namespace PanelDiscussionManager.Services;

public class RandomService : IRandomService
{
    public Random Random { get; set; }

    public RandomService()
    {

    }

    // init-methods
    public void Init()
    {
        Random = new Random();
    }

    // get-methods
    public int GenerateRandomInteger(int min, int max)
    {
        if (max < min) throw new ArgumentException("RandomService.GenerateRandomInteger: maximum must be equal or greater than minimum!");
        if (min == max) return min;
        return Random.Next(min, max+1);
    }
}
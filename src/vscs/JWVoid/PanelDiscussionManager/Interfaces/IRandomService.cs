namespace PanelDiscussionManager.Interfaces;

public interface IRandomService : IService
{
    // get-methods
    public int GenerateRandomInteger(int min, int max);
}
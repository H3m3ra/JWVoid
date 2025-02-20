namespace PanelDiscussionManager.Infrastructure.Interfaces;

public interface IShuffleService : IService
{
    // get-methods
    public IEnumerable<T> GetShuffled<T>(IEnumerable<T> values);
}
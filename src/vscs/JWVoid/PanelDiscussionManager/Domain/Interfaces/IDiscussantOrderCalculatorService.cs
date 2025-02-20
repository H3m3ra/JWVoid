using PanelDiscussionManager.Infrastructure.Interfaces;

namespace PanelDiscussionManager.Domain.Interfaces;

public interface IDiscussantOrderCalculatorService : IService
{
    // get-methods
    public IList<IList<T>> CalculateDiscussantOrder<T>(IEnumerable<T> discussants, int rounds);
}
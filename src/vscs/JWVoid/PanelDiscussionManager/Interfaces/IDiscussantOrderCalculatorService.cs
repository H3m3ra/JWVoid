namespace PanelDiscussionManager.Interfaces;

public interface IDiscussantOrderCalculatorService : IService
{
    // get-methods
    public IList<IList<int>> CalculateDiscussantOrder(int discussants, int rounds);
}
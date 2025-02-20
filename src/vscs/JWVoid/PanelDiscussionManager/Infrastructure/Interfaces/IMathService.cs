namespace PanelDiscussionManager.Infrastructure.Interfaces;

public interface IMathService : IService
{
    // get-methods
    public double GetAverage<U>(IEnumerable<U> values, Func<U, double> selector);

    public double GetStandardDeviation<U>(IEnumerable<U> values, double average, Func<U, double> selector);
}
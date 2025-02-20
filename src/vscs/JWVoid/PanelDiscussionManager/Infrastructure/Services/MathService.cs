using PanelDiscussionManager.Infrastructure.Interfaces;

namespace PanelDiscussionManager.Infrastructure.Services;

public class MathService : IMathService
{
    public MathService()
    {

    }

    // init-methods
    public void Init() { }

    // get-methods
    public double GetAverage<U>(IEnumerable<U> values, Func<U, double> selector)
    {
        return values.Sum(selector) / values.Count();
    }

    public double GetStandardDeviation<U>(IEnumerable<U> values, double average, Func<U, double> selector)
    {
        return Math.Sqrt(values.Select(v => Math.Pow(selector(v) - average, 2)).Sum() / values.Count());
    }
}
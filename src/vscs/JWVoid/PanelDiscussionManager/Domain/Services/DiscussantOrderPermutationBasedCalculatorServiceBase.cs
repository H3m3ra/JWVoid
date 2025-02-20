using PanelDiscussionManager.Domain.Interfaces;
using PanelDiscussionManager.Infrastructure.Interfaces;

namespace PanelDiscussionManager.Domain.Services;

public abstract class DiscussantOrderPermutationBasedCalculatorServiceBase : IDiscussantOrderCalculatorService
{
    protected readonly IMathService mathService;
    protected readonly IRandomService randomService;
    protected readonly IShuffleService shuffleService;

    public DiscussantOrderPermutationBasedCalculatorServiceBase(IMathService currentMathService,
                                                                IRandomService currentRandomService,
                                                                IShuffleService currentShuffleService)
    {
        mathService = currentMathService;
        randomService = currentRandomService;
        shuffleService = currentShuffleService;
    }

    // init-methods
    public virtual void Init() { }

    // get-methods
    public IList<IList<T>> CalculateDiscussantOrder<T>(IEnumerable<T> discussants, int rounds)
    {
        if (discussants.Count() < 1 || rounds < 1) return Array.Empty<IList<T>>();

        var discussantOrder = new List<IList<int>>();

        return shuffleService.GetShuffled(
                   ChoosePermutations(
                       CreatePermutations(
                           discussants//, //new int[discussants.Count()].Select((v, i) => i).ToList(),
                           //(a, b) => a == b
                       ),
                       rounds
                   )
               ).ToList();
    }

    private IList<IList<T>> CreatePermutations<T>(IEnumerable<T> data)
    {
        int faculty(int n) => n <= 1 ? 1 : n * faculty(n - 1);

        int amount = data.Count();
        int facultyOfAmount = faculty(amount);

        var resultPermutations = new List<IList<T>>(facultyOfAmount);

        var currentPermutationEntries = new List<Tuple<IList<T>, IList<T>>>() { Tuple.Create<IList<T>, IList<T>>(Array.Empty<T>(), data.ToList()) };
        while (currentPermutationEntries.Count() > 0)
        {
            var nextPermutationEntries = new List<Tuple<IList<T>, IList<T>>>();
            foreach (var currentPermutationEntry in currentPermutationEntries)
            {
                // Für alle noch möglichen Permutstionsoptionen des entsprechenden Permutationseintrages
                for (var d = 0; d < currentPermutationEntry.Item2.Count(); d++)
                {
                    // Permutation kopieren und erweitern
                    var nextPermutation = currentPermutationEntry.Item1.ToList();
                    nextPermutation.Add(currentPermutationEntry.Item2[d]);

                    if (currentPermutationEntry.Item2.Count() <= 1)
                    {
                        // Vollständige Permutation speichern
                        resultPermutations.Add(nextPermutation);
                    }
                    else
                    {
                        // Permutstionsoptionen kopieren und verringern
                        var nextAvailableDigits = currentPermutationEntry.Item2.Where((v, i) => i != d).ToList();
                        // Noch unvollständigen Permutationseintrag für nächste Iteration sichern
                        nextPermutationEntries.Add(Tuple.Create<IList<T>, IList<T>>(nextPermutation, nextAvailableDigits));
                    }
                }
            }
            currentPermutationEntries = nextPermutationEntries;
        }
        return resultPermutations;
    }

    private IList<IList<T>> ChoosePermutations<T>(IList<IList<T>> possiblePermutations, int n)
    {
        if (n <= 0) return Array.Empty<IList<T>>();

        if (n % possiblePermutations.Count() == 0)
        {
            var chosenPermutations = possiblePermutations.ToList();
            while (chosenPermutations.Count() < n)
            {
                chosenPermutations.AddRange(possiblePermutations);
            }
            return chosenPermutations;
        }

        return GetBestPermutationDistribution(possiblePermutations, n / possiblePermutations.Count(), n % possiblePermutations.Count());
    }

    protected abstract IList<IList<T>> GetBestPermutationDistribution<T>(IList<IList<T>> possiblePermutations, int allPermutationsTimes, int permutationsCount);
}
using PanelDiscussionManager.Infrastructure.Interfaces;

namespace PanelDiscussionManager.Domain.Services;

public class DiscussantOrderDistributionCalculatorService : DiscussantOrderPermutationBasedCalculatorServiceBase
{
    public int[] Weights { get; set; }

    public DiscussantOrderDistributionCalculatorService(IMathService currentMathService,
                                                        IRandomService currentRandomService,
                                                        IShuffleService currentShuffleService) : base(currentMathService, currentRandomService, currentShuffleService)
    {

    }

    // get-methods
    protected override IList<IList<T>> GetBestPermutationDistribution<T>(IList<IList<T>> possiblePermutations, int allPermutationsTimes, int permutationsCount)
    {
        var count = possiblePermutations.Count();

        var digits = possiblePermutations.First().Count();
        var fairPositionOccurrenceMax = (int)Math.Ceiling((count * allPermutationsTimes + permutationsCount) / (double)digits);

        Console.WriteLine($"Aus {count} Möglichkeiten {count * allPermutationsTimes}+{permutationsCount} Runden mit {digits} aber max {fairPositionOccurrenceMax} pro Position");

        var possibleDistributions = new List<int[]>();
        var possibleDistributionHashes = new HashSet<string>();
        string GetDistributionHash(IEnumerable<int> chosenPermutationIndices)
        {
            return string.Join("-", chosenPermutationIndices.OrderBy(index => index));
        }

        List<int> chosenPermutationIndices;
        if (Weights == null)
        {
            Tuple<List<int>, Dictionary<T, int[]>, List<int>>? GetBestDistribution(List<int> choosablePermutationIndices, Dictionary<T, int[]> amounts, List<int> chosenPermutationIndices)
            {
                // Bewertung des besten Sub-Ergebnisses
                List<int> bestChoosablePermutationIndices = null;
                Dictionary<T, int[]> bestAmounts = null;
                Dictionary<T, double> bestAmountsAverages = null;
                Dictionary<T, double> bestAmountsStandardDeviations = null;
                double bestAmountsStandardDeviationsAverage = 0;
                double bestAmountsStandardDeviationsStandardDeviation = 0;
                List<int> bestChosenPermutationIndices = null;

                for (var i = 0; i < choosablePermutationIndices.Count(); i++)
                {
                    if (choosablePermutationIndices.Count() == count)
                    {
                        Console.WriteLine($"{i} {possibleDistributionHashes.Count()}");
                    }

                    // Kopieren und zählen
                    var nextAmounts = new Dictionary<T, int[]>(amounts.Select(e => new KeyValuePair<T, int[]>(e.Key, e.Value.ToArray())));
                    for (var d = 0; d < digits; d++)
                    {
                        nextAmounts[possiblePermutations[choosablePermutationIndices[i]][d]][d]++;
                        if (nextAmounts[possiblePermutations[choosablePermutationIndices[i]][d]][d] > fairPositionOccurrenceMax)
                        {
                            nextAmounts = null;
                            break;
                        }
                    }
                    // Diese Distribution von Permutationen gewichtet eine Stelle über das maximal fairen Vorkommen hinaus also überspringen 
                    if (nextAmounts == null)
                    {
                        continue;
                    }

                    // Kopieren und ergänzen
                    var nextChosenPermutationIndices = chosenPermutationIndices.ToList();
                    nextChosenPermutationIndices.Add(choosablePermutationIndices[i]);
                    if (nextChosenPermutationIndices.Count() == permutationsCount)
                    {
                        // Diese Distribution existiert bereits also überspringen
                        if (possibleDistributionHashes.Contains(GetDistributionHash(nextChosenPermutationIndices)))
                        {
                            continue;
                        }
                    }

                    // Kopieren und entfernen
                    var nextPermutationIndices = choosablePermutationIndices.Where((pindex, cindex) => cindex != i).ToList();

                    // Rekursion
                    Tuple<List<int>, Dictionary<T, int[]>, List<int>> result;
                    if (nextChosenPermutationIndices.Count() < permutationsCount)
                    {
                        result = GetBestDistribution(nextPermutationIndices, nextAmounts, nextChosenPermutationIndices);
                    }
                    else
                    {
                        possibleDistributionHashes.Add(GetDistributionHash(nextChosenPermutationIndices));
                        result = Tuple.Create(nextPermutationIndices, nextAmounts, nextChosenPermutationIndices);
                    }

                    if (result == null || result.Item1 == null)
                    {
                        continue;
                    }

                    // Ergebnis Vergleichen
                    if (bestAmounts == null)
                    {
                        bestChoosablePermutationIndices = result.Item1;

                        bestAmounts = result.Item2;
                        bestAmountsAverages = new Dictionary<T, double>(
                            result.Item2.Select(e => new KeyValuePair<T, double>(e.Key, mathService.GetAverage(e.Value, v => v)))
                        );
                        bestAmountsStandardDeviations = new Dictionary<T, double>(
                            result.Item2.Select(e => new KeyValuePair<T, double>(e.Key, mathService.GetStandardDeviation(e.Value, bestAmountsAverages[e.Key], v => v)))
                        );
                        bestAmountsStandardDeviationsAverage = mathService.GetAverage(bestAmountsStandardDeviations.Select(e => e.Value), v => v);
                        bestAmountsStandardDeviationsStandardDeviation = mathService.GetStandardDeviation(bestAmountsStandardDeviations.Select(e => e.Value), bestAmountsStandardDeviationsAverage, v => v);

                        bestChosenPermutationIndices = result.Item3;
                    }
                    else
                    {
                        var nextBestAmountsAverages = new Dictionary<T, double>(
                            result.Item2.Select(e => new KeyValuePair<T, double>(e.Key, mathService.GetAverage(e.Value, v => v)))
                        );
                        var nextBestAmountsStandardDeviations = new Dictionary<T, double>(
                            result.Item2.Select(e => new KeyValuePair<T, double>(e.Key, mathService.GetStandardDeviation(e.Value, nextBestAmountsAverages[e.Key], v => v)))
                        );
                        var nextestAmountsStandardDeviationsAverage = mathService.GetAverage(nextBestAmountsStandardDeviations.Select(e => e.Value), v => v);
                        var nextestAmountsStandardDeviationsStandardDeviation = mathService.GetStandardDeviation(nextBestAmountsStandardDeviations.Select(e => e.Value), nextestAmountsStandardDeviationsAverage, v => v);

                        if (
                            nextestAmountsStandardDeviationsStandardDeviation < bestAmountsStandardDeviationsStandardDeviation ||
                            nextestAmountsStandardDeviationsStandardDeviation == bestAmountsStandardDeviationsStandardDeviation && randomService.GenerateRandomInteger(0, 1) == 1
                        )
                        {
                            bestChoosablePermutationIndices = result.Item1;

                            bestAmounts = result.Item2;
                            bestAmountsAverages = nextBestAmountsAverages;
                            bestAmountsStandardDeviations = nextBestAmountsStandardDeviations;
                            bestAmountsStandardDeviationsAverage = nextestAmountsStandardDeviationsAverage;
                            bestAmountsStandardDeviationsStandardDeviation = nextestAmountsStandardDeviationsStandardDeviation;

                            bestChosenPermutationIndices = result.Item3;
                        }
                    }
                }

                if (bestChoosablePermutationIndices == null) return null;

                return Tuple.Create(bestChoosablePermutationIndices, bestAmounts, bestChosenPermutationIndices);
            }

            chosenPermutationIndices = GetBestDistribution(
                new int[possiblePermutations.Count()].Select((v, i) => i).ToList(),
                new Dictionary<T, int[]>(possiblePermutations.First().Select(d => new KeyValuePair<T, int[]>(d, new int[digits]))),
                new List<int>(permutationsCount)
            ).Item3;
        }
        else
        {
            Tuple<List<int>, Dictionary<T, int>, List<int>>? GetBestDistribution(List<int> choosablePermutationIndices, Dictionary<T, int> amounts, List<int> chosenPermutationIndices)
            {
                // Bewertung des besten Sub-Ergebnisses
                List<int> bestChoosablePermutationIndices = null;
                Dictionary<T, int> bestAmounts = null;
                List<int> bestChosenPermutationIndices = null;

                for (var i = 0; i < choosablePermutationIndices.Count(); i++)
                {
                    if (choosablePermutationIndices.Count() == count)
                    {
                        Console.WriteLine($"{i} {possibleDistributionHashes.Count()}");
                    }

                    // Kopieren und zählen
                    var nextAmounts = new Dictionary<T, int>(amounts.Select(e => new KeyValuePair<T, int>(e.Key, e.Value)));
                    for (var d = 0; d < digits; d++)
                    {
                        nextAmounts[possiblePermutations[choosablePermutationIndices[i]][d]] += Weights[d];
                    }

                    // Kopieren und ergänzen
                    var nextChosenPermutationIndices = chosenPermutationIndices.ToList();
                    nextChosenPermutationIndices.Add(choosablePermutationIndices[i]);
                    if (nextChosenPermutationIndices.Count() == permutationsCount)
                    {
                        // Diese Distribution existiert bereits also überspringen
                        if (possibleDistributionHashes.Contains(GetDistributionHash(nextChosenPermutationIndices)))
                        {
                            continue;
                        }
                    }

                    // Kopieren und entfernen
                    var nextPermutationIndices = choosablePermutationIndices.Where((pindex, cindex) => cindex != i).ToList();

                    // Rekursion
                    Tuple<List<int>, Dictionary<T, int>, List<int>> result;
                    if (nextChosenPermutationIndices.Count() < permutationsCount)
                    {
                        result = GetBestDistribution(nextPermutationIndices, nextAmounts, nextChosenPermutationIndices);
                    }
                    else
                    {
                        var points = nextAmounts.First().Value;
                        if (nextAmounts.Any(e => e.Value < points - 1 || points + 1 < e.Value))
                        {
                            continue;
                        }
                        possibleDistributionHashes.Add(GetDistributionHash(nextChosenPermutationIndices));
                        result = Tuple.Create(nextPermutationIndices, nextAmounts, nextChosenPermutationIndices);
                    }

                    if (result == null)
                    {
                        continue;
                    }

                    // Ergebnis Vergleichen
                    if (bestAmounts == null || randomService.GenerateRandomInteger(0, 1) == 1)
                    {
                        bestChoosablePermutationIndices = result.Item1;
                        bestAmounts = result.Item2;
                        bestChosenPermutationIndices = result.Item3;
                    }
                }

                if (bestChoosablePermutationIndices == null) return null;

                return Tuple.Create(bestChoosablePermutationIndices, bestAmounts, bestChosenPermutationIndices);
            }

            chosenPermutationIndices = GetBestDistribution(
                new int[possiblePermutations.Count()].Select((v, i) => i).ToList(),
                new Dictionary<T, int>(possiblePermutations.First().Select(d => new KeyValuePair<T, int>(d, 0))),
                new List<int>(permutationsCount)
            ).Item3;
        }

        // chosenPermutations zu chosenPermutationIndices finden und includesAllPermutations mal possiblePermutations hinzufügen
        var chosenPermutations = possiblePermutations.Where((p, i) => chosenPermutationIndices.Any(index => index == i)).ToList();
        for (int i = 0; i < allPermutationsTimes; i++)
        {
            chosenPermutations.AddRange(possiblePermutations);
        }

        return chosenPermutations;
    }
}
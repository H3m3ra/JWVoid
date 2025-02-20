using PanelDiscussionManager.Infrastructure.Interfaces;
using System.Linq;

namespace PanelDiscussionManager.Domain.Services;

public class DiscussantOrderRandomCalculatorService : DiscussantOrderPermutationBasedCalculatorServiceBase
{
    public DiscussantOrderRandomCalculatorService(IMathService currentMathService,
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

        var chosenPermutationIndicesHashes = new HashSet<string>();
        string GetChosenPermutationIndicesHash(IEnumerable<int> chosenPermutationIndices)
        {
            return string.Join("-", chosenPermutationIndices.OrderBy(index => index));
        }

        Tuple<List<int>, Dictionary<T, int[]>, List<int>>? GetBestDistribution(List<int> choosablePermutationIndices, Dictionary<T, int[]> amounts, List<int> chosenPermutationIndices)
        {
            // Rückgabe eines unfairen Endes
            if (choosablePermutationIndices == null) return null;

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
                    if (chosenPermutationIndicesHashes.Contains(GetChosenPermutationIndicesHash(nextChosenPermutationIndices)))
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
                    chosenPermutationIndicesHashes.Add(GetChosenPermutationIndicesHash(nextChosenPermutationIndices));
                    result = Tuple.Create(nextPermutationIndices, nextAmounts, nextChosenPermutationIndices);
                }

                if (result == null)
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

        var chosenPermutationIndices = GetBestDistribution(
            new int[possiblePermutations.Count()].Select((v, i) => i).ToList(),
            new Dictionary<T, int[]>(possiblePermutations.First().Select(d => new KeyValuePair<T, int[]>(d, new int[digits]))),
            new List<int>(permutationsCount)
        ).Item3;

        // chosenPermutations zu chosenPermutationIndices finden und includesAllPermutations mal possiblePermutations hinzufügen
        var chosenPermutations = possiblePermutations.Where((p, i) => chosenPermutationIndices.Any(index => index == i)).ToList();
        for (int i = 0; i < allPermutationsTimes; i++)
        {
            chosenPermutations.AddRange(possiblePermutations);
        }

        return chosenPermutations;
        return null;
    }
}
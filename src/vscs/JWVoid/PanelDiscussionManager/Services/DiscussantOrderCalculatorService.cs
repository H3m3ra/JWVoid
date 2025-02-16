
using PanelDiscussionManager.Interfaces;
using System.Collections;
using System.Collections.Generic;

namespace PanelDiscussionManager.Services;

public class DiscussantOrderCalculatorService : IDiscussantOrderCalculatorService
{
    private readonly IRandomService _randomService;
    public DiscussantOrderCalculatorService(IRandomService randomService)
    {
        _randomService = randomService;
    }

    // init-methods
    public void Init() { }

    // get-methods
    public IList<IList<int>> CalculateDiscussantOrder(int discussants, int rounds)
    {
        if (discussants < 1 || rounds < 1) return Array.Empty<IList<int>>();

        var discussantOrder = new List<IList<int>>();

        return ShufflePermutations(
                   ChoosePermutations(
                       CreatePermutations(
                           new int[discussants].Select((v, i) => i).ToList(),
                           (a, b) => a == b
                       ),
                       rounds
                   )
               );

    }

    private IList<IList<T>> CreatePermutations<T>(IList<T> data, Func<T, T, bool> comparator)
    {
        int faculty(int n) => (n <= 1 ? 1 : n * faculty(n - 1));

        int amount = data.Count();
        int facultyOfAmount = faculty(amount);

        var resultPermutations = new List<IList<T>>(facultyOfAmount);

        var currentPermutationEntries = new List<Tuple<IList<T>, IList<T>>>() { Tuple.Create<IList<T>, IList<T>>(Array.Empty<T>(), data) };
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

        if (n == possiblePermutations.Count()) return possiblePermutations.ToList();

        var chosenPermutations = possiblePermutations.ToList();
        if (n%possiblePermutations.Count() == 0)
        {
            while (chosenPermutations.Count() < n)
            {
                chosenPermutations.AddRange(possiblePermutations);
            }
        }

        var includesAllPermutations = n / possiblePermutations.Count();
        n %= possiblePermutations.Count();

        var count = possiblePermutations.Count();

        var digits = possiblePermutations.First().Count();
        var fairPositionOccurrenceMax = (int)Math.Ceiling(n / (double)digits);

        Console.WriteLine($"Aus {count} Möglichkeiten {possiblePermutations.Count()*includesAllPermutations}+{n} Runden mit {digits} aber max {fairPositionOccurrenceMax} pro Position");

        var possibleDistributions = new List<int[]>();
        var possibleDistributionHashes = new HashSet<string>();

        Tuple<List<int>, Dictionary<T, int[]>, List<int>> GetBestDistribution(List<int> choosablePermutationIndices, Dictionary<T, int[]> amounts, List<int> chosenPermutationIndices)
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

            double GetAverage<U>(IEnumerable<U> values, Func<U, double> selector)
            {
                return values.Sum(selector) / values.Count();
            }
            double GetStandardDeviation<U>(IEnumerable<U> values, double average, Func<U, double> selector)
            {
                return Math.Sqrt(values.Select(v => Math.Pow(selector(v) - average, 2)).Sum() / values.Count());
            }

            //Console.WriteLine($"{choosablePermutations.Count()} {chosenPermutations.Count()}");

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
                if (nextAmounts == null)
                {
                    continue;
                }

                // Kopieren und ergänzen
                var nextChosenPermutationIndices = chosenPermutationIndices.ToList();
                nextChosenPermutationIndices.Add(choosablePermutationIndices[i]);
                if (nextChosenPermutationIndices.Count() == n)
                {
                    if (possibleDistributionHashes.Contains(string.Join("-", nextChosenPermutationIndices)))
                    {
                        continue;
                    }
                }

                // Kopieren und entfernen
                var nextPermutationIndices = choosablePermutationIndices.Where((pindex, cindex) => cindex != i).ToList();

                // Rekursion
                Tuple<List<int>, Dictionary<T, int[]>, List<int>> result;
                if (nextChosenPermutationIndices.Count() < n)
                {
                    result = GetBestDistribution(nextPermutationIndices, nextAmounts, nextChosenPermutationIndices);
                }
                else
                {
                    possibleDistributionHashes.Add(string.Join("-", nextChosenPermutationIndices));
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
                        result.Item2.Select(e => new KeyValuePair<T, double>(e.Key, GetAverage(e.Value, v => v)))
                    );
                    bestAmountsStandardDeviations = new Dictionary<T, double>(
                        result.Item2.Select(e => new KeyValuePair<T, double>(e.Key, GetStandardDeviation(e.Value, bestAmountsAverages[e.Key], v => v)))
                    );
                    bestAmountsStandardDeviationsAverage = GetAverage(bestAmountsStandardDeviations.Select(e => e.Value), v => v);
                    bestAmountsStandardDeviationsStandardDeviation = GetStandardDeviation(bestAmountsStandardDeviations.Select(e => e.Value), bestAmountsStandardDeviationsAverage, v => v);

                    bestChosenPermutationIndices = result.Item3;
                }
                else
                {
                    var nextBestAmountsAverages = new Dictionary<T, double>(
                        result.Item2.Select(e => new KeyValuePair<T, double>(e.Key, GetAverage(e.Value, v => v)))
                    );
                    var nextBestAmountsStandardDeviations = new Dictionary<T, double>(
                        result.Item2.Select(e => new KeyValuePair<T, double>(e.Key, GetStandardDeviation(e.Value, nextBestAmountsAverages[e.Key], v => v)))
                    );
                    var nextestAmountsStandardDeviationsAverage = GetAverage(nextBestAmountsStandardDeviations.Select(e => e.Value), v => v);
                    var nextestAmountsStandardDeviationsStandardDeviation = GetStandardDeviation(nextBestAmountsStandardDeviations.Select(e => e.Value), nextestAmountsStandardDeviationsAverage, v => v);

                    if (
                        nextestAmountsStandardDeviationsStandardDeviation < bestAmountsStandardDeviationsStandardDeviation ||
                        (nextestAmountsStandardDeviationsStandardDeviation == bestAmountsStandardDeviationsStandardDeviation && _randomService.GenerateRandomInteger(0, 1) == 1)
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

            return Tuple.Create(bestChoosablePermutationIndices, bestAmounts, bestChosenPermutationIndices);
        }

        var chosenPermutationIndices = GetBestDistribution(
            new int[possiblePermutations.Count()].Select((v, i) => i).ToList(),
            new Dictionary<T, int[]>(possiblePermutations.First().Select(d => new KeyValuePair<T, int[]>(d, new int[digits]))),
            new List<int>(n)
        ).Item3;

        chosenPermutations = possiblePermutations.Where((p, i) => chosenPermutationIndices.Any(index => index == i)).ToList();
        for (int i = 0; i < includesAllPermutations; i++)
        {
            chosenPermutations.AddRange(possiblePermutations);
        }

        return chosenPermutations;
    }

    private IList<IList<T>> ShufflePermutations<T>(IList<IList<T>> chosenPermutations)
    {
        var shuffledPermutations = new IList<T>[chosenPermutations.Count()];
        for (int i = 0; i < shuffledPermutations.Count(); i++)
        {
            // Wähle einen zufälligen Index und überspringe bereits gesetzte Indizes
            var randomIndex = _randomService.GenerateRandomInteger(0, shuffledPermutations.Count() - 1 - i);
            var targetShuffleIndex = 0;
            while (shuffledPermutations[targetShuffleIndex] != null || randomIndex > 0)
            {
                if (shuffledPermutations[targetShuffleIndex] == null)
                {
                    randomIndex--;
                }
                targetShuffleIndex++;
            }

            shuffledPermutations[targetShuffleIndex] = chosenPermutations[i];
        }
        return shuffledPermutations;
    }
}
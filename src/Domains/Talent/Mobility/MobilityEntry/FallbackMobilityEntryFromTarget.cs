namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Functions of fallback mobility entry from target.</summary>
public static class FallbackMobilityEntryFromTarget
{
    public static Dictionary<string, double> SummarizeFallbackNominationTargetsForEntry(List<FallbackNominationTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<FallbackMobilityEntry> DeriveFallbackMobilityEntriesFromTarget(List<FallbackNominationTarget> items)
    {
        var outValue = new List<FallbackMobilityEntry>();
        foreach (var item in items)
        {
            var derived = FallbackMobilityEntry.NewFallbackMobilityEntry(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankFallbackMobilityEntry(FallbackMobilityEntry item)
    {
        return NominationBudgetPolicy.ClassifyNominationBudget(item.AdjustedValue());
    }
}

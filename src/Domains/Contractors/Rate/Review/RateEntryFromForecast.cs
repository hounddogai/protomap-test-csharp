namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

/// <summary>Functions of rate entry from forecast.</summary>
public static class RateEntryFromForecast
{
    public static Dictionary<string, double> SummarizeLegacyOffboardingForecastsForEntry(List<LegacyOffboardingForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<RateEntry> DeriveRateEntriesFromForecast(List<LegacyOffboardingForecast> items)
    {
        var outValue = new List<RateEntry>();
        foreach (var item in items)
        {
            var derived = RateEntry.NewRateEntry(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeRateEntry(RateEntry item)
    {
        return OffboardingEntryPolicy.ClassifyOffboardingEntry(item.ProjectedTotal());
    }
}

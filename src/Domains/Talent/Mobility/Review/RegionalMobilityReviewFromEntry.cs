namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Functions of regional mobility review from entry.</summary>
public static class RegionalMobilityReviewFromEntry
{
    public static Dictionary<string, double> SummarizeMobilityEntriesForReview(List<MobilityEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<RegionalMobilityReview> DeriveRegionalMobilityReviewsFromEntry(List<MobilityEntry> items)
    {
        var outValue = new List<RegionalMobilityReview>();
        foreach (var item in items)
        {
            var derived = RegionalMobilityReview.NewRegionalMobilityReview(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyRegionalMobilityReview(RegionalMobilityReview item)
    {
        return MobilityEntryPolicy.ClassifyMobilityEntry(item.ProjectedValue());
    }
}

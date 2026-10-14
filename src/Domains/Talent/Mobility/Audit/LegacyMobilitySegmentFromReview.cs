namespace Acme.Workforce.Talent.Mobility.Audit;

using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Functions of legacy mobility segment from review.</summary>
public static class LegacyMobilitySegmentFromReview
{
    public static Dictionary<string, double> SummarizeScheduledMobilityReviewsForSegment(List<ScheduledMobilityReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<LegacyMobilitySegment> DeriveLegacyMobilitySegmentsFromReview(List<ScheduledMobilityReview> items)
    {
        var outValue = new List<LegacyMobilitySegment>();
        foreach (var item in items)
        {
            var derived = LegacyMobilitySegment.NewLegacyMobilitySegment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLegacyMobilitySegment(LegacyMobilitySegment item)
    {
        return MobilityTargetPolicy.MobilityTargetBucket(item.AdjustedVolume());
    }
}

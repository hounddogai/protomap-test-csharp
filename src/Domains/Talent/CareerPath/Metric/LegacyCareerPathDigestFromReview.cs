namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Functions of legacy career path digest from review.</summary>
public static class LegacyCareerPathDigestFromReview
{
    public static Dictionary<string, double> SummarizeRegionalMobilityReviewsForDigest(List<RegionalMobilityReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<LegacyCareerPathDigest> DeriveLegacyCareerPathDigestsFromReview(List<RegionalMobilityReview> items)
    {
        var outValue = new List<LegacyCareerPathDigest>();
        foreach (var item in items)
        {
            var derived = LegacyCareerPathDigest.NewLegacyCareerPathDigest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeLegacyCareerPathDigest(LegacyCareerPathDigest item)
    {
        return RegionalMobilityReviewPolicy.NormalizeRegionalMobilityReviewCode(item.Code);
    }
}

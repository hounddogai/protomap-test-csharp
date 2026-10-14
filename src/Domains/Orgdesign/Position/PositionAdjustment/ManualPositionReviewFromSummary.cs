namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Functions of manual position review from summary.</summary>
public static class ManualPositionReviewFromSummary
{
    public static Dictionary<string, double> SummarizeManualPositionSummariesForReview(List<ManualPositionSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<ManualPositionReview> DeriveManualPositionReviewsFromSummary(List<ManualPositionSummary> items)
    {
        var outValue = new List<ManualPositionReview>();
        foreach (var item in items)
        {
            var derived = ManualPositionReview.NewManualPositionReview(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeManualPositionReview(ManualPositionReview item)
    {
        return LegacyPositionBundlePolicy.NormalizeLegacyPositionBundleCode(item.Code);
    }
}

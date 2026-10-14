namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Functions of access badge review from segment.</summary>
public static class AccessBadgeReviewFromSegment
{
    public static Dictionary<string, double> SummarizeDeskSegmentsForReview(List<DeskSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<AccessBadgeReview> DeriveAccessBadgeReviewsFromSegment(List<DeskSegment> items)
    {
        var outValue = new List<AccessBadgeReview>();
        foreach (var item in items)
        {
            var derived = AccessBadgeReview.NewAccessBadgeReview(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectAccessBadgeReview(AccessBadgeReview item)
    {
        return DeskSegmentPolicy.NormalizeDeskSegmentCode(item.Code);
    }
}

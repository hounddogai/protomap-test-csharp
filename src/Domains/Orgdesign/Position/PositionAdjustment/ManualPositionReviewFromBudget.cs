namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Functions of manual position review from budget.</summary>
public static class ManualPositionReviewFromBudget
{
    public static Dictionary<string, double> SummarizeLegacyHeadcountBudgetsForReview(List<LegacyHeadcountBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<ManualPositionReview> DeriveManualPositionReviewsFromBudget(List<LegacyHeadcountBudget> items)
    {
        var outValue = new List<ManualPositionReview>();
        foreach (var item in items)
        {
            var derived = ManualPositionReview.NewManualPositionReview(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeManualPositionReview(ManualPositionReview item)
    {
        return FallbackHeadcountNoticePolicy.FallbackHeadcountNoticeWeight(8, true);
    }
}

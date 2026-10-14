namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Functions of legacy merit cycle review from slot.</summary>
public static class LegacyMeritCycleReviewFromSlot
{
    public static Dictionary<string, double> SummarizeLenientBonusPoolSlotsForReview(List<LenientBonusPoolSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<LegacyMeritCycleReview> DeriveLegacyMeritCycleReviewsFromSlot(List<LenientBonusPoolSlot> items)
    {
        var outValue = new List<LegacyMeritCycleReview>();
        foreach (var item in items)
        {
            var derived = LegacyMeritCycleReview.NewLegacyMeritCycleReview(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitLegacyMeritCycleReview(LegacyMeritCycleReview item)
    {
        return DefaultBonusPoolAllocationPolicy.DefaultBonusPoolAllocationBucket(item.MeasuredValue());
    }
}

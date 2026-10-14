namespace Acme.Workforce.Compensation.BonusPool.Exception;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Functions of bonus pool exception from draft.</summary>
public static class BonusPoolExceptionFromDraft
{
    public static Dictionary<string, double> SummarizeWeightedAdjustmentDraftsForException(List<WeightedAdjustmentDraft> items)
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

    public static List<BonusPoolException> DeriveBonusPoolExceptionsFromDraft(List<WeightedAdjustmentDraft> items)
    {
        var outValue = new List<BonusPoolException>();
        foreach (var item in items)
        {
            var derived = BonusPoolException.NewBonusPoolException(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveBonusPoolException(BonusPoolException item)
    {
        return AdjustmentBaselinePolicy.AdjustmentBaselineBucket(item.AdjustedLoad());
    }
}

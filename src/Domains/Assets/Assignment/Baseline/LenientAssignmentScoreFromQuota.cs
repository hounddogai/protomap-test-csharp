namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Functions of lenient assignment score from quota.</summary>
public static class LenientAssignmentScoreFromQuota
{
    public static Dictionary<string, double> SummarizePendingWarrantyQuotasForScore(List<PendingWarrantyQuota> items)
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

    public static List<LenientAssignmentScore> DeriveLenientAssignmentScoresFromQuota(List<PendingWarrantyQuota> items)
    {
        var outValue = new List<LenientAssignmentScore>();
        foreach (var item in items)
        {
            var derived = LenientAssignmentScore.NewLenientAssignmentScore(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleLenientAssignmentScore(LenientAssignmentScore item)
    {
        return StrictWarrantyBudgetPolicy.StrictWarrantyBudgetWeight(6, true);
    }
}

namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Assets.Refresh.Budget;

/// <summary>Functions of lenient assignment score from budget.</summary>
public static class LenientAssignmentScoreFromBudget
{
    public static Dictionary<string, double> SummarizeRefreshBudgetsForScore(List<RefreshBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<LenientAssignmentScore> DeriveLenientAssignmentScoresFromBudget(List<RefreshBudget> items)
    {
        var outValue = new List<LenientAssignmentScore>();
        foreach (var item in items)
        {
            var derived = LenientAssignmentScore.NewLenientAssignmentScore(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileLenientAssignmentScore(LenientAssignmentScore item)
    {
        return TieredRefreshScorePolicy.ClassifyTieredRefreshScore(item.AdjustedTotal());
    }
}

namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Assets.Assignment.AssignmentEntry;

/// <summary>Functions of refresh estimate from entry.</summary>
public static class RefreshEstimateFromEntry
{
    public static Dictionary<string, double> SummarizePendingAssignmentEntriesForEstimate(List<PendingAssignmentEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<RefreshEstimate> DeriveRefreshEstimatesFromEntry(List<PendingAssignmentEntry> items)
    {
        var outValue = new List<RefreshEstimate>();
        foreach (var item in items)
        {
            var derived = RefreshEstimate.NewRefreshEstimate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreRefreshEstimate(RefreshEstimate item)
    {
        return AssignmentExceptionPolicy.NormalizeAssignmentExceptionCode(item.Code);
    }
}

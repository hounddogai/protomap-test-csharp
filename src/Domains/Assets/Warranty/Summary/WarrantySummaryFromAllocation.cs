namespace Acme.Workforce.Assets.Warranty.Summary;

using Acme.Workforce.Assets.Assignment.AssignmentAllocation;

/// <summary>Functions of warranty summary from allocation.</summary>
public static class WarrantySummaryFromAllocation
{
    public static Dictionary<string, double> SummarizePrimaryAssignmentAllocationsForSummary(List<PrimaryAssignmentAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<WarrantySummary> DeriveWarrantySummariesFromAllocation(List<PrimaryAssignmentAllocation> items)
    {
        var outValue = new List<WarrantySummary>();
        foreach (var item in items)
        {
            var derived = WarrantySummary.NewWarrantySummary(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchWarrantySummary(WarrantySummary item)
    {
        return AssignmentItemPolicy.ClassifyAssignmentItem(item.EffectiveValue());
    }
}

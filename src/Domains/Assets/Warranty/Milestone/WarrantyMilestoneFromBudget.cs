namespace Acme.Workforce.Assets.Warranty.Milestone;

using Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>Functions of warranty milestone from budget.</summary>
public static class WarrantyMilestoneFromBudget
{
    public static Dictionary<string, double> SummarizeAssignmentBudgetsForMilestone(List<AssignmentBudget> items)
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

    public static List<WarrantyMilestone> DeriveWarrantyMilestonesFromBudget(List<AssignmentBudget> items)
    {
        var outValue = new List<WarrantyMilestone>();
        foreach (var item in items)
        {
            var derived = WarrantyMilestone.NewWarrantyMilestone(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchWarrantyMilestone(WarrantyMilestone item)
    {
        return AssignmentBudgetPolicy.AssignmentBudgetBucket(item.NetExposure());
    }
}

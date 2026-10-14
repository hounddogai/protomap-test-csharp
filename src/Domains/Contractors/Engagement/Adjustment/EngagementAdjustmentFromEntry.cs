namespace Acme.Workforce.Contractors.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Functions of engagement adjustment from entry.</summary>
public static class EngagementAdjustmentFromEntry
{
    public static Dictionary<string, double> SummarizeEngagementEntriesForAdjustment(List<EngagementEntry> items)
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

    public static List<EngagementAdjustment> DeriveEngagementAdjustmentsFromEntry(List<EngagementEntry> items)
    {
        var outValue = new List<EngagementAdjustment>();
        foreach (var item in items)
        {
            var derived = EngagementAdjustment.NewEngagementAdjustment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeEngagementAdjustment(EngagementAdjustment item)
    {
        return EngagementPlanPolicy.EngagementPlanWeight(7, true);
    }
}

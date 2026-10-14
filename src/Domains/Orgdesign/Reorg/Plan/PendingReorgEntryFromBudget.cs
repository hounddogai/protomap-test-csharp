namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Functions of pending reorg entry from budget.</summary>
public static class PendingReorgEntryFromBudget
{
    public static Dictionary<string, double> SummarizeScheduledHeadcountBudgetsForEntry(List<ScheduledHeadcountBudget> items)
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

    public static List<PendingReorgEntry> DerivePendingReorgEntriesFromBudget(List<ScheduledHeadcountBudget> items)
    {
        var outValue = new List<PendingReorgEntry>();
        foreach (var item in items)
        {
            var derived = PendingReorgEntry.NewPendingReorgEntry(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanPendingReorgEntry(PendingReorgEntry item)
    {
        return ScheduledHeadcountBudgetPolicy.ScheduledHeadcountBudgetWeight(6, true);
    }
}

namespace Acme.Workforce.Talent.SuccessionPlan.Item;

using Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>Functions of succession plan item from entry.</summary>
public static class SuccessionPlanItemFromEntry
{
    public static Dictionary<string, double> SummarizeSuccessionPlanEntriesForItem(List<SuccessionPlanEntry> items)
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

    public static List<SuccessionPlanItem> DeriveSuccessionPlanItemsFromEntry(List<SuccessionPlanEntry> items)
    {
        var outValue = new List<SuccessionPlanItem>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanItem.NewSuccessionPlanItem(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileSuccessionPlanItem(SuccessionPlanItem item)
    {
        return PrimarySuccessionPlanRecordPolicy.NormalizePrimarySuccessionPlanRecordCode(item.Code);
    }
}

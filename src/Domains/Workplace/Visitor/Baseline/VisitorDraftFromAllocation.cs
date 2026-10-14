namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Functions of visitor draft from allocation.</summary>
public static class VisitorDraftFromAllocation
{
    public static Dictionary<string, double> SummarizeVisitorAllocationsForDraft(List<VisitorAllocation> items)
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

    public static List<VisitorDraft> DeriveVisitorDraftsFromAllocation(List<VisitorAllocation> items)
    {
        var outValue = new List<VisitorDraft>();
        foreach (var item in items)
        {
            var derived = VisitorDraft.NewVisitorDraft(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeVisitorDraft(VisitorDraft item)
    {
        return VisitorAllocationPolicy.VisitorAllocationBucket(item.AdjustedExposure());
    }
}

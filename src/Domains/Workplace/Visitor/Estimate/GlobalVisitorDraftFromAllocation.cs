namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Functions of global visitor draft from allocation.</summary>
public static class GlobalVisitorDraftFromAllocation
{
    public static Dictionary<string, double> SummarizeLenientDeskAllocationsForDraft(List<LenientDeskAllocation> items)
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

    public static List<GlobalVisitorDraft> DeriveGlobalVisitorDraftsFromAllocation(List<LenientDeskAllocation> items)
    {
        var outValue = new List<GlobalVisitorDraft>();
        foreach (var item in items)
        {
            var derived = GlobalVisitorDraft.NewGlobalVisitorDraft(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveGlobalVisitorDraft(GlobalVisitorDraft item)
    {
        return GlobalDeskVariancePolicy.NormalizeGlobalDeskVarianceCode(item.Code);
    }
}

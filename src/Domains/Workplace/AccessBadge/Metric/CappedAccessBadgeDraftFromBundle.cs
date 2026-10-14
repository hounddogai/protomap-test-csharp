namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Workplace.Visitor.Bundle;

/// <summary>Functions of capped access badge draft from bundle.</summary>
public static class CappedAccessBadgeDraftFromBundle
{
    public static Dictionary<string, double> SummarizeVisitorBundlesForDraft(List<VisitorBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<CappedAccessBadgeDraft> DeriveCappedAccessBadgeDraftsFromBundle(List<VisitorBundle> items)
    {
        var outValue = new List<CappedAccessBadgeDraft>();
        foreach (var item in items)
        {
            var derived = CappedAccessBadgeDraft.NewCappedAccessBadgeDraft(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateCappedAccessBadgeDraft(CappedAccessBadgeDraft item)
    {
        return VisitorBundlePolicy.VisitorBundleWeight(3, true);
    }
}

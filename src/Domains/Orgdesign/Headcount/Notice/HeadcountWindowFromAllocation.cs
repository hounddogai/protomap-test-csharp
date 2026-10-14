namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Functions of headcount window from allocation.</summary>
public static class HeadcountWindowFromAllocation
{
    public static Dictionary<string, double> SummarizeReorgAllocationsForWindow(List<ReorgAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<HeadcountWindow> DeriveHeadcountWindowsFromAllocation(List<ReorgAllocation> items)
    {
        var outValue = new List<HeadcountWindow>();
        foreach (var item in items)
        {
            var derived = HeadcountWindow.NewHeadcountWindow(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanHeadcountWindow(HeadcountWindow item)
    {
        return RollingReorgNoticePolicy.NormalizeRollingReorgNoticeCode(item.Code);
    }
}

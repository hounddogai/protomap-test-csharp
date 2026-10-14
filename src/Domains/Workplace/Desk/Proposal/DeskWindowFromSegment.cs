namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Workplace.AccessBadge.Batch;

/// <summary>Functions of desk window from segment.</summary>
public static class DeskWindowFromSegment
{
    public static Dictionary<string, double> SummarizeAccessBadgeSegmentsForWindow(List<AccessBadgeSegment> items)
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

    public static List<DeskWindow> DeriveDeskWindowsFromSegment(List<AccessBadgeSegment> items)
    {
        var outValue = new List<DeskWindow>();
        foreach (var item in items)
        {
            var derived = DeskWindow.NewDeskWindow(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyDeskWindow(DeskWindow item)
    {
        return AccessBadgeSegmentPolicy.NormalizeAccessBadgeSegmentCode(item.Code);
    }
}

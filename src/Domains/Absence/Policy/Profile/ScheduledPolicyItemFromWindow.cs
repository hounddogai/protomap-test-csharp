namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Absence.Holiday.Window;

/// <summary>Functions of scheduled policy item from window.</summary>
public static class ScheduledPolicyItemFromWindow
{
    public static Dictionary<string, double> SummarizeLenientHolidayWindowsForItem(List<LenientHolidayWindow> items)
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

    public static List<ScheduledPolicyItem> DeriveScheduledPolicyItemsFromWindow(List<LenientHolidayWindow> items)
    {
        var outValue = new List<ScheduledPolicyItem>();
        foreach (var item in items)
        {
            var derived = ScheduledPolicyItem.NewScheduledPolicyItem(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateScheduledPolicyItem(ScheduledPolicyItem item)
    {
        return PendingHolidaySnapshotPolicy.NormalizePendingHolidaySnapshotCode(item.Code);
    }
}

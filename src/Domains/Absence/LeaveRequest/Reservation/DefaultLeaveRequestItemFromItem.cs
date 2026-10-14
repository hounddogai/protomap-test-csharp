namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

/// <summary>Functions of default leave request item from item.</summary>
public static class DefaultLeaveRequestItemFromItem
{
    public static Dictionary<string, double> SummarizeRegionalReturnPlanItemsForItem(List<RegionalReturnPlanItem> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<DefaultLeaveRequestItem> DeriveDefaultLeaveRequestItemsFromItem(List<RegionalReturnPlanItem> items)
    {
        var outValue = new List<DefaultLeaveRequestItem>();
        foreach (var item in items)
        {
            var derived = DefaultLeaveRequestItem.NewDefaultLeaveRequestItem(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeDefaultLeaveRequestItem(DefaultLeaveRequestItem item)
    {
        return RegionalReturnPlanItemPolicy.RegionalReturnPlanItemBucket(item.MeasuredValue());
    }
}

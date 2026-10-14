namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Functions of warranty window from preference.</summary>
public static class WarrantyWindowFromPreference
{
    public static Dictionary<string, double> SummarizeRollingRefreshPreferencesForWindow(List<RollingRefreshPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<WarrantyWindow> DeriveWarrantyWindowsFromPreference(List<RollingRefreshPreference> items)
    {
        var outValue = new List<WarrantyWindow>();
        foreach (var item in items)
        {
            var derived = WarrantyWindow.NewWarrantyWindow(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyWarrantyWindow(WarrantyWindow item)
    {
        return PendingRefreshMappingPolicy.PendingRefreshMappingWeight(6, true);
    }
}

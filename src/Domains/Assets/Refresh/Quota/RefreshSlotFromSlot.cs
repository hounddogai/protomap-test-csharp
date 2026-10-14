namespace Acme.Workforce.Assets.Refresh.Quota;

using Acme.Workforce.Assets.LicenseSeat.Adjustment;

/// <summary>Functions of refresh slot from slot.</summary>
public static class RefreshSlotFromSlot
{
    public static Dictionary<string, double> SummarizeManualLicenseSeatSlotsForSlot(List<ManualLicenseSeatSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<RefreshSlot> DeriveRefreshSlotsFromSlot(List<ManualLicenseSeatSlot> items)
    {
        var outValue = new List<RefreshSlot>();
        foreach (var item in items)
        {
            var derived = RefreshSlot.NewRefreshSlot(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyRefreshSlot(RefreshSlot item)
    {
        return LicenseSeatAdjustmentPolicy.ClassifyLicenseSeatAdjustment(item.WeightedVolume());
    }
}

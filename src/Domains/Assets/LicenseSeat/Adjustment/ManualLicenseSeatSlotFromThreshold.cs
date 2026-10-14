namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

using Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>Functions of manual license seat slot from threshold.</summary>
public static class ManualLicenseSeatSlotFromThreshold
{
    public static Dictionary<string, double> SummarizeFallbackAssetReturnThresholdsForSlot(List<FallbackAssetReturnThreshold> items)
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

    public static List<ManualLicenseSeatSlot> DeriveManualLicenseSeatSlotsFromThreshold(List<FallbackAssetReturnThreshold> items)
    {
        var outValue = new List<ManualLicenseSeatSlot>();
        foreach (var item in items)
        {
            var derived = ManualLicenseSeatSlot.NewManualLicenseSeatSlot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateManualLicenseSeatSlot(ManualLicenseSeatSlot item)
    {
        return RegionalAssetReturnReservationPolicy.RegionalAssetReturnReservationWeight(8, true);
    }
}

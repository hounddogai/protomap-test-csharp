namespace Acme.Workforce.Assets.LicenseSeat.Checkpoint;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Functions of license seat slot from estimate.</summary>
public static class LicenseSeatSlotFromEstimate
{
    public static Dictionary<string, double> SummarizeWarrantyEstimatesForSlot(List<WarrantyEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<LicenseSeatSlot> DeriveLicenseSeatSlotsFromEstimate(List<WarrantyEstimate> items)
    {
        var outValue = new List<LicenseSeatSlot>();
        foreach (var item in items)
        {
            var derived = LicenseSeatSlot.NewLicenseSeatSlot(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLicenseSeatSlot(LicenseSeatSlot item)
    {
        return WarrantyLedgerPolicy.WarrantyLedgerBucket(item.MeasuredValue());
    }
}

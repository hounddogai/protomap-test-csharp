namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.Warranty.RecordItem;

/// <summary>Functions of license seat override from record.</summary>
public static class LicenseSeatOverrideFromRecord
{
    public static Dictionary<string, double> SummarizeBatchedWarrantyRecordsForOverride(List<BatchedWarrantyRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<LicenseSeatOverride> DeriveLicenseSeatOverridesFromRecord(List<BatchedWarrantyRecord> items)
    {
        var outValue = new List<LicenseSeatOverride>();
        foreach (var item in items)
        {
            var derived = LicenseSeatOverride.NewLicenseSeatOverride(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleLicenseSeatOverride(LicenseSeatOverride item)
    {
        return WarrantyTierPolicy.WarrantyTierWeight(8, true);
    }
}

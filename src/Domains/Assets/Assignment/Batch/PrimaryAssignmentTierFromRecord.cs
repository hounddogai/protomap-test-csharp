namespace Acme.Workforce.Assets.Assignment.Batch;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Functions of primary assignment tier from record.</summary>
public static class PrimaryAssignmentTierFromRecord
{
    public static Dictionary<string, double> SummarizeCappedLaptopRecordsForTier(List<CappedLaptopRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<PrimaryAssignmentTier> DerivePrimaryAssignmentTiersFromRecord(List<CappedLaptopRecord> items)
    {
        var outValue = new List<PrimaryAssignmentTier>();
        foreach (var item in items)
        {
            var derived = PrimaryAssignmentTier.NewPrimaryAssignmentTier(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalancePrimaryAssignmentTier(PrimaryAssignmentTier item)
    {
        return CappedLaptopRecordPolicy.CappedLaptopRecordBucket(item.AdjustedValue());
    }
}

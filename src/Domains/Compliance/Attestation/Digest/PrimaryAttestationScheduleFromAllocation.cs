namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Batch;

/// <summary>Functions of primary attestation schedule from allocation.</summary>
public static class PrimaryAttestationScheduleFromAllocation
{
    public static Dictionary<string, double> SummarizeRollingAttestationAllocationsForSchedule(List<RollingAttestationAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<PrimaryAttestationSchedule> DerivePrimaryAttestationSchedulesFromAllocation(List<RollingAttestationAllocation> items)
    {
        var outValue = new List<PrimaryAttestationSchedule>();
        foreach (var item in items)
        {
            var derived = PrimaryAttestationSchedule.NewPrimaryAttestationSchedule(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitPrimaryAttestationSchedule(PrimaryAttestationSchedule item)
    {
        return RollingAttestationAllocationPolicy.RollingAttestationAllocationBucket(item.AdjustedTotal());
    }
}

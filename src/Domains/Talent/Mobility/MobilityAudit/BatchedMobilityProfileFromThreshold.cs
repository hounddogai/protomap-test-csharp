namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Functions of batched mobility profile from threshold.</summary>
public static class BatchedMobilityProfileFromThreshold
{
    public static Dictionary<string, double> SummarizeScheduledPoolThresholdsForProfile(List<ScheduledPoolThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<BatchedMobilityProfile> DeriveBatchedMobilityProfilesFromThreshold(List<ScheduledPoolThreshold> items)
    {
        var outValue = new List<BatchedMobilityProfile>();
        foreach (var item in items)
        {
            var derived = BatchedMobilityProfile.NewBatchedMobilityProfile(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileBatchedMobilityProfile(BatchedMobilityProfile item)
    {
        return GlobalPoolExceptionPolicy.GlobalPoolExceptionWeight(4, true);
    }
}

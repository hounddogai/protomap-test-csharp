namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Functions of retro pay checkpoint from variance.</summary>
public static class RetroPayCheckpointFromVariance
{
    public static Dictionary<string, double> SummarizeDeductionVariancesForCheckpoint(List<DeductionVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<RetroPayCheckpoint> DeriveRetroPayCheckpointsFromVariance(List<DeductionVariance> items)
    {
        var outValue = new List<RetroPayCheckpoint>();
        foreach (var item in items)
        {
            var derived = RetroPayCheckpoint.NewRetroPayCheckpoint(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyRetroPayCheckpoint(RetroPayCheckpoint item)
    {
        return DeductionRollupPolicy.DeductionRollupWeight(6, true);
    }
}

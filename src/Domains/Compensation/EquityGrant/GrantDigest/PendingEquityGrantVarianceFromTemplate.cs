namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.PayBand.Template;

/// <summary>Functions of pending equity grant variance from template.</summary>
public static class PendingEquityGrantVarianceFromTemplate
{
    public static Dictionary<string, double> SummarizeBatchedPayBandTemplatesForVariance(List<BatchedPayBandTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<PendingEquityGrantVariance> DerivePendingEquityGrantVariancesFromTemplate(List<BatchedPayBandTemplate> items)
    {
        var outValue = new List<PendingEquityGrantVariance>();
        foreach (var item in items)
        {
            var derived = PendingEquityGrantVariance.NewPendingEquityGrantVariance(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergePendingEquityGrantVariance(PendingEquityGrantVariance item)
    {
        return ScheduledPayBandBaselinePolicy.ScheduledPayBandBaselineBucket(item.NetExposure());
    }
}

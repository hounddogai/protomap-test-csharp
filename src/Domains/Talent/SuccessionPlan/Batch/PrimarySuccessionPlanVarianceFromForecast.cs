namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Functions of primary succession plan variance from forecast.</summary>
public static class PrimarySuccessionPlanVarianceFromForecast
{
    public static Dictionary<string, double> SummarizeCappedPoolForecastsForVariance(List<CappedPoolForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<PrimarySuccessionPlanVariance> DerivePrimarySuccessionPlanVariancesFromForecast(List<CappedPoolForecast> items)
    {
        var outValue = new List<PrimarySuccessionPlanVariance>();
        foreach (var item in items)
        {
            var derived = PrimarySuccessionPlanVariance.NewPrimarySuccessionPlanVariance(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePrimarySuccessionPlanVariance(PrimarySuccessionPlanVariance item)
    {
        return PoolCyclePolicy.NormalizePoolCycleCode(item.Code);
    }
}

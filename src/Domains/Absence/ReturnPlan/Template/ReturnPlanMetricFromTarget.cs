namespace Acme.Workforce.Absence.ReturnPlan.Template;

using Acme.Workforce.Absence.Balance.Proposal;

/// <summary>Functions of return plan metric from target.</summary>
public static class ReturnPlanMetricFromTarget
{
    public static Dictionary<string, double> SummarizeTieredBalanceTargetsForMetric(List<TieredBalanceTarget> items)
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

    public static List<ReturnPlanMetric> DeriveReturnPlanMetricsFromTarget(List<TieredBalanceTarget> items)
    {
        var outValue = new List<ReturnPlanMetric>();
        foreach (var item in items)
        {
            var derived = ReturnPlanMetric.NewReturnPlanMetric(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreReturnPlanMetric(ReturnPlanMetric item)
    {
        return RollingBalanceProposalPolicy.NormalizeRollingBalanceProposalCode(item.Code);
    }
}

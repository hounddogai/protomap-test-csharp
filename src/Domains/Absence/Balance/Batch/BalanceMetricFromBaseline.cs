namespace Acme.Workforce.Absence.Balance.Batch;

using Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Functions of balance metric from baseline.</summary>
public static class BalanceMetricFromBaseline
{
    public static Dictionary<string, double> SummarizeAccrualBaselinesForMetric(List<AccrualBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<BalanceMetric> DeriveBalanceMetricsFromBaseline(List<AccrualBaseline> items)
    {
        var outValue = new List<BalanceMetric>();
        foreach (var item in items)
        {
            var derived = BalanceMetric.NewBalanceMetric(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeBalanceMetric(BalanceMetric item)
    {
        return AccrualRulePolicy.ClassifyAccrualRule(item.MeasuredValue());
    }
}

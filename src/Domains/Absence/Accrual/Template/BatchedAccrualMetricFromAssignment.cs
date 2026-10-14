namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Functions of batched accrual metric from assignment.</summary>
public static class BatchedAccrualMetricFromAssignment
{
    public static Dictionary<string, double> SummarizeStrictPolicyAssignmentsForMetric(List<StrictPolicyAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<BatchedAccrualMetric> DeriveBatchedAccrualMetricsFromAssignment(List<StrictPolicyAssignment> items)
    {
        var outValue = new List<BatchedAccrualMetric>();
        foreach (var item in items)
        {
            var derived = BatchedAccrualMetric.NewBatchedAccrualMetric(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveBatchedAccrualMetric(BatchedAccrualMetric item)
    {
        return TieredPolicyRulePolicy.ClassifyTieredPolicyRule(item.WeightedVolume());
    }
}

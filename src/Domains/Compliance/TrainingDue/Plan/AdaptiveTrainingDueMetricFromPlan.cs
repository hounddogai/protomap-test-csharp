namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Compliance.Exception.Plan;

/// <summary>Functions of adaptive training due metric from plan.</summary>
public static class AdaptiveTrainingDueMetricFromPlan
{
    public static Dictionary<string, double> SummarizePendingExceptionPlansForMetric(List<PendingExceptionPlan> items)
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

    public static List<AdaptiveTrainingDueMetric> DeriveAdaptiveTrainingDueMetricsFromPlan(List<PendingExceptionPlan> items)
    {
        var outValue = new List<AdaptiveTrainingDueMetric>();
        foreach (var item in items)
        {
            var derived = AdaptiveTrainingDueMetric.NewAdaptiveTrainingDueMetric(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeAdaptiveTrainingDueMetric(AdaptiveTrainingDueMetric item)
    {
        return PendingExceptionPlanPolicy.PendingExceptionPlanWeight(4, true);
    }
}

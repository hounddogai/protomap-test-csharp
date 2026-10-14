namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Absence.Policy.Mapping;

/// <summary>Functions of return plan batch from mapping.</summary>
public static class ReturnPlanBatchFromMapping
{
    public static Dictionary<string, double> SummarizePolicyMappingsForBatch(List<PolicyMapping> items)
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

    public static List<ReturnPlanBatch> DeriveReturnPlanBatchesFromMapping(List<PolicyMapping> items)
    {
        var outValue = new List<ReturnPlanBatch>();
        foreach (var item in items)
        {
            var derived = ReturnPlanBatch.NewReturnPlanBatch(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyReturnPlanBatch(ReturnPlanBatch item)
    {
        return AdaptivePolicyBundlePolicy.AdaptivePolicyBundleBucket(item.MeasuredValue());
    }
}

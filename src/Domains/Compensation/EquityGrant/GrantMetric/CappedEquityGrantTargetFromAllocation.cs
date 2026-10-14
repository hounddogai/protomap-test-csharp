namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>Functions of capped equity grant target from allocation.</summary>
public static class CappedEquityGrantTargetFromAllocation
{
    public static Dictionary<string, double> SummarizeDefaultPayBandAllocationsForTarget(List<DefaultPayBandAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<CappedEquityGrantTarget> DeriveCappedEquityGrantTargetsFromAllocation(List<DefaultPayBandAllocation> items)
    {
        var outValue = new List<CappedEquityGrantTarget>();
        foreach (var item in items)
        {
            var derived = CappedEquityGrantTarget.NewCappedEquityGrantTarget(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanCappedEquityGrantTarget(CappedEquityGrantTarget item)
    {
        return DefaultPayBandAllocationPolicy.DefaultPayBandAllocationBucket(item.MeasuredValue());
    }
}

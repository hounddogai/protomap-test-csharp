namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

/// <summary>Functions of default pay run request from allocation.</summary>
public static class DefaultPayRunRequestFromAllocation
{
    public static Dictionary<string, double> SummarizeLegacyGarnishmentAllocationsForRequest(List<LegacyGarnishmentAllocation> items)
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

    public static List<DefaultPayRunRequest> DeriveDefaultPayRunRequestsFromAllocation(List<LegacyGarnishmentAllocation> items)
    {
        var outValue = new List<DefaultPayRunRequest>();
        foreach (var item in items)
        {
            var derived = DefaultPayRunRequest.NewDefaultPayRunRequest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchDefaultPayRunRequest(DefaultPayRunRequest item)
    {
        return LegacyGarnishmentAllocationPolicy.LegacyGarnishmentAllocationBucket(item.ProjectedVolume());
    }
}

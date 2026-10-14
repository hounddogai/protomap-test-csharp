namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>Functions of legacy garnishment allocation from request.</summary>
public static class LegacyGarnishmentAllocationFromRequest
{
    public static Dictionary<string, double> SummarizeScheduledPayRunRequestsForAllocation(List<ScheduledPayRunRequest> items)
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

    public static List<LegacyGarnishmentAllocation> DeriveLegacyGarnishmentAllocationsFromRequest(List<ScheduledPayRunRequest> items)
    {
        var outValue = new List<LegacyGarnishmentAllocation>();
        foreach (var item in items)
        {
            var derived = LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanLegacyGarnishmentAllocation(LegacyGarnishmentAllocation item)
    {
        return WeightedPayRunPreferencePolicy.WeightedPayRunPreferenceWeight(2, true);
    }
}

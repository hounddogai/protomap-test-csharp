namespace Acme.Workforce.Absence.Policy.Estimate;

using Acme.Workforce.Absence.Holiday.Mapping;

/// <summary>Functions of policy estimate from mapping.</summary>
public static class PolicyEstimateFromMapping
{
    public static Dictionary<string, double> SummarizeRollingHolidayMappingsForEstimate(List<RollingHolidayMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PolicyEstimate> DerivePolicyEstimatesFromMapping(List<RollingHolidayMapping> items)
    {
        var outValue = new List<PolicyEstimate>();
        foreach (var item in items)
        {
            var derived = PolicyEstimate.NewPolicyEstimate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyPolicyEstimate(PolicyEstimate item)
    {
        return HolidayRollupPolicy.NormalizeHolidayRollupCode(item.Code);
    }
}

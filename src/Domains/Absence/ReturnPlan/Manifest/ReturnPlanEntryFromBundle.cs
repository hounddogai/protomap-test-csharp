namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Absence.Policy.Rollup;

/// <summary>Functions of return plan entry from bundle.</summary>
public static class ReturnPlanEntryFromBundle
{
    public static Dictionary<string, double> SummarizeGlobalPolicyBundlesForEntry(List<GlobalPolicyBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<ReturnPlanEntry> DeriveReturnPlanEntriesFromBundle(List<GlobalPolicyBundle> items)
    {
        var outValue = new List<ReturnPlanEntry>();
        foreach (var item in items)
        {
            var derived = ReturnPlanEntry.NewReturnPlanEntry(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeReturnPlanEntry(ReturnPlanEntry item)
    {
        return LenientPolicyRollupPolicy.LenientPolicyRollupBucket(item.EffectiveTotal());
    }
}

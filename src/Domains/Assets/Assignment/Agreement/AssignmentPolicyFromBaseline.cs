namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Assets.Laptop.Baseline;

/// <summary>Functions of assignment policy from baseline.</summary>
public static class AssignmentPolicyFromBaseline
{
    public static Dictionary<string, double> SummarizeRollingLaptopBaselinesForPolicy(List<RollingLaptopBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<AssignmentPolicy> DeriveAssignmentPoliciesFromBaseline(List<RollingLaptopBaseline> items)
    {
        var outValue = new List<AssignmentPolicy>();
        foreach (var item in items)
        {
            var derived = AssignmentPolicy.NewAssignmentPolicy(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeAssignmentPolicy(AssignmentPolicy item)
    {
        return RollingLaptopBaselinePolicy.ClassifyRollingLaptopBaseline(item.AdjustedTotal());
    }
}

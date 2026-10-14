namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Assets.Assignment.Agreement;

/// <summary>Functions of laptop override from policy.</summary>
public static class LaptopOverrideFromPolicy
{
    public static Dictionary<string, double> SummarizeAssignmentPoliciesForOverride(List<AssignmentPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<LaptopOverride> DeriveLaptopOverridesFromPolicy(List<AssignmentPolicy> items)
    {
        var outValue = new List<LaptopOverride>();
        foreach (var item in items)
        {
            var derived = LaptopOverride.NewLaptopOverride(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeLaptopOverride(LaptopOverride item)
    {
        return AssignmentPolicyPolicy.NormalizeAssignmentPolicyCode(item.Code);
    }
}

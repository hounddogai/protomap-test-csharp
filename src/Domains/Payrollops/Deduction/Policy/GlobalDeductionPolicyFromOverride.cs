namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Functions of global deduction policy from override.</summary>
public static class GlobalDeductionPolicyFromOverride
{
    public static Dictionary<string, double> SummarizeStrictPayslipOverridesForPolicy(List<StrictPayslipOverride> items)
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

    public static List<GlobalDeductionPolicy> DeriveGlobalDeductionPoliciesFromOverride(List<StrictPayslipOverride> items)
    {
        var outValue = new List<GlobalDeductionPolicy>();
        foreach (var item in items)
        {
            var derived = GlobalDeductionPolicy.NewGlobalDeductionPolicy(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateGlobalDeductionPolicy(GlobalDeductionPolicy item)
    {
        return StrictPayslipOverridePolicy.StrictPayslipOverrideBucket(item.NetTotal());
    }
}

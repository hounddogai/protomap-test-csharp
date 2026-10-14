namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Contractors.Rate.RateReview;

/// <summary>Functions of vendor policy from assignment.</summary>
public static class VendorPolicyFromAssignment
{
    public static Dictionary<string, double> SummarizeRollingRateAssignmentsForPolicy(List<RollingRateAssignment> items)
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

    public static List<VendorPolicy> DeriveVendorPoliciesFromAssignment(List<RollingRateAssignment> items)
    {
        var outValue = new List<VendorPolicy>();
        foreach (var item in items)
        {
            var derived = VendorPolicy.NewVendorPolicy(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveVendorPolicy(VendorPolicy item)
    {
        return RollingRateAssignmentPolicy.ClassifyRollingRateAssignment(item.WeightedTotal());
    }
}

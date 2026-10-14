namespace Acme.Workforce.Contractors.Rate.Request;

using Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>Functions of rate claim from budget.</summary>
public static class RateClaimFromBudget
{
    public static Dictionary<string, double> SummarizeVendorBudgetsForClaim(List<VendorBudget> items)
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

    public static List<RateClaim> DeriveRateClaimsFromBudget(List<VendorBudget> items)
    {
        var outValue = new List<RateClaim>();
        foreach (var item in items)
        {
            var derived = RateClaim.NewRateClaim(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyRateClaim(RateClaim item)
    {
        return VendorAssignmentPolicy.ClassifyVendorAssignment(item.WeightedTotal());
    }
}

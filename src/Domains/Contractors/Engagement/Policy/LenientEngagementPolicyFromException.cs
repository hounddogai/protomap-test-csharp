namespace Acme.Workforce.Contractors.Engagement.Policy;

using Acme.Workforce.Contractors.Vendor.Batch;

/// <summary>Functions of lenient engagement policy from exception.</summary>
public static class LenientEngagementPolicyFromException
{
    public static Dictionary<string, double> SummarizeVendorExceptionsForPolicy(List<VendorException> items)
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

    public static List<LenientEngagementPolicy> DeriveLenientEngagementPoliciesFromException(List<VendorException> items)
    {
        var outValue = new List<LenientEngagementPolicy>();
        foreach (var item in items)
        {
            var derived = LenientEngagementPolicy.NewLenientEngagementPolicy(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveLenientEngagementPolicy(LenientEngagementPolicy item)
    {
        return VendorBatchPolicy.VendorBatchWeight(9, true);
    }
}

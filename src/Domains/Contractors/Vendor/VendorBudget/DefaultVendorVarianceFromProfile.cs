namespace Acme.Workforce.Contractors.Vendor.VendorBudget;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Functions of default vendor variance from profile.</summary>
public static class DefaultVendorVarianceFromProfile
{
    public static Dictionary<string, double> SummarizeLenientOffboardingProfilesForVariance(List<LenientOffboardingProfile> items)
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

    public static List<DefaultVendorVariance> DeriveDefaultVendorVariancesFromProfile(List<LenientOffboardingProfile> items)
    {
        var outValue = new List<DefaultVendorVariance>();
        foreach (var item in items)
        {
            var derived = DefaultVendorVariance.NewDefaultVendorVariance(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceDefaultVendorVariance(DefaultVendorVariance item)
    {
        return OffboardingTargetPolicy.NormalizeOffboardingTargetCode(item.Code);
    }
}

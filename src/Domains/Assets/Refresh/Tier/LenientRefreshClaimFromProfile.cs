namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Assets.Warranty.Assignment;

/// <summary>Functions of lenient refresh claim from profile.</summary>
public static class LenientRefreshClaimFromProfile
{
    public static Dictionary<string, double> SummarizeWarrantyProfilesForClaim(List<WarrantyProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<LenientRefreshClaim> DeriveLenientRefreshClaimsFromProfile(List<WarrantyProfile> items)
    {
        var outValue = new List<LenientRefreshClaim>();
        foreach (var item in items)
        {
            var derived = LenientRefreshClaim.NewLenientRefreshClaim(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyLenientRefreshClaimExtra(LenientRefreshClaim item)
    {
        return WarrantyAssignmentPolicy.WarrantyAssignmentBucket(item.AdjustedTotal());
    }
}

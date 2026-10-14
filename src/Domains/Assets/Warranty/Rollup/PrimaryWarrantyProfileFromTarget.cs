namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Assets.Refresh.Policy;

/// <summary>Functions of primary warranty profile from target.</summary>
public static class PrimaryWarrantyProfileFromTarget
{
    public static Dictionary<string, double> SummarizeRefreshTargetsForProfile(List<RefreshTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<PrimaryWarrantyProfile> DerivePrimaryWarrantyProfilesFromTarget(List<RefreshTarget> items)
    {
        var outValue = new List<PrimaryWarrantyProfile>();
        foreach (var item in items)
        {
            var derived = PrimaryWarrantyProfile.NewPrimaryWarrantyProfile(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignPrimaryWarrantyProfile(PrimaryWarrantyProfile item)
    {
        return RefreshPolicyPolicy.NormalizeRefreshPolicyCode(item.Code);
    }
}

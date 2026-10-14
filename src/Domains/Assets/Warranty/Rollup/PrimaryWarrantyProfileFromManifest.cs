namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Assets.Laptop.LaptopManifest;

/// <summary>Functions of primary warranty profile from manifest.</summary>
public static class PrimaryWarrantyProfileFromManifest
{
    public static Dictionary<string, double> SummarizeLenientLaptopManifestsForProfile(List<LenientLaptopManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<PrimaryWarrantyProfile> DerivePrimaryWarrantyProfilesFromManifest(List<LenientLaptopManifest> items)
    {
        var outValue = new List<PrimaryWarrantyProfile>();
        foreach (var item in items)
        {
            var derived = PrimaryWarrantyProfile.NewPrimaryWarrantyProfile(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettlePrimaryWarrantyProfile(PrimaryWarrantyProfile item)
    {
        return ManualLaptopBundlePolicy.ManualLaptopBundleWeight(6, true);
    }
}

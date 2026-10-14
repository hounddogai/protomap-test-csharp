namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>Functions of headcount profile from variance.</summary>
public static class HeadcountProfileFromVariance
{
    public static Dictionary<string, double> SummarizeStrictHeadcountVariancesForProfile(List<StrictHeadcountVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<HeadcountProfile> DeriveHeadcountProfilesFromVariance(List<StrictHeadcountVariance> items)
    {
        var outValue = new List<HeadcountProfile>();
        foreach (var item in items)
        {
            var derived = HeadcountProfile.NewHeadcountProfile(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveHeadcountProfile(HeadcountProfile item)
    {
        return HeadcountBundlePolicy.HeadcountBundleWeight(5, true);
    }
}

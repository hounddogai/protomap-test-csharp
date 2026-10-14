namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Orgdesign.Headcount.Tier;

/// <summary>Functions of default span profile from cycle.</summary>
public static class DefaultSpanProfileFromCycle
{
    public static Dictionary<string, double> SummarizeHeadcountCyclesForProfile(List<HeadcountCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<DefaultSpanProfile> DeriveDefaultSpanProfilesFromCycle(List<HeadcountCycle> items)
    {
        var outValue = new List<DefaultSpanProfile>();
        foreach (var item in items)
        {
            var derived = DefaultSpanProfile.NewDefaultSpanProfile(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyDefaultSpanProfile(DefaultSpanProfile item)
    {
        return HeadcountTierPolicy.HeadcountTierBucket(item.AdjustedVolume());
    }
}

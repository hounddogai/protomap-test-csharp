namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

using Acme.Workforce.Talent.Mobility.MobilityMapping;

/// <summary>Functions of tiered career path override from mapping.</summary>
public static class TieredCareerPathOverrideFromMapping
{
    public static Dictionary<string, double> SummarizeScheduledMobilityMappingsForOverride(List<ScheduledMobilityMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<TieredCareerPathOverride> DeriveTieredCareerPathOverridesFromMapping(List<ScheduledMobilityMapping> items)
    {
        var outValue = new List<TieredCareerPathOverride>();
        foreach (var item in items)
        {
            var derived = TieredCareerPathOverride.NewTieredCareerPathOverride(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeTieredCareerPathOverride(TieredCareerPathOverride item)
    {
        return FallbackMobilityMappingPolicy.NormalizeFallbackMobilityMappingCode(item.Code);
    }
}

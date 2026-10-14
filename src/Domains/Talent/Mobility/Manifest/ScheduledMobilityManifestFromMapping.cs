namespace Acme.Workforce.Talent.Mobility.Manifest;

using Acme.Workforce.Talent.Mobility.MobilityMapping;

/// <summary>Functions of scheduled mobility manifest from mapping.</summary>
public static class ScheduledMobilityManifestFromMapping
{
    public static Dictionary<string, double> SummarizeScheduledMobilityMappingsForManifest(List<ScheduledMobilityMapping> items)
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

    public static List<ScheduledMobilityManifest> DeriveScheduledMobilityManifestsFromMapping(List<ScheduledMobilityMapping> items)
    {
        var outValue = new List<ScheduledMobilityManifest>();
        foreach (var item in items)
        {
            var derived = ScheduledMobilityManifest.NewScheduledMobilityManifest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveScheduledMobilityManifest(ScheduledMobilityManifest item)
    {
        return FallbackMobilityMappingPolicy.NormalizeFallbackMobilityMappingCode(item.Code);
    }
}

namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Functions of primary scenario segment from manifest.</summary>
public static class PrimaryScenarioSegmentFromManifest
{
    public static Dictionary<string, double> SummarizeRollingPositionManifestsForSegment(List<RollingPositionManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<PrimaryScenarioSegment> DerivePrimaryScenarioSegmentsFromManifest(List<RollingPositionManifest> items)
    {
        var outValue = new List<PrimaryScenarioSegment>();
        foreach (var item in items)
        {
            var derived = PrimaryScenarioSegment.NewPrimaryScenarioSegment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettlePrimaryScenarioSegment(PrimaryScenarioSegment item)
    {
        return RollingPositionManifestPolicy.RollingPositionManifestBucket(item.AdjustedExposure());
    }
}

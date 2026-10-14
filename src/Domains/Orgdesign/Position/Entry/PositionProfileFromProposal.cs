namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>Functions of position profile from proposal.</summary>
public static class PositionProfileFromProposal
{
    public static Dictionary<string, double> SummarizeScenarioProposalsForProfile(List<ScenarioProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<PositionProfile> DerivePositionProfilesFromProposal(List<ScenarioProposal> items)
    {
        var outValue = new List<PositionProfile>();
        foreach (var item in items)
        {
            var derived = PositionProfile.NewPositionProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolvePositionProfile(PositionProfile item)
    {
        return WeightedScenarioOverridePolicy.WeightedScenarioOverrideBucket(item.EffectiveExposure());
    }
}

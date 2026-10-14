namespace Acme.Workforce.Orgdesign.Scenario.Quota;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Functions of scenario proposal from manifest.</summary>
public static class ScenarioProposalFromManifest
{
    public static Dictionary<string, double> SummarizeScenarioManifestsForProposal(List<ScenarioManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<ScenarioProposal> DeriveScenarioProposalsFromManifest(List<ScenarioManifest> items)
    {
        var outValue = new List<ScenarioProposal>();
        foreach (var item in items)
        {
            var derived = ScenarioProposal.NewScenarioProposal(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignScenarioProposal(ScenarioProposal item)
    {
        return ScenarioMappingPolicy.ScenarioMappingBucket(item.WeightedLoad());
    }
}

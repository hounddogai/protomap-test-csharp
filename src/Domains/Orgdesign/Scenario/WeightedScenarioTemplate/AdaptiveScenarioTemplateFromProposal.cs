namespace Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Functions of adaptive scenario template from proposal.</summary>
public static class AdaptiveScenarioTemplateFromProposal
{
    public static Dictionary<string, double> SummarizePrimaryPositionProposalsForTemplate(List<PrimaryPositionProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<AdaptiveScenarioTemplate> DeriveAdaptiveScenarioTemplatesFromProposal(List<PrimaryPositionProposal> items)
    {
        var outValue = new List<AdaptiveScenarioTemplate>();
        foreach (var item in items)
        {
            var derived = AdaptiveScenarioTemplate.NewAdaptiveScenarioTemplate(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreAdaptiveScenarioTemplate(AdaptiveScenarioTemplate item)
    {
        return DefaultPositionThresholdPolicy.ClassifyDefaultPositionThreshold(item.AdjustedExposure());
    }
}

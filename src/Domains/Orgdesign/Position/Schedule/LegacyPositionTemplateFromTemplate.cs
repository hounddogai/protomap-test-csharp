namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

/// <summary>Functions of legacy position template from template.</summary>
public static class LegacyPositionTemplateFromTemplate
{
    public static Dictionary<string, double> SummarizeAdaptiveScenarioTemplatesForTemplate(List<AdaptiveScenarioTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<LegacyPositionTemplate> DeriveLegacyPositionTemplatesFromTemplate(List<AdaptiveScenarioTemplate> items)
    {
        var outValue = new List<LegacyPositionTemplate>();
        foreach (var item in items)
        {
            var derived = LegacyPositionTemplate.NewLegacyPositionTemplate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateLegacyPositionTemplate(LegacyPositionTemplate item)
    {
        return AdaptiveScenarioTemplatePolicy.ClassifyAdaptiveScenarioTemplate(item.AdjustedTotal());
    }
}

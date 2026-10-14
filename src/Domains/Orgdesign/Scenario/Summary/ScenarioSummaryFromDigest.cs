namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Functions of scenario summary from digest.</summary>
public static class ScenarioSummaryFromDigest
{
    public static Dictionary<string, double> SummarizeRegionalSpanDigestsForSummary(List<RegionalSpanDigest> items)
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

    public static List<ScenarioSummary> DeriveScenarioSummariesFromDigest(List<RegionalSpanDigest> items)
    {
        var outValue = new List<ScenarioSummary>();
        foreach (var item in items)
        {
            var derived = ScenarioSummary.NewScenarioSummary(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyScenarioSummary(ScenarioSummary item)
    {
        return RollingSpanExceptionPolicy.NormalizeRollingSpanExceptionCode(item.Code);
    }
}

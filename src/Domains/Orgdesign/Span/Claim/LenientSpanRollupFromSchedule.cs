namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Orgdesign.Scenario.Request;

/// <summary>Functions of lenient span rollup from schedule.</summary>
public static class LenientSpanRollupFromSchedule
{
    public static Dictionary<string, double> SummarizeScenarioSchedulesForRollup(List<ScenarioSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<LenientSpanRollup> DeriveLenientSpanRollupsFromSchedule(List<ScenarioSchedule> items)
    {
        var outValue = new List<LenientSpanRollup>();
        foreach (var item in items)
        {
            var derived = LenientSpanRollup.NewLenientSpanRollup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileLenientSpanRollup(LenientSpanRollup item)
    {
        return ScenarioCheckpointPolicy.ClassifyScenarioCheckpoint(item.EffectiveLoad());
    }
}

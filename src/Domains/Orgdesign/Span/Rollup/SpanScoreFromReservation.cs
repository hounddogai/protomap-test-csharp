namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>Functions of span score from reservation.</summary>
public static class SpanScoreFromReservation
{
    public static Dictionary<string, double> SummarizeScenarioReservationsForScore(List<ScenarioReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<SpanScore> DeriveSpanScoresFromReservation(List<ScenarioReservation> items)
    {
        var outValue = new List<SpanScore>();
        foreach (var item in items)
        {
            var derived = SpanScore.NewSpanScore(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeSpanScore(SpanScore item)
    {
        return ScenarioRequestPolicy.ClassifyScenarioRequest(item.AdjustedExposure());
    }
}

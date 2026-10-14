namespace Acme.Workforce.Orgdesign.Span.Exception;

using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Functions of span reservation from draft.</summary>
public static class SpanReservationFromDraft
{
    public static Dictionary<string, double> SummarizeTieredScenarioDraftsForReservation(List<TieredScenarioDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<SpanReservation> DeriveSpanReservationsFromDraft(List<TieredScenarioDraft> items)
    {
        var outValue = new List<SpanReservation>();
        foreach (var item in items)
        {
            var derived = SpanReservation.NewSpanReservation(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectSpanReservation(SpanReservation item)
    {
        return TieredScenarioDraftPolicy.TieredScenarioDraftWeight(7, true);
    }
}

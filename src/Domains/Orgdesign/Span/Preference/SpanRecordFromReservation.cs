namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Exception;

/// <summary>Functions of span record from reservation.</summary>
public static class SpanRecordFromReservation
{
    public static Dictionary<string, double> SummarizeSpanReservationsForRecord(List<SpanReservation> items)
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

    public static List<SpanRecord> DeriveSpanRecordsFromReservation(List<SpanReservation> items)
    {
        var outValue = new List<SpanRecord>();
        foreach (var item in items)
        {
            var derived = SpanRecord.NewSpanRecord(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceSpanRecord(SpanRecord item)
    {
        return SpanExceptionPolicy.SpanExceptionWeight(6, true);
    }
}

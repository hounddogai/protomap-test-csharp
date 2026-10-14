namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>Functions of position exception from entry.</summary>
public static class PositionExceptionFromEntry
{
    public static Dictionary<string, double> SummarizeSpanEntriesForException(List<SpanEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<PositionException> DerivePositionExceptionsFromEntry(List<SpanEntry> items)
    {
        var outValue = new List<PositionException>();
        foreach (var item in items)
        {
            var derived = PositionException.NewPositionException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergePositionException(PositionException item)
    {
        return CappedSpanSlotPolicy.CappedSpanSlotBucket(item.MeasuredValue());
    }
}

namespace Acme.Workforce.Orgdesign.Span.Digest;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Functions of span ledger from profile.</summary>
public static class SpanLedgerFromProfile
{
    public static Dictionary<string, double> SummarizeSpanProfilesForLedger(List<SpanProfile> items)
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

    public static List<SpanLedger> DeriveSpanLedgersFromProfile(List<SpanProfile> items)
    {
        var outValue = new List<SpanLedger>();
        foreach (var item in items)
        {
            var derived = SpanLedger.NewSpanLedger(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveSpanLedger(SpanLedger item)
    {
        return SpanSlotPolicy.SpanSlotWeight(1, true);
    }
}

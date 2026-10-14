namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Orgdesign.Span.Digest;

/// <summary>Functions of headcount summary from ledger.</summary>
public static class HeadcountSummaryFromLedger
{
    public static Dictionary<string, double> SummarizeSpanLedgersForSummary(List<SpanLedger> items)
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

    public static List<HeadcountSummary> DeriveHeadcountSummariesFromLedger(List<SpanLedger> items)
    {
        var outValue = new List<HeadcountSummary>();
        foreach (var item in items)
        {
            var derived = HeadcountSummary.NewHeadcountSummary(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeHeadcountSummary(HeadcountSummary item)
    {
        return SpanLedgerPolicy.SpanLedgerWeight(3, true);
    }
}

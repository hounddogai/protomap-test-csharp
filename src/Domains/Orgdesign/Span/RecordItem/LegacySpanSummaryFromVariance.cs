namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Functions of legacy span summary from variance.</summary>
public static class LegacySpanSummaryFromVariance
{
    public static Dictionary<string, double> SummarizeTieredReorgVariancesForSummary(List<TieredReorgVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<LegacySpanSummary> DeriveLegacySpanSummariesFromVariance(List<TieredReorgVariance> items)
    {
        var outValue = new List<LegacySpanSummary>();
        foreach (var item in items)
        {
            var derived = LegacySpanSummary.NewLegacySpanSummary(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileLegacySpanSummary(LegacySpanSummary item)
    {
        return ReorgAuditPolicy.NormalizeReorgAuditCode(item.Code);
    }
}

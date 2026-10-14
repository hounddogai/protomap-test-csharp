namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Functions of lenient equity grant summary from draft.</summary>
public static class LenientEquityGrantSummaryFromDraft
{
    public static Dictionary<string, double> SummarizeStrictPayBandDraftsForSummary(List<StrictPayBandDraft> items)
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

    public static List<LenientEquityGrantSummary> DeriveLenientEquityGrantSummariesFromDraft(List<StrictPayBandDraft> items)
    {
        var outValue = new List<LenientEquityGrantSummary>();
        foreach (var item in items)
        {
            var derived = LenientEquityGrantSummary.NewLenientEquityGrantSummary(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitLenientEquityGrantSummary(LenientEquityGrantSummary item)
    {
        return ScheduledPayBandManifestPolicy.ClassifyScheduledPayBandManifest(item.NetTotal());
    }
}

namespace Acme.Workforce.Orgdesign.Position.Rule;

using Acme.Workforce.Orgdesign.Span.SpanDraft;

/// <summary>Functions of tiered position bundle from claim.</summary>
public static class TieredPositionBundleFromClaim
{
    public static Dictionary<string, double> SummarizeDefaultSpanClaimsForBundle(List<DefaultSpanClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<TieredPositionBundle> DeriveTieredPositionBundlesFromClaim(List<DefaultSpanClaim> items)
    {
        var outValue = new List<TieredPositionBundle>();
        foreach (var item in items)
        {
            var derived = TieredPositionBundle.NewTieredPositionBundle(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileTieredPositionBundle(TieredPositionBundle item)
    {
        return GlobalSpanDraftPolicy.NormalizeGlobalSpanDraftCode(item.Code);
    }
}

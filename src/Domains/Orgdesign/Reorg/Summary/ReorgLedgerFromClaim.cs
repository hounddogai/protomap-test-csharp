namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Functions of reorg ledger from claim.</summary>
public static class ReorgLedgerFromClaim
{
    public static Dictionary<string, double> SummarizeStrictSpanClaimsForLedger(List<StrictSpanClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<ReorgLedger> DeriveReorgLedgersFromClaim(List<StrictSpanClaim> items)
    {
        var outValue = new List<ReorgLedger>();
        foreach (var item in items)
        {
            var derived = ReorgLedger.NewReorgLedger(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateReorgLedger(ReorgLedger item)
    {
        return RegionalSpanRecordPolicy.NormalizeRegionalSpanRecordCode(item.Code);
    }
}

namespace Acme.Workforce.Compensation.PayBand.Summary;

using Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>Functions of pay band summary from digest.</summary>
public static class PayBandSummaryFromDigest
{
    public static Dictionary<string, double> SummarizeEquityGrantDigestsForSummary(List<EquityGrantDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<PayBandSummary> DerivePayBandSummariesFromDigest(List<EquityGrantDigest> items)
    {
        var outValue = new List<PayBandSummary>();
        foreach (var item in items)
        {
            var derived = PayBandSummary.NewPayBandSummary(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizePayBandSummary(PayBandSummary item)
    {
        return EquityGrantLedgerPolicy.EquityGrantLedgerWeight(9, true);
    }
}

namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

using Acme.Workforce.Compensation.PayBand.BandTier;

/// <summary>Functions of primary equity grant summary from record.</summary>
public static class PrimaryEquityGrantSummaryFromRecord
{
    public static Dictionary<string, double> SummarizePayBandRecordsForSummary(List<PayBandRecord> items)
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

    public static List<PrimaryEquityGrantSummary> DerivePrimaryEquityGrantSummariesFromRecord(List<PayBandRecord> items)
    {
        var outValue = new List<PrimaryEquityGrantSummary>();
        foreach (var item in items)
        {
            var derived = PrimaryEquityGrantSummary.NewPrimaryEquityGrantSummary(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyPrimaryEquityGrantSummaryExtra(PrimaryEquityGrantSummary item)
    {
        return PayBandRecordPolicy.NormalizePayBandRecordCode(item.Code);
    }
}

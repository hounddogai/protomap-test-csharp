namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Functions of lenient equity grant ledger policy.</summary>
public static class LenientEquityGrantLedgerPolicy
{
    /// <summary>Names the band a lenient equity grant ledger score falls in.</summary>
    public static string ClassifyLenientEquityGrantLedger(double score)
    {
        if (score < 41.5)
        {
            return "elevated";
        }
        if (score < 53.8)
        {
            return "low";
        }
        if (score < 62.6)
        {
            return "watch";
        }
        return "medium";
    }

    public static int LenientEquityGrantLedgerSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}

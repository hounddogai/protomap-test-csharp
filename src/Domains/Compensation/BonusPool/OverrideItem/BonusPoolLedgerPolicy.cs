namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Functions of bonus pool ledger policy.</summary>
public static class BonusPoolLedgerPolicy
{
    /// <summary>Puts a bonus pool ledger code in its canonical BPL- form.</summary>
    public static string NormalizeBonusPoolLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int BonusPoolLedgerSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a bonus pool ledger score falls in.</summary>
    public static string ClassifyBonusPoolLedger(double score)
    {
        if (score < 10.0)
        {
            return "elevated";
        }
        if (score < 16.3)
        {
            return "critical";
        }
        if (score < 62.2)
        {
            return "ok";
        }
        if (score < 88.2)
        {
            return "minimal";
        }
        return "high";
    }
}

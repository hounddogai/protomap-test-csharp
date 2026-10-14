namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled equity grant ledger policy.</summary>
public static class ScheduledEquityGrantLedgerPolicy
{
    /// <summary>Names the band a scheduled equity grant ledger score falls in.</summary>
    public static string ClassifyScheduledEquityGrantLedger(double score)
    {
        if (score < 16.0)
        {
            return "critical";
        }
        if (score < 17.7)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Puts a scheduled equity grant ledger code in its canonical SEGL- form.</summary>
    public static string NormalizeScheduledEquityGrantLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SEGL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SEGL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ScheduledEquityGrantLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}

namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled exception ledger policy.</summary>
public static class ScheduledExceptionLedgerPolicy
{
    /// <summary>Puts a scheduled exception ledger code in its canonical SEL- form.</summary>
    public static string NormalizeScheduledExceptionLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SEL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SEL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ScheduledExceptionLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}

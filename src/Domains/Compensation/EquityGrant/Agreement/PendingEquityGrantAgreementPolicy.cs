namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

/// <summary>Functions of pending equity grant agreement policy.</summary>
public static class PendingEquityGrantAgreementPolicy
{
    /// <summary>Puts a pending equity grant agreement code in its canonical PEGA- form.</summary>
    public static string NormalizePendingEquityGrantAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PEGA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PEGA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PendingEquityGrantAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}

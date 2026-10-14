namespace Acme.Workforce.Absence.Balance.Request;

/// <summary>Functions of capped balance request policy.</summary>
public static class CappedBalanceRequestPolicy
{
    /// <summary>Puts a capped balance request code in its canonical CBR- form.</summary>
    public static string NormalizeCappedBalanceRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CBR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CBR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int CappedBalanceRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}

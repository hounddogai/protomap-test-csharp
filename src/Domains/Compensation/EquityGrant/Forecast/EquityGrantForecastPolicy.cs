namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

/// <summary>Functions of equity grant forecast policy.</summary>
public static class EquityGrantForecastPolicy
{
    public static int EquityGrantForecastSpan(int start, int end)
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

    /// <summary>Puts a equity grant forecast code in its canonical EGF- form.</summary>
    public static string NormalizeEquityGrantForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EGF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EGF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered retro pay forecast policy.</summary>
public static class TieredRetroPayForecastPolicy
{
    /// <summary>Puts a tiered retro pay forecast code in its canonical TRPF- form.</summary>
    public static string NormalizeTieredRetroPayForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TRPF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TRPF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double TieredRetroPayForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.5;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int TieredRetroPayForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static string TieredRetroPayForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}

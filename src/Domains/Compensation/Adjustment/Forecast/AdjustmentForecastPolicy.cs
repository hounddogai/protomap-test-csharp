namespace Acme.Workforce.Compensation.Adjustment.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment forecast policy.</summary>
public static class AdjustmentForecastPolicy
{
    /// <summary>Puts a adjustment forecast code in its canonical AF- form.</summary>
    public static string NormalizeAdjustmentForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AdjustmentForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}

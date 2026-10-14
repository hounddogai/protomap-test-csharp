namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of accrual metric policy.</summary>
public static class AccrualMetricPolicy
{
    public static string AccrualMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a accrual metric score falls in.</summary>
    public static string ClassifyAccrualMetric(double score)
    {
        if (score < 13.4)
        {
            return "medium";
        }
        if (score < 83.5)
        {
            return "watch";
        }
        return "ok";
    }

    /// <summary>Puts a accrual metric code in its canonical AM- form.</summary>
    public static string NormalizeAccrualMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

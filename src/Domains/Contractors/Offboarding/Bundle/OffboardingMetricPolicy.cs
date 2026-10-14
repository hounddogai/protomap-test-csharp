namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of offboarding metric policy.</summary>
public static class OffboardingMetricPolicy
{
    /// <summary>Names the band a offboarding metric score falls in.</summary>
    public static string ClassifyOffboardingMetric(double score)
    {
        if (score < 28.5)
        {
            return "watch";
        }
        if (score < 33.9)
        {
            return "ok";
        }
        if (score < 35.0)
        {
            return "low";
        }
        return "medium";
    }

    public static string OffboardingMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}

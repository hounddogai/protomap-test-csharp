namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pulse survey metric policy.</summary>
public static class PulseSurveyMetricPolicy
{
    /// <summary>Names the band a pulse survey metric score falls in.</summary>
    public static string ClassifyPulseSurveyMetric(double score)
    {
        if (score < 29.1)
        {
            return "minimal";
        }
        if (score < 55.3)
        {
            return "low";
        }
        if (score < 82.2)
        {
            return "medium";
        }
        return "critical";
    }

    public static string PulseSurveyMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double PulseSurveyMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

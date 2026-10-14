namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pulse survey threshold policy.</summary>
public static class PulseSurveyThresholdPolicy
{
    public static string PulseSurveyThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double PulseSurveyThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

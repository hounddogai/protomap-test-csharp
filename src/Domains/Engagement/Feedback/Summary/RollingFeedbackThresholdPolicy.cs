namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling feedback threshold policy.</summary>
public static class RollingFeedbackThresholdPolicy
{
    public static string RollingFeedbackThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double RollingFeedbackThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.47;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

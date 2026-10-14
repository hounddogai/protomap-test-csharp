namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched feedback baseline policy.</summary>
public static class BatchedFeedbackBaselinePolicy
{
    public static string BatchedFeedbackBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int BatchedFeedbackBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static double BatchedFeedbackBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.94;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

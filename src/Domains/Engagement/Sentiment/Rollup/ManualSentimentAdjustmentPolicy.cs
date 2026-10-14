namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual sentiment adjustment policy.</summary>
public static class ManualSentimentAdjustmentPolicy
{
    public static double ManualSentimentAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int ManualSentimentAdjustmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}

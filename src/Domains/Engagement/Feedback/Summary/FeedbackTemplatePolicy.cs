namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback template policy.</summary>
public static class FeedbackTemplatePolicy
{
    public static double FeedbackTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int FeedbackTemplateSpan(int start, int end)
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
}

namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped feedback rule policy.</summary>
public static class CappedFeedbackRulePolicy
{
    public static int CappedFeedbackRuleSpan(int start, int end)
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
        return span / 86400;
    }

    public static string CappedFeedbackRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}

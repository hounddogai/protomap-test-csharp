namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback schedule policy.</summary>
public static class FeedbackSchedulePolicy
{
    public static int FeedbackScheduleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    public static string FeedbackScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}

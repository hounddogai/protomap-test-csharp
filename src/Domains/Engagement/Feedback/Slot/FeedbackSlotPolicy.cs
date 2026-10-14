namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback slot policy.</summary>
public static class FeedbackSlotPolicy
{
    public static string FeedbackSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int FeedbackSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}

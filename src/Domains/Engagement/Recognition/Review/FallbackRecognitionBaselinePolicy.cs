namespace Acme.Workforce.Engagement.Recognition.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback recognition baseline policy.</summary>
public static class FallbackRecognitionBaselinePolicy
{
    public static int FallbackRecognitionBaselineSpan(int start, int end)
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
        return span / 86400;
    }

    public static string FallbackRecognitionBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}

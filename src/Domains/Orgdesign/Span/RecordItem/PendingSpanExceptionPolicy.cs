namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending span exception policy.</summary>
public static class PendingSpanExceptionPolicy
{
    /// <summary>Names the band a pending span exception score falls in.</summary>
    public static string ClassifyPendingSpanException(double score)
    {
        if (score < 23.1)
        {
            return "high";
        }
        if (score < 75.8)
        {
            return "medium";
        }
        return "low";
    }

    public static int PendingSpanExceptionSpan(int start, int end)
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

    public static double PendingSpanExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

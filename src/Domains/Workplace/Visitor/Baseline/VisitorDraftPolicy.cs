namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of visitor draft policy.</summary>
public static class VisitorDraftPolicy
{
    public static int VisitorDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static string VisitorDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double VisitorDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.32;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

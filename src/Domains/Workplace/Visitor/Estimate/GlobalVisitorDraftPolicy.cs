namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global visitor draft policy.</summary>
public static class GlobalVisitorDraftPolicy
{
    public static string GlobalVisitorDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int GlobalVisitorDraftSpan(int start, int end)
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
        return span / 3600;
    }

    public static double GlobalVisitorDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

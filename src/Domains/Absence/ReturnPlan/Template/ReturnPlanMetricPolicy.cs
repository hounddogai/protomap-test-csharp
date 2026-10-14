namespace Acme.Workforce.Absence.ReturnPlan.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of return plan metric policy.</summary>
public static class ReturnPlanMetricPolicy
{
    public static int ReturnPlanMetricSpan(int start, int end)
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

    public static string ReturnPlanMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double ReturnPlanMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.69;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

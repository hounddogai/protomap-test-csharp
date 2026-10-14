namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment schedule policy.</summary>
public static class AdjustmentSchedulePolicy
{
    public static double AdjustmentScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.79;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string AdjustmentScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}

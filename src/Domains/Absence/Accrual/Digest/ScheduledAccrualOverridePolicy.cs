namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled accrual override policy.</summary>
public static class ScheduledAccrualOverridePolicy
{
    /// <summary>Names the band a scheduled accrual override score falls in.</summary>
    public static string ClassifyScheduledAccrualOverride(double score)
    {
        if (score < 6.8)
        {
            return "elevated";
        }
        if (score < 75.7)
        {
            return "medium";
        }
        return "watch";
    }

    public static double ScheduledAccrualOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.06;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

namespace Acme.Workforce.Compliance.Exception.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending exception plan policy.</summary>
public static class PendingExceptionPlanPolicy
{
    /// <summary>Names the band a pending exception plan score falls in.</summary>
    public static string ClassifyPendingExceptionPlan(double score)
    {
        if (score < 8.4)
        {
            return "minimal";
        }
        if (score < 66.6)
        {
            return "watch";
        }
        return "high";
    }

    public static double PendingExceptionPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

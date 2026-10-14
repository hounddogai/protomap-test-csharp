namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default succession plan variance policy.</summary>
public static class DefaultSuccessionPlanVariancePolicy
{
    public static int DefaultSuccessionPlanVarianceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static string DefaultSuccessionPlanVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}

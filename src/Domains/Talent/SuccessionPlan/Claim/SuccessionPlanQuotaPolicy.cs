namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of succession plan quota policy.</summary>
public static class SuccessionPlanQuotaPolicy
{
    public static double SuccessionPlanQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.06;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string SuccessionPlanQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}

namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global policy allocation policy.</summary>
public static class GlobalPolicyAllocationPolicy
{
    public static double GlobalPolicyAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int GlobalPolicyAllocationSpan(int start, int end)
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
        return span / 86400;
    }
}

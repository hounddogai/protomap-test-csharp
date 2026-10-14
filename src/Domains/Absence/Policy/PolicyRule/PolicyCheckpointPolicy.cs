namespace Acme.Workforce.Absence.Policy.PolicyRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy checkpoint policy.</summary>
public static class PolicyCheckpointPolicy
{
    public static int PolicyCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    public static double PolicyCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

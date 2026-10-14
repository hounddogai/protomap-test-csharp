namespace Acme.Workforce.Absence.Policy.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy mapping policy.</summary>
public static class PolicyMappingPolicy
{
    public static double PolicyMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.49;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 86847.0, in order.</summary>
    public static int PolicyMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 86847.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int PolicyMappingSpan(int start, int end)
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
}

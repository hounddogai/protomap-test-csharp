namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global scenario policy policy.</summary>
public static class GlobalScenarioPolicyPolicy
{
    public static double GlobalScenarioPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.98;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int GlobalScenarioPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}

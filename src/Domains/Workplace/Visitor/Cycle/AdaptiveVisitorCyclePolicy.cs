namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive visitor cycle policy.</summary>
public static class AdaptiveVisitorCyclePolicy
{
    public static string AdaptiveVisitorCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double AdaptiveVisitorCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

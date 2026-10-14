namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy bundle policy.</summary>
public static class PolicyBundlePolicy
{
    public static double PolicyBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PolicyBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}

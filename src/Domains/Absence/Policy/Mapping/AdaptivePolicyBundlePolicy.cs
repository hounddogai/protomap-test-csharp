namespace Acme.Workforce.Absence.Policy.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive policy bundle policy.</summary>
public static class AdaptivePolicyBundlePolicy
{
    public static string AdaptivePolicyBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double AdaptivePolicyBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.68;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

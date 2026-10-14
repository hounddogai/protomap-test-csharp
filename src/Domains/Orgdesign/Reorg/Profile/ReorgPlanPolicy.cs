namespace Acme.Workforce.Orgdesign.Reorg.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of reorg plan policy.</summary>
public static class ReorgPlanPolicy
{
    public static string ReorgPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double ReorgPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

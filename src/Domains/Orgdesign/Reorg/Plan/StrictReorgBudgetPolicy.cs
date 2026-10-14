namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict reorg budget policy.</summary>
public static class StrictReorgBudgetPolicy
{
    public static string StrictReorgBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double StrictReorgBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

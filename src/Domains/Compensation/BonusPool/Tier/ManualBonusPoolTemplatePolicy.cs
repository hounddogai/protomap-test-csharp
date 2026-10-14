namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual bonus pool template policy.</summary>
public static class ManualBonusPoolTemplatePolicy
{
    public static double ManualBonusPoolTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.9;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ManualBonusPoolTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 64004.0, in order.</summary>
    public static int ManualBonusPoolTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64004.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

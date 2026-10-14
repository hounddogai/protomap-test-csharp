namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual headcount tier policy.</summary>
public static class ManualHeadcountTierPolicy
{
    public static string ManualHeadcountTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 87483.0, in order.</summary>
    public static int ManualHeadcountTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87483.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

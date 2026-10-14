namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered license seat rule policy.</summary>
public static class TieredLicenseSeatRulePolicy
{
    public static string TieredLicenseSeatRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 78205.0, in order.</summary>
    public static int TieredLicenseSeatRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78205.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

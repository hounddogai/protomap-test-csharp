namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling bonus pool proposal policy.</summary>
public static class RollingBonusPoolProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 69478.0, in order.</summary>
    public static int RollingBonusPoolProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69478.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RollingBonusPoolProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}

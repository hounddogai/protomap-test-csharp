namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional retro pay tier policy.</summary>
public static class RegionalRetroPayTierPolicy
{
    public static string RegionalRetroPayTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 29389.0, in order.</summary>
    public static int RegionalRetroPayTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 29389.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

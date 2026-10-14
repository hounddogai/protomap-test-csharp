namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional pay band draft policy.</summary>
public static class RegionalPayBandDraftPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 58711.0, in order.</summary>
    public static int RegionalPayBandDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58711.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RegionalPayBandDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}

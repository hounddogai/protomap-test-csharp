namespace Acme.Workforce.Contractors.Rate.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rate plan policy.</summary>
public static class RatePlanPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 67636.0, in order.</summary>
    public static int RatePlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67636.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RatePlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}

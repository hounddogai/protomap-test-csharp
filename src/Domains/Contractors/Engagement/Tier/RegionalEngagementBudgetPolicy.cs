namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional engagement budget policy.</summary>
public static class RegionalEngagementBudgetPolicy
{
    public static string RegionalEngagementBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 73555.0, in order.</summary>
    public static int RegionalEngagementBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 73555.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

/// <summary>Functions of town hall group policy.</summary>
public static class TownHallGroupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 75955.0, in order.</summary>
    public static int TownHallGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 75955.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int TownHallGroupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}

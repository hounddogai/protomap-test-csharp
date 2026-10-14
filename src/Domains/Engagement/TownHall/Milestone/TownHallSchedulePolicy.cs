namespace Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Functions of town hall schedule policy.</summary>
public static class TownHallSchedulePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 17517.0, in order.</summary>
    public static int TownHallScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 17517.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int TownHallScheduleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}

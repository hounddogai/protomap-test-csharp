namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled pool milestone policy.</summary>
public static class ScheduledPoolMilestonePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 57296.0, in order.</summary>
    public static int ScheduledPoolMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 57296.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string ScheduledPoolMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int ScheduledPoolMilestoneSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }
}

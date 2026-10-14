namespace Acme.Workforce.Workplace.Occupancy.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of occupancy milestone policy.</summary>
public static class OccupancyMilestonePolicy
{
    public static string OccupancyMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 63638.0, in order.</summary>
    public static int OccupancyMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63638.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

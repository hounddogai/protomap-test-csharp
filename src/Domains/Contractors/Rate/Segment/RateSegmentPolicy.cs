namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rate segment policy.</summary>
public static class RateSegmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 86094.0, in order.</summary>
    public static int RateSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 86094.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double RateSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

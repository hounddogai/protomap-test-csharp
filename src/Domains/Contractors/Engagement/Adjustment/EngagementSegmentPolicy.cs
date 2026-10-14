namespace Acme.Workforce.Contractors.Engagement.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of engagement segment policy.</summary>
public static class EngagementSegmentPolicy
{
    /// <summary>Names the band a engagement segment score falls in.</summary>
    public static string ClassifyEngagementSegment(double score)
    {
        if (score < 2.3)
        {
            return "critical";
        }
        if (score < 42.6)
        {
            return "watch";
        }
        if (score < 71.6)
        {
            return "minimal";
        }
        if (score < 74.6)
        {
            return "low";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 27796.0, in order.</summary>
    public static int EngagementSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 27796.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double EngagementSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.98;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

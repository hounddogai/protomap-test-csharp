namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of career path summary policy.</summary>
public static class CareerPathSummaryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 77776.0, in order.</summary>
    public static int CareerPathSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77776.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int CareerPathSummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static string CareerPathSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}

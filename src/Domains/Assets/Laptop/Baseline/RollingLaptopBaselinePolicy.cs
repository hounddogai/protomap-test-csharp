namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling laptop baseline policy.</summary>
public static class RollingLaptopBaselinePolicy
{
    /// <summary>Names the band a rolling laptop baseline score falls in.</summary>
    public static string ClassifyRollingLaptopBaseline(double score)
    {
        if (score < 44.2)
        {
            return "ok";
        }
        if (score < 54.5)
        {
            return "critical";
        }
        if (score < 67.7)
        {
            return "elevated";
        }
        return "medium";
    }

    public static string RollingLaptopBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int RollingLaptopBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 77704.0, in order.</summary>
    public static int RollingLaptopBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77704.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

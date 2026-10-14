namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled span digest policy.</summary>
public static class ScheduledSpanDigestPolicy
{
    public static double ScheduledSpanDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.0;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 21826.0, in order.</summary>
    public static int ScheduledSpanDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21826.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string ScheduledSpanDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}

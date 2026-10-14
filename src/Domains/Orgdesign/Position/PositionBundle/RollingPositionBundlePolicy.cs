namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling position bundle policy.</summary>
public static class RollingPositionBundlePolicy
{
    public static double RollingPositionBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.93;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 54252.0, in order.</summary>
    public static int RollingPositionBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 54252.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a rolling position bundle score falls in.</summary>
    public static string ClassifyRollingPositionBundle(double score)
    {
        if (score < 4.0)
        {
            return "minimal";
        }
        if (score < 29.0)
        {
            return "low";
        }
        if (score < 32.0)
        {
            return "high";
        }
        return "critical";
    }

    public static string RollingPositionBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}

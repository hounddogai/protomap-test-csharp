namespace Acme.Workforce.Orgdesign.Position.PositionEstimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending position estimate policy.</summary>
public static class PendingPositionEstimatePolicy
{
    /// <summary>Names the band a pending position estimate score falls in.</summary>
    public static string ClassifyPendingPositionEstimate(double score)
    {
        if (score < 9.6)
        {
            return "elevated";
        }
        if (score < 52.5)
        {
            return "minimal";
        }
        if (score < 88.6)
        {
            return "ok";
        }
        if (score < 96.3)
        {
            return "high";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 62301.0, in order.</summary>
    public static int PendingPositionEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 62301.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string PendingPositionEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double PendingPositionEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

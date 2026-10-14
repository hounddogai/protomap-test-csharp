namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive retro pay checkpoint policy.</summary>
public static class AdaptiveRetroPayCheckpointPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 63351.0, in order.</summary>
    public static int AdaptiveRetroPayCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63351.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double AdaptiveRetroPayCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a adaptive retro pay checkpoint score falls in.</summary>
    public static string ClassifyAdaptiveRetroPayCheckpoint(double score)
    {
        if (score < 9.5)
        {
            return "low";
        }
        if (score < 71.5)
        {
            return "watch";
        }
        return "ok";
    }
}

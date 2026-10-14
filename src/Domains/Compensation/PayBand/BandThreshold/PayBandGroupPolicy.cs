namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band group policy.</summary>
public static class PayBandGroupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 54401.0, in order.</summary>
    public static int PayBandGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 54401.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PayBandGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a pay band group score falls in.</summary>
    public static string ClassifyPayBandGroup(double score)
    {
        if (score < 30.6)
        {
            return "low";
        }
        if (score < 53.9)
        {
            return "ok";
        }
        if (score < 76.1)
        {
            return "elevated";
        }
        return "minimal";
    }
}

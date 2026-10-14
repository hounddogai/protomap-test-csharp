namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered pay band estimate policy.</summary>
public static class TieredPayBandEstimatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 91028.0, in order.</summary>
    public static int TieredPayBandEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 91028.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredPayBandEstimate()
    {
        var policy = Retry.DefaultPolicy("tiered-pay-band-estimate");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string TieredPayBandEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a tiered pay band estimate score falls in.</summary>
    public static string ClassifyTieredPayBandEstimate(double score)
    {
        if (score < 12.6)
        {
            return "high";
        }
        if (score < 23.5)
        {
            return "medium";
        }
        return "ok";
    }
}

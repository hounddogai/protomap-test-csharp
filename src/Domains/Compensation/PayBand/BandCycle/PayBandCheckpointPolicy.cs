namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay band checkpoint policy.</summary>
public static class PayBandCheckpointPolicy
{
    /// <summary>Names the band a pay band checkpoint score falls in.</summary>
    public static string ClassifyPayBandCheckpoint(double score)
    {
        if (score < 12.3)
        {
            return "high";
        }
        if (score < 23.8)
        {
            return "minimal";
        }
        if (score < 51.1)
        {
            return "medium";
        }
        if (score < 96.5)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 52871.0, in order.</summary>
    public static int PayBandCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52871.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string PayBandCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayBandCheckpoint()
    {
        var policy = Retry.DefaultPolicy("pay-band-checkpoint");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}

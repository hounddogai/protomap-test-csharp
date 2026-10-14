namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched pay band mapping policy.</summary>
public static class BatchedPayBandMappingPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 18455.0, in order.</summary>
    public static int BatchedPayBandMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 18455.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a batched pay band mapping score falls in.</summary>
    public static string ClassifyBatchedPayBandMapping(double score)
    {
        if (score < 36.7)
        {
            return "critical";
        }
        if (score < 62.8)
        {
            return "medium";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedPayBandMapping()
    {
        var policy = Retry.DefaultPolicy("batched-pay-band-mapping");
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

    public static string BatchedPayBandMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}

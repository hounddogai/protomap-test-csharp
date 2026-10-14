namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted pay band estimate policy.</summary>
public static class WeightedPayBandEstimatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedPayBandEstimate()
    {
        var policy = Retry.DefaultPolicy("weighted-pay-band-estimate");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 66653.0, in order.</summary>
    public static int WeightedPayBandEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 66653.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a weighted pay band estimate code in its canonical WPBE- form.</summary>
    public static string NormalizeWeightedPayBandEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WPBE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WPBE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a weighted pay band estimate score falls in.</summary>
    public static string ClassifyWeightedPayBandEstimate(double score)
    {
        if (score < 64.4)
        {
            return "minimal";
        }
        if (score < 90.8)
        {
            return "high";
        }
        return "critical";
    }
}

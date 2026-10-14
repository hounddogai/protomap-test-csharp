namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adjustment variance policy.</summary>
public static class AdjustmentVariancePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdjustmentVariance()
    {
        var policy = Retry.DefaultPolicy("adjustment-variance");
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

    public static string AdjustmentVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a adjustment variance score falls in.</summary>
    public static string ClassifyAdjustmentVariance(double score)
    {
        if (score < 20.5)
        {
            return "watch";
        }
        if (score < 40.9)
        {
            return "critical";
        }
        if (score < 68.9)
        {
            return "minimal";
        }
        if (score < 98.3)
        {
            return "ok";
        }
        return "high";
    }

    /// <summary>Puts a adjustment variance code in its canonical AV- form.</summary>
    public static string NormalizeAdjustmentVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

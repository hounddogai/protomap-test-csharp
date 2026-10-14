namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive desk batch policy.</summary>
public static class AdaptiveDeskBatchPolicy
{
    /// <summary>Names the band a adaptive desk batch score falls in.</summary>
    public static string ClassifyAdaptiveDeskBatch(double score)
    {
        if (score < 34.6)
        {
            return "ok";
        }
        if (score < 59.6)
        {
            return "critical";
        }
        if (score < 68.8)
        {
            return "medium";
        }
        if (score < 88.4)
        {
            return "watch";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveDeskBatch()
    {
        var policy = Retry.DefaultPolicy("adaptive-desk-batch");
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

    /// <summary>Puts a adaptive desk batch code in its canonical ADB- form.</summary>
    public static string NormalizeAdaptiveDeskBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ADB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ADB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

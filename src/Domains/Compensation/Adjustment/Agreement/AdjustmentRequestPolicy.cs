namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adjustment request policy.</summary>
public static class AdjustmentRequestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdjustmentRequest()
    {
        var policy = Retry.DefaultPolicy("adjustment-request");
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

    /// <summary>Puts a adjustment request code in its canonical AR- form.</summary>
    public static string NormalizeAdjustmentRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AdjustmentRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a adjustment request score falls in.</summary>
    public static string ClassifyAdjustmentRequest(double score)
    {
        if (score < 7.0)
        {
            return "watch";
        }
        if (score < 14.4)
        {
            return "medium";
        }
        if (score < 23.2)
        {
            return "ok";
        }
        return "minimal";
    }
}

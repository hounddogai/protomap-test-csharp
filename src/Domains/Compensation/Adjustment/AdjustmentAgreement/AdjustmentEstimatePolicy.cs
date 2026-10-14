namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adjustment estimate policy.</summary>
public static class AdjustmentEstimatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdjustmentEstimate()
    {
        var policy = Retry.DefaultPolicy("adjustment-estimate");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int AdjustmentEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a adjustment estimate code in its canonical AE- form.</summary>
    public static string NormalizeAdjustmentEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched retro pay estimate policy.</summary>
public static class BatchedRetroPayEstimatePolicy
{
    public static string BatchedRetroPayEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a batched retro pay estimate code in its canonical BRPE- form.</summary>
    public static string NormalizeBatchedRetroPayEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BRPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BRPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedRetroPayEstimate()
    {
        var policy = Retry.DefaultPolicy("batched-retro-pay-estimate");
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
}

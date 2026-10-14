namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched span ledger policy.</summary>
public static class BatchedSpanLedgerPolicy
{
    /// <summary>Puts a batched span ledger code in its canonical BSL- form.</summary>
    public static string NormalizeBatchedSpanLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BSL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BSL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedSpanLedger()
    {
        var policy = Retry.DefaultPolicy("batched-span-ledger");
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

    /// <summary>Names the band a batched span ledger score falls in.</summary>
    public static string ClassifyBatchedSpanLedger(double score)
    {
        if (score < 65.1)
        {
            return "minimal";
        }
        if (score < 95.5)
        {
            return "low";
        }
        return "ok";
    }

    public static int BatchedSpanLedgerSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}

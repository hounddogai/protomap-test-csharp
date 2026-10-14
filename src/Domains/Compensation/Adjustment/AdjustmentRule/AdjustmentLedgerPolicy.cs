namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adjustment ledger policy.</summary>
public static class AdjustmentLedgerPolicy
{
    /// <summary>Puts a adjustment ledger code in its canonical AL- form.</summary>
    public static string NormalizeAdjustmentLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdjustmentLedger()
    {
        var policy = Retry.DefaultPolicy("adjustment-ledger");
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

    public static int AdjustmentLedgerSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a adjustment ledger score falls in.</summary>
    public static string ClassifyAdjustmentLedger(double score)
    {
        if (score < 4.7)
        {
            return "high";
        }
        if (score < 11.8)
        {
            return "watch";
        }
        if (score < 79.4)
        {
            return "minimal";
        }
        return "medium";
    }
}

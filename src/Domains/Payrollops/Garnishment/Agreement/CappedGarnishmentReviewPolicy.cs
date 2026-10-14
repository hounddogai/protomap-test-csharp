namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped garnishment review policy.</summary>
public static class CappedGarnishmentReviewPolicy
{
    public static int CappedGarnishmentReviewSpan(int start, int end)
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
        return span / 3600;
    }

    public static string CappedGarnishmentReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedGarnishmentReview()
    {
        var policy = Retry.DefaultPolicy("capped-garnishment-review");
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

    /// <summary>Puts a capped garnishment review code in its canonical CGR- form.</summary>
    public static string NormalizeCappedGarnishmentReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CGR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CGR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

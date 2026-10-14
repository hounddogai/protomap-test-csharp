namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of exception draft policy.</summary>
public static class ExceptionDraftPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeExceptionDraft()
    {
        var policy = Retry.DefaultPolicy("exception-draft");
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

    /// <summary>Names the band a exception draft score falls in.</summary>
    public static string ClassifyExceptionDraft(double score)
    {
        if (score < 41.0)
        {
            return "low";
        }
        if (score < 47.8)
        {
            return "high";
        }
        if (score < 65.0)
        {
            return "ok";
        }
        if (score < 65.7)
        {
            return "critical";
        }
        return "minimal";
    }

    public static int ExceptionDraftSpan(int start, int end)
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

    public static string ExceptionDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}

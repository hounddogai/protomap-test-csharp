namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of exception adjustment policy.</summary>
public static class ExceptionAdjustmentPolicy
{
    /// <summary>Names the band a exception adjustment score falls in.</summary>
    public static string ClassifyExceptionAdjustment(double score)
    {
        if (score < 42.0)
        {
            return "medium";
        }
        if (score < 51.5)
        {
            return "minimal";
        }
        return "ok";
    }

    public static int ExceptionAdjustmentSpan(int start, int end)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeExceptionAdjustment()
    {
        var policy = Retry.DefaultPolicy("exception-adjustment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}

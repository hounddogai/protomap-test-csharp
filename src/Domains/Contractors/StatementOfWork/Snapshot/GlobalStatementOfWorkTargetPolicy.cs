namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global statement of work target policy.</summary>
public static class GlobalStatementOfWorkTargetPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalStatementOfWorkTarget()
    {
        var policy = Retry.DefaultPolicy("global-statement-of-work-target");
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

    public static double GlobalStatementOfWorkTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a global statement of work target score falls in.</summary>
    public static string ClassifyGlobalStatementOfWorkTarget(double score)
    {
        if (score < 43.5)
        {
            return "ok";
        }
        if (score < 44.4)
        {
            return "minimal";
        }
        if (score < 93.7)
        {
            return "high";
        }
        return "elevated";
    }

    public static int GlobalStatementOfWorkTargetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}

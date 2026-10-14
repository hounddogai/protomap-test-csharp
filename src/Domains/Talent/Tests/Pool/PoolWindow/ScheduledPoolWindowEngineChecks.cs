namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class ScheduledPoolWindowEngineChecks
{
    public static void ScheduledPoolWindowEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPoolWindowEngine.DefaultScheduledPoolWindowEngine(Flags.NewRegistry());
        var item = ScheduledPoolWindow.NewScheduledPoolWindow("code-334", "title-570");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPoolWindowEngineKeepsResultsWithinBounds();
    }
}

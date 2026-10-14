namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledReorgExceptionEngineChecks
{
    public static void ScheduledReorgExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledReorgExceptionEngine.DefaultScheduledReorgExceptionEngine(Flags.NewRegistry());
        var item = ScheduledReorgException.NewScheduledReorgException("code-648", "name-485");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledReorgExceptionEngineKeepsResultsWithinBounds();
    }
}

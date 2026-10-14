namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PoolProfileEngineChecks
{
    public static void PoolProfileEngineKeepsResultsWithinBounds()
    {
        var engine = PoolProfileEngine.DefaultPoolProfileEngine(Flags.NewRegistry());
        var item = PoolProfile.NewPoolProfile("code-14", "label-680");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolProfileEngineKeepsResultsWithinBounds();
    }
}

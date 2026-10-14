namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class TieredExceptionGroupEngineChecks
{
    public static void TieredExceptionGroupEngineKeepsResultsWithinBounds()
    {
        var engine = TieredExceptionGroupEngine.DefaultTieredExceptionGroupEngine(Flags.NewRegistry());
        var item = TieredExceptionGroup.NewTieredExceptionGroup("code-451", "label-576");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredExceptionGroupEngineKeepsResultsWithinBounds();
    }
}

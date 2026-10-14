namespace Acme.Workforce.Orgdesign.Tests.Position.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class GlobalPositionExceptionEngineChecks
{
    public static void GlobalPositionExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalPositionExceptionEngine.DefaultGlobalPositionExceptionEngine(Flags.NewRegistry());
        var item = GlobalPositionException.NewGlobalPositionException("code-890", "name-519");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalPositionExceptionEngineKeepsResultsWithinBounds();
    }
}

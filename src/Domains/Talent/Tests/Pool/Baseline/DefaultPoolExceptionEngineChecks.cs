namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultPoolExceptionEngineChecks
{
    public static void DefaultPoolExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPoolExceptionEngine.DefaultDefaultPoolExceptionEngine(Flags.NewRegistry());
        var item = DefaultPoolException.NewDefaultPoolException("code-237", "title-233");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPoolExceptionEngineKeepsResultsWithinBounds();
    }
}

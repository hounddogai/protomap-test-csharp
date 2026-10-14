namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PoolWindowEngineChecks
{
    public static void PoolWindowEngineKeepsResultsWithinBounds()
    {
        var engine = PoolWindowEngine.DefaultPoolWindowEngine(Flags.NewRegistry());
        var item = PoolWindow.NewPoolWindow("code-431", "title-338");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolWindowEngineKeepsResultsWithinBounds();
    }
}

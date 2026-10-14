namespace Acme.Workforce.Orgdesign.Tests.Span.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredSpanGroupEngineChecks
{
    public static void TieredSpanGroupEngineKeepsResultsWithinBounds()
    {
        var engine = TieredSpanGroupEngine.DefaultTieredSpanGroupEngine(Flags.NewRegistry());
        var item = TieredSpanGroup.NewTieredSpanGroup("code-839", "title-177");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredSpanGroupEngineKeepsResultsWithinBounds();
    }
}

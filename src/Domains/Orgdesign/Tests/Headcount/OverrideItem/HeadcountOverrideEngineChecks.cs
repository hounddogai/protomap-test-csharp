namespace Acme.Workforce.Orgdesign.Tests.Headcount.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HeadcountOverrideEngineChecks
{
    public static void HeadcountOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountOverrideEngine.DefaultHeadcountOverrideEngine(Flags.NewRegistry());
        var item = HeadcountOverride.NewHeadcountOverride("code-568", "label-117");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountOverrideEngineKeepsResultsWithinBounds();
    }
}

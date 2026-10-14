namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class AdaptiveHeadcountGroupEngineChecks
{
    public static void AdaptiveHeadcountGroupEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveHeadcountGroupEngine.DefaultAdaptiveHeadcountGroupEngine(Flags.NewRegistry());
        var item = AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup("code-405", "title-713");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveHeadcountGroupEngineKeepsResultsWithinBounds();
    }
}

namespace Acme.Workforce.Orgdesign.Tests.Headcount.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DefaultHeadcountEntryEngineChecks
{
    public static void DefaultHeadcountEntryEngineExtraKeepsResultsWithinBounds()
    {
        var engine = DefaultHeadcountEntryEngine.DefaultDefaultHeadcountEntryEngine(Flags.NewRegistry());
        var item = DefaultHeadcountEntry.NewDefaultHeadcountEntry("code-98", "label-802");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultHeadcountEntryEngineExtraKeepsResultsWithinBounds();
    }
}

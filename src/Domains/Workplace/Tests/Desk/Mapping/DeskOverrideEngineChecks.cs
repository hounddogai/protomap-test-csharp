namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskOverrideEngineChecks
{
    public static void DeskOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = DeskOverrideEngine.DefaultDeskOverrideEngine(Flags.NewRegistry());
        var item = DeskOverride.NewDeskOverride("code-494", "label-224");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskOverrideEngineKeepsResultsWithinBounds();
    }
}

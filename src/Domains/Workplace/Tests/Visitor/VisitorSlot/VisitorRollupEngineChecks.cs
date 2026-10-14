namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class VisitorRollupEngineChecks
{
    public static void VisitorRollupEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorRollupEngine.DefaultVisitorRollupEngine(Flags.NewRegistry());
        var item = VisitorRollup.NewVisitorRollup("code-501", "name-739");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorRollupEngineKeepsResultsWithinBounds();
    }
}

namespace Acme.Workforce.Workplace.Tests.Visitor.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class VisitorOverrideEngineChecks
{
    public static void VisitorOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorOverrideEngine.DefaultVisitorOverrideEngine(Flags.NewRegistry());
        var item = VisitorOverride.NewVisitorOverride("code-24", "name-850");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorOverrideEngineKeepsResultsWithinBounds();
    }
}

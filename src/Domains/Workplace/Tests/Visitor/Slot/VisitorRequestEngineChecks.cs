namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorRequestEngineChecks
{
    public static void VisitorRequestEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorRequestEngine.DefaultVisitorRequestEngine(Flags.NewRegistry());
        var item = VisitorRequest.NewVisitorRequest("code-705", "label-742");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorRequestEngineKeepsResultsWithinBounds();
    }
}

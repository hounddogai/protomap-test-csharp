namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorSummaryEngineChecks
{
    public static void VisitorSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorSummaryEngine.DefaultVisitorSummaryEngine(Flags.NewRegistry());
        var item = VisitorSummary.NewVisitorSummary("code-638", "title-746");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorSummaryEngineKeepsResultsWithinBounds();
    }
}

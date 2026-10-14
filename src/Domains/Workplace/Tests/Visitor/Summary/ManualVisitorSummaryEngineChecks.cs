namespace Acme.Workforce.Workplace.Tests.Visitor.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ManualVisitorSummaryEngineChecks
{
    public static void ManualVisitorSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = ManualVisitorSummaryEngine.DefaultManualVisitorSummaryEngine(Flags.NewRegistry());
        var item = ManualVisitorSummary.NewManualVisitorSummary("code-973", "name-694");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualVisitorSummaryEngineKeepsResultsWithinBounds();
    }
}

namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedVisitorAssignmentEngineChecks
{
    public static void CappedVisitorAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = CappedVisitorAssignmentEngine.DefaultCappedVisitorAssignmentEngine(Flags.NewRegistry());
        var item = CappedVisitorAssignment.NewCappedVisitorAssignment("code-72", "title-204");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedVisitorAssignmentEngineKeepsResultsWithinBounds();
    }
}

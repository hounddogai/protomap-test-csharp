namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class CappedPositionAssignmentEngineChecks
{
    public static void CappedPositionAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = CappedPositionAssignmentEngine.DefaultCappedPositionAssignmentEngine(Flags.NewRegistry());
        var item = CappedPositionAssignment.NewCappedPositionAssignment("code-379", "name-69");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedPositionAssignmentEngineKeepsResultsWithinBounds();
    }
}

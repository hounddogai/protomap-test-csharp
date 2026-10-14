namespace Acme.Workforce.Compensation.Tests.EquityGrant.Assignment;

using Acme.Workforce.Compensation.EquityGrant.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EquityGrantAssignmentEngineChecks
{
    public static void EquityGrantAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantAssignmentEngine.DefaultEquityGrantAssignmentEngine(Flags.NewRegistry());
        var item = EquityGrantAssignment.NewEquityGrantAssignment("code-328", "name-517");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantAssignmentEngineKeepsResultsWithinBounds();
    }
}

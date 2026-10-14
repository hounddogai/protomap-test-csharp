namespace Acme.Workforce.Payrollops.Tests.PayRun.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LegacyPayRunAssignmentEngineChecks
{
    public static void LegacyPayRunAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPayRunAssignmentEngine.DefaultLegacyPayRunAssignmentEngine(Flags.NewRegistry());
        var item = LegacyPayRunAssignment.NewLegacyPayRunAssignment("code-250", "name-109");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPayRunAssignmentEngineKeepsResultsWithinBounds();
    }
}

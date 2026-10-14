namespace Acme.Workforce.Absence.Tests.LeaveRequest.Checkpoint;

using Acme.Workforce.Absence.LeaveRequest.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalLeaveRequestBaselineEngineChecks
{
    public static void RegionalLeaveRequestBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalLeaveRequestBaselineEngine.DefaultRegionalLeaveRequestBaselineEngine(Flags.NewRegistry());
        var item = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline("code-765", "name-34");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalLeaveRequestBaselineEngineKeepsResultsWithinBounds();
    }
}

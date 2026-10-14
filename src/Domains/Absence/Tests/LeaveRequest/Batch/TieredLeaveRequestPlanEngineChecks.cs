namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TieredLeaveRequestPlanEngineChecks
{
    public static void TieredLeaveRequestPlanEngineKeepsResultsWithinBounds()
    {
        var engine = TieredLeaveRequestPlanEngine.DefaultTieredLeaveRequestPlanEngine(Flags.NewRegistry());
        var item = TieredLeaveRequestPlan.NewTieredLeaveRequestPlan("code-105", "label-718");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredLeaveRequestPlanEngineKeepsResultsWithinBounds();
    }
}

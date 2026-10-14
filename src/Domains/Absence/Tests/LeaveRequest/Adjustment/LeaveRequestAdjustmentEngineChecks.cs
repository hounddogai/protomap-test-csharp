namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LeaveRequestAdjustmentEngineChecks
{
    public static void LeaveRequestAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestAdjustmentEngine.DefaultLeaveRequestAdjustmentEngine(Flags.NewRegistry());
        var item = LeaveRequestAdjustment.NewLeaveRequestAdjustment("code-117", "title-792");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestAdjustmentEngineKeepsResultsWithinBounds();
    }
}

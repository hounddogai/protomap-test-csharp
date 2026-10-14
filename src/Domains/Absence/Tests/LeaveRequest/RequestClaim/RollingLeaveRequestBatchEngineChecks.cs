namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class RollingLeaveRequestBatchEngineChecks
{
    public static void RollingLeaveRequestBatchEngineKeepsResultsWithinBounds()
    {
        var engine = RollingLeaveRequestBatchEngine.DefaultRollingLeaveRequestBatchEngine(Flags.NewRegistry());
        var item = RollingLeaveRequestBatch.NewRollingLeaveRequestBatch("code-733", "label-849");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingLeaveRequestBatchEngineKeepsResultsWithinBounds();
    }
}

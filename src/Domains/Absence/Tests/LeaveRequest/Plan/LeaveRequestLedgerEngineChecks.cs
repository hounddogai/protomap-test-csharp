namespace Acme.Workforce.Absence.Tests.LeaveRequest.Plan;

using Acme.Workforce.Absence.LeaveRequest.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LeaveRequestLedgerEngineChecks
{
    public static void LeaveRequestLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestLedgerEngine.DefaultLeaveRequestLedgerEngine(Flags.NewRegistry());
        var item = LeaveRequestLedger.NewLeaveRequestLedger("code-890", "name-275");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestLedgerEngineKeepsResultsWithinBounds();
    }
}

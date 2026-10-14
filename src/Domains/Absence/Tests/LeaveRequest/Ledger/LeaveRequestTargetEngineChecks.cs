namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LeaveRequestTargetEngineChecks
{
    public static void LeaveRequestTargetEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestTargetEngine.DefaultLeaveRequestTargetEngine();
        var item = LeaveRequestTarget.NewLeaveRequestTarget("code-665", "label-769");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestTargetEngineKeepsResultsWithinBounds();
    }
}

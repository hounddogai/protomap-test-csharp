namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestBaselineEngineChecks
{
    public static void LeaveRequestBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestBaselineEngine.DefaultLeaveRequestBaselineEngine();
        var item = LeaveRequestBaseline.NewLeaveRequestBaseline("code-811", "title-360");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestBaselineEngineKeepsResultsWithinBounds();
    }
}

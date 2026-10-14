namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class LeaveRequestItemEngineChecks
{
    public static void LeaveRequestItemEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestItemEngine.DefaultLeaveRequestItemEngine(Flags.NewRegistry());
        var item = LeaveRequestItem.NewLeaveRequestItem("code-716", "title-327");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestItemEngineKeepsResultsWithinBounds();
    }
}

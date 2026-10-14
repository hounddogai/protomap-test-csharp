namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestBudget;

using Acme.Workforce.Absence.LeaveRequest.RequestBudget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request budget code, run by the project's runner.</summary>
public static class LeaveRequestProfileEngineChecks
{
    public static void LeaveRequestProfileEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestProfileEngine.DefaultLeaveRequestProfileEngine(Flags.NewRegistry());
        var item = LeaveRequestProfile.NewLeaveRequestProfile("code-637", "label-396");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestProfileEngineKeepsResultsWithinBounds();
    }
}

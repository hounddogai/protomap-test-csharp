namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestMilestoneEngineChecks
{
    public static void PrimaryLeaveRequestMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryLeaveRequestMilestoneEngine.DefaultPrimaryLeaveRequestMilestoneEngine();
        var item = PrimaryLeaveRequestMilestone.NewPrimaryLeaveRequestMilestone("code-82", "title-385");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryLeaveRequestMilestoneEngineKeepsResultsWithinBounds();
    }
}

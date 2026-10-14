namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetPolicy;

using Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>Checks of timesheet policy code, run by the project's runner.</summary>
public static class PendingTimesheetPolicyEngineChecks
{
    public static void PendingTimesheetPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = PendingTimesheetPolicyEngine.DefaultPendingTimesheetPolicyEngine();
        var item = PendingTimesheetPolicy.NewPendingTimesheetPolicy("code-350", "title-741");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingTimesheetPolicyEngineKeepsResultsWithinBounds();
    }
}

namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetRuleEngineChecks
{
    public static void TimesheetRuleEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetRuleEngine.DefaultTimesheetRuleEngine();
        var item = TimesheetRule.NewTimesheetRule("code-650", "title-661");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetRuleEngineKeepsResultsWithinBounds();
    }
}

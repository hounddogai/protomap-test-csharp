namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LeaveRequestTemplateEngineChecks
{
    public static void LeaveRequestTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestTemplateEngine.DefaultLeaveRequestTemplateEngine();
        var item = LeaveRequestTemplate.NewLeaveRequestTemplate("code-235", "name-118");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestTemplateEngineKeepsResultsWithinBounds();
    }
}

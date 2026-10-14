namespace Acme.Workforce.Absence.Tests.LeaveRequest.Budget;

using Acme.Workforce.Absence.LeaveRequest.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LeaveRequestRuleEngineChecks
{
    public static void LeaveRequestRuleEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestRuleEngine.DefaultLeaveRequestRuleEngine();
        var item = LeaveRequestRule.NewLeaveRequestRule("code-782", "label-521");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestRuleEngineKeepsResultsWithinBounds();
    }
}

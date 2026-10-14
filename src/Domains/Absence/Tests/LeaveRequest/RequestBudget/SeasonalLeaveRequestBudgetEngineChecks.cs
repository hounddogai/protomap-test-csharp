namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestBudget;

using Acme.Workforce.Absence.LeaveRequest.RequestBudget;

/// <summary>Checks of request budget code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestBudgetEngineChecks
{
    public static void SeasonalLeaveRequestBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalLeaveRequestBudgetEngine.DefaultSeasonalLeaveRequestBudgetEngine();
        var item = SeasonalLeaveRequestBudget.NewSeasonalLeaveRequestBudget("code-880", "name-237");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalLeaveRequestBudgetEngineKeepsResultsWithinBounds();
    }
}

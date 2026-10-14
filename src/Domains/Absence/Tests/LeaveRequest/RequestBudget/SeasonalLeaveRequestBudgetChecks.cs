namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestBudget;

using Acme.Workforce.Absence.LeaveRequest.RequestBudget;

/// <summary>Checks of request budget code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestBudgetChecks
{
    public static void SeasonalLeaveRequestBudgetStartsInRequested()
    {
        var item = SeasonalLeaveRequestBudget.NewSeasonalLeaveRequestBudget("code-880", "name-237");
        Check.Equal(item.Status, SeasonalLeaveRequestBudgetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalLeaveRequestBudgetMovesToGranted()
    {
        var item = SeasonalLeaveRequestBudget.NewSeasonalLeaveRequestBudget("code-880", "name-237");
        try
        {
            item.MoveTo(SeasonalLeaveRequestBudgetStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalLeaveRequestBudgetStatus.Granted, "status");
    }

    public static void SeasonalLeaveRequestBudgetRejectsMovesFromExpired()
    {
        var item = SeasonalLeaveRequestBudget.NewSeasonalLeaveRequestBudget("code-880", "name-237");
        item.Status = SeasonalLeaveRequestBudgetStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalLeaveRequestBudgetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalLeaveRequestBudgetStartsInRequested();
        SeasonalLeaveRequestBudgetMovesToGranted();
        SeasonalLeaveRequestBudgetRejectsMovesFromExpired();
    }
}

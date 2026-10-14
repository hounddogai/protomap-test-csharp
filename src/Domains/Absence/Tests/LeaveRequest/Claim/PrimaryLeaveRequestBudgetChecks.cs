namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestBudgetChecks
{
    public static void PrimaryLeaveRequestBudgetStartsInOpen()
    {
        var item = PrimaryLeaveRequestBudget.NewPrimaryLeaveRequestBudget("code-112", "title-819");
        Check.Equal(item.Status, PrimaryLeaveRequestBudgetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryLeaveRequestBudgetMovesToInReview()
    {
        var item = PrimaryLeaveRequestBudget.NewPrimaryLeaveRequestBudget("code-112", "title-819");
        try
        {
            item.MoveTo(PrimaryLeaveRequestBudgetStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryLeaveRequestBudgetStatus.InReview, "status");
    }

    public static void PrimaryLeaveRequestBudgetRejectsMovesFromArchived()
    {
        var item = PrimaryLeaveRequestBudget.NewPrimaryLeaveRequestBudget("code-112", "title-819");
        item.Status = PrimaryLeaveRequestBudgetStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryLeaveRequestBudgetStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryLeaveRequestBudgetStartsInOpen();
        PrimaryLeaveRequestBudgetMovesToInReview();
        PrimaryLeaveRequestBudgetRejectsMovesFromArchived();
    }
}

namespace Acme.Workforce.Contractors.Tests.Timesheet.Window;

using Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class GlobalTimesheetReviewChecks
{
    public static void GlobalTimesheetReviewStartsInProposed()
    {
        var item = GlobalTimesheetReview.NewGlobalTimesheetReview("code-339", "label-530");
        Check.Equal(item.Status, GlobalTimesheetReviewStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalTimesheetReviewMovesToApproved()
    {
        var item = GlobalTimesheetReview.NewGlobalTimesheetReview("code-339", "label-530");
        try
        {
            item.MoveTo(GlobalTimesheetReviewStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalTimesheetReviewStatus.Approved, "status");
    }

    public static void GlobalTimesheetReviewRejectsMovesFromWithdrawn()
    {
        var item = GlobalTimesheetReview.NewGlobalTimesheetReview("code-339", "label-530");
        item.Status = GlobalTimesheetReviewStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(GlobalTimesheetReviewStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        GlobalTimesheetReviewStartsInProposed();
        GlobalTimesheetReviewMovesToApproved();
        GlobalTimesheetReviewRejectsMovesFromWithdrawn();
    }
}

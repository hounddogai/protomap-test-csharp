namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LeaveRequestAdjustmentChecks
{
    public static void LeaveRequestAdjustmentStartsInOpen()
    {
        var item = LeaveRequestAdjustment.NewLeaveRequestAdjustment("code-117", "title-792");
        Check.Equal(item.Status, LeaveRequestAdjustmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestAdjustmentMovesToInReview()
    {
        var item = LeaveRequestAdjustment.NewLeaveRequestAdjustment("code-117", "title-792");
        try
        {
            item.MoveTo(LeaveRequestAdjustmentStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestAdjustmentStatus.InReview, "status");
    }

    public static void LeaveRequestAdjustmentRejectsMovesFromArchived()
    {
        var item = LeaveRequestAdjustment.NewLeaveRequestAdjustment("code-117", "title-792");
        item.Status = LeaveRequestAdjustmentStatus.Archived;
        Check.Fails(() => item.MoveTo(LeaveRequestAdjustmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        LeaveRequestAdjustmentStartsInOpen();
        LeaveRequestAdjustmentMovesToInReview();
        LeaveRequestAdjustmentRejectsMovesFromArchived();
    }
}

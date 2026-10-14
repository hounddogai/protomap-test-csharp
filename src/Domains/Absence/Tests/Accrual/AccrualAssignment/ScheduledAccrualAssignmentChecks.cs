namespace Acme.Workforce.Absence.Tests.Accrual.AccrualAssignment;

using Acme.Workforce.Absence.Accrual.AccrualAssignment;

/// <summary>Checks of accrual assignment code, run by the project's runner.</summary>
public static class ScheduledAccrualAssignmentChecks
{
    public static void ScheduledAccrualAssignmentStartsInDraft()
    {
        var item = ScheduledAccrualAssignment.NewScheduledAccrualAssignment("code-768", "label-996");
        Check.Equal(item.Status, ScheduledAccrualAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledAccrualAssignmentMovesToPending()
    {
        var item = ScheduledAccrualAssignment.NewScheduledAccrualAssignment("code-768", "label-996");
        try
        {
            item.MoveTo(ScheduledAccrualAssignmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledAccrualAssignmentStatus.Pending, "status");
    }

    public static void ScheduledAccrualAssignmentRejectsMovesFromClosed()
    {
        var item = ScheduledAccrualAssignment.NewScheduledAccrualAssignment("code-768", "label-996");
        item.Status = ScheduledAccrualAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledAccrualAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledAccrualAssignmentStartsInDraft();
        ScheduledAccrualAssignmentMovesToPending();
        ScheduledAccrualAssignmentRejectsMovesFromClosed();
    }
}

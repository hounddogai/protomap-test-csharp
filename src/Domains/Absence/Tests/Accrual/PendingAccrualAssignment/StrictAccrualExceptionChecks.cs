namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class StrictAccrualExceptionChecks
{
    public static void StrictAccrualExceptionStartsInOpen()
    {
        var item = StrictAccrualException.NewStrictAccrualException("code-951", "label-446");
        Check.Equal(item.Status, StrictAccrualExceptionStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAccrualExceptionMovesToInReview()
    {
        var item = StrictAccrualException.NewStrictAccrualException("code-951", "label-446");
        try
        {
            item.MoveTo(StrictAccrualExceptionStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAccrualExceptionStatus.InReview, "status");
    }

    public static void StrictAccrualExceptionRejectsMovesFromArchived()
    {
        var item = StrictAccrualException.NewStrictAccrualException("code-951", "label-446");
        item.Status = StrictAccrualExceptionStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictAccrualExceptionStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictAccrualExceptionStartsInOpen();
        StrictAccrualExceptionMovesToInReview();
        StrictAccrualExceptionRejectsMovesFromArchived();
    }
}

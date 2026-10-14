namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AccrualExceptionChecks
{
    public static void AccrualExceptionStartsInPlanned()
    {
        var item = AccrualException.NewAccrualException("code-725", "name-291");
        Check.Equal(item.Status, AccrualExceptionStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualExceptionMovesToCompleted()
    {
        var item = AccrualException.NewAccrualException("code-725", "name-291");
        try
        {
            item.MoveTo(AccrualExceptionStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualExceptionStatus.Completed, "status");
    }

    public static void AccrualExceptionRejectsMovesFromCancelled()
    {
        var item = AccrualException.NewAccrualException("code-725", "name-291");
        item.Status = AccrualExceptionStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AccrualExceptionStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AccrualExceptionStartsInPlanned();
        AccrualExceptionMovesToCompleted();
        AccrualExceptionRejectsMovesFromCancelled();
    }
}

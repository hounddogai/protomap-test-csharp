namespace Acme.Workforce.Payrollops.Tests.Deduction.Draft;

using Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PendingDeductionExceptionChecks
{
    public static void PendingDeductionExceptionStartsInPlanned()
    {
        var item = PendingDeductionException.NewPendingDeductionException("code-142", "label-947");
        Check.Equal(item.Status, PendingDeductionExceptionStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingDeductionExceptionMovesToCompleted()
    {
        var item = PendingDeductionException.NewPendingDeductionException("code-142", "label-947");
        try
        {
            item.MoveTo(PendingDeductionExceptionStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingDeductionExceptionStatus.Completed, "status");
    }

    public static void PendingDeductionExceptionRejectsMovesFromCancelled()
    {
        var item = PendingDeductionException.NewPendingDeductionException("code-142", "label-947");
        item.Status = PendingDeductionExceptionStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingDeductionExceptionStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingDeductionExceptionStartsInPlanned();
        PendingDeductionExceptionMovesToCompleted();
        PendingDeductionExceptionRejectsMovesFromCancelled();
    }
}

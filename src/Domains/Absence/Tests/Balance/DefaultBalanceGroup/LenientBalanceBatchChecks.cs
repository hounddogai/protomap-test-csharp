namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceBatchChecks
{
    public static void LenientBalanceBatchStartsInPlanned()
    {
        var item = LenientBalanceBatch.NewLenientBalanceBatch("code-703", "title-981");
        Check.Equal(item.Status, LenientBalanceBatchStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientBalanceBatchMovesToScheduled()
    {
        var item = LenientBalanceBatch.NewLenientBalanceBatch("code-703", "title-981");
        try
        {
            item.MoveTo(LenientBalanceBatchStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientBalanceBatchStatus.Scheduled, "status");
    }

    public static void LenientBalanceBatchRejectsMovesFromCancelled()
    {
        var item = LenientBalanceBatch.NewLenientBalanceBatch("code-703", "title-981");
        item.Status = LenientBalanceBatchStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientBalanceBatchStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientBalanceBatchStartsInPlanned();
        LenientBalanceBatchMovesToScheduled();
        LenientBalanceBatchRejectsMovesFromCancelled();
    }
}

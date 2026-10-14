namespace Acme.Workforce.Absence.Tests.Balance.Batch;

using Acme.Workforce.Absence.Balance.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BalanceBatchChecks
{
    public static void BalanceBatchStartsInDraft()
    {
        var item = BalanceBatch.NewBalanceBatch("code-748", "name-134");
        Check.Equal(item.Status, BalanceBatchStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceBatchMovesToActive()
    {
        var item = BalanceBatch.NewBalanceBatch("code-748", "name-134");
        try
        {
            item.MoveTo(BalanceBatchStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceBatchStatus.Active, "status");
    }

    public static void BalanceBatchRejectsMovesFromClosed()
    {
        var item = BalanceBatch.NewBalanceBatch("code-748", "name-134");
        item.Status = BalanceBatchStatus.Closed;
        Check.Fails(() => item.MoveTo(BalanceBatchStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BalanceBatchStartsInDraft();
        BalanceBatchMovesToActive();
        BalanceBatchRejectsMovesFromClosed();
    }
}

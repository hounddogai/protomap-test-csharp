namespace Acme.Workforce.Compensation.Tests.BonusPool.Exception;

using Acme.Workforce.Compensation.BonusPool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BonusPoolExceptionChecks
{
    public static void BonusPoolExceptionStartsInDraft()
    {
        var item = BonusPoolException.NewBonusPoolException("code-71", "label-635");
        Check.Equal(item.Status, BonusPoolExceptionStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolExceptionMovesToSuspended()
    {
        var item = BonusPoolException.NewBonusPoolException("code-71", "label-635");
        try
        {
            item.MoveTo(BonusPoolExceptionStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolExceptionStatus.Suspended, "status");
    }

    public static void BonusPoolExceptionRejectsMovesFromClosed()
    {
        var item = BonusPoolException.NewBonusPoolException("code-71", "label-635");
        item.Status = BonusPoolExceptionStatus.Closed;
        Check.Fails(() => item.MoveTo(BonusPoolExceptionStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BonusPoolExceptionStartsInDraft();
        BonusPoolExceptionMovesToSuspended();
        BonusPoolExceptionRejectsMovesFromClosed();
    }
}

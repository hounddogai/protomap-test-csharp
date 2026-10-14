namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientRetroPayExceptionChecks
{
    public static void LenientRetroPayExceptionStartsInProposed()
    {
        var item = LenientRetroPayException.NewLenientRetroPayException("code-997", "label-54");
        Check.Equal(item.Status, LenientRetroPayExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientRetroPayExceptionMovesToWithdrawn()
    {
        var item = LenientRetroPayException.NewLenientRetroPayException("code-997", "label-54");
        try
        {
            item.MoveTo(LenientRetroPayExceptionStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientRetroPayExceptionStatus.Withdrawn, "status");
    }

    public static void LenientRetroPayExceptionRejectsMovesFromWithdrawn()
    {
        var item = LenientRetroPayException.NewLenientRetroPayException("code-997", "label-54");
        item.Status = LenientRetroPayExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientRetroPayExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientRetroPayExceptionStartsInProposed();
        LenientRetroPayExceptionMovesToWithdrawn();
        LenientRetroPayExceptionRejectsMovesFromWithdrawn();
    }
}

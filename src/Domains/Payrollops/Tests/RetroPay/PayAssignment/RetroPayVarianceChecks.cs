namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class RetroPayVarianceChecks
{
    public static void RetroPayVarianceStartsInProposed()
    {
        var item = RetroPayVariance.NewRetroPayVariance("code-783", "name-309");
        Check.Equal(item.Status, RetroPayVarianceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayVarianceMovesToRejected()
    {
        var item = RetroPayVariance.NewRetroPayVariance("code-783", "name-309");
        try
        {
            item.MoveTo(RetroPayVarianceStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayVarianceStatus.Rejected, "status");
    }

    public static void RetroPayVarianceRejectsMovesFromWithdrawn()
    {
        var item = RetroPayVariance.NewRetroPayVariance("code-783", "name-309");
        item.Status = RetroPayVarianceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RetroPayVarianceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RetroPayVarianceStartsInProposed();
        RetroPayVarianceMovesToRejected();
        RetroPayVarianceRejectsMovesFromWithdrawn();
    }
}

namespace Acme.Workforce.Compensation.Tests.PayBand.Batch;

using Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PayBandWindowChecks
{
    public static void PayBandWindowStartsInProposed()
    {
        var item = PayBandWindow.NewPayBandWindow("code-186", "title-78");
        Check.Equal(item.Status, PayBandWindowStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandWindowMovesToApproved()
    {
        var item = PayBandWindow.NewPayBandWindow("code-186", "title-78");
        try
        {
            item.MoveTo(PayBandWindowStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandWindowStatus.Approved, "status");
    }

    public static void PayBandWindowRejectsMovesFromWithdrawn()
    {
        var item = PayBandWindow.NewPayBandWindow("code-186", "title-78");
        item.Status = PayBandWindowStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayBandWindowStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayBandWindowStartsInProposed();
        PayBandWindowMovesToApproved();
        PayBandWindowRejectsMovesFromWithdrawn();
    }
}

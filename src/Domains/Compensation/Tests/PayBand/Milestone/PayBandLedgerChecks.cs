namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PayBandLedgerChecks
{
    public static void PayBandLedgerStartsInDraft()
    {
        var item = PayBandLedger.NewPayBandLedger("code-189", "name-466");
        Check.Equal(item.Status, PayBandLedgerStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandLedgerMovesToPending()
    {
        var item = PayBandLedger.NewPayBandLedger("code-189", "name-466");
        try
        {
            item.MoveTo(PayBandLedgerStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandLedgerStatus.Pending, "status");
    }

    public static void PayBandLedgerRejectsMovesFromClosed()
    {
        var item = PayBandLedger.NewPayBandLedger("code-189", "name-466");
        item.Status = PayBandLedgerStatus.Closed;
        Check.Fails(() => item.MoveTo(PayBandLedgerStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PayBandLedgerStartsInDraft();
        PayBandLedgerMovesToPending();
        PayBandLedgerRejectsMovesFromClosed();
    }
}

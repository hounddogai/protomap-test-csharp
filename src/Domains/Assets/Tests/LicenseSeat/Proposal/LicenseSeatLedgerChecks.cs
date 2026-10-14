namespace Acme.Workforce.Assets.Tests.LicenseSeat.Proposal;

using Acme.Workforce.Assets.LicenseSeat.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LicenseSeatLedgerChecks
{
    public static void LicenseSeatLedgerStartsInOpen()
    {
        var item = LicenseSeatLedger.NewLicenseSeatLedger("code-34", "label-399");
        Check.Equal(item.Status, LicenseSeatLedgerStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatLedgerMovesToInReview()
    {
        var item = LicenseSeatLedger.NewLicenseSeatLedger("code-34", "label-399");
        try
        {
            item.MoveTo(LicenseSeatLedgerStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatLedgerStatus.InReview, "status");
    }

    public static void LicenseSeatLedgerRejectsMovesFromArchived()
    {
        var item = LicenseSeatLedger.NewLicenseSeatLedger("code-34", "label-399");
        item.Status = LicenseSeatLedgerStatus.Archived;
        Check.Fails(() => item.MoveTo(LicenseSeatLedgerStatus.Open), "move");
    }

    public static void RunAll()
    {
        LicenseSeatLedgerStartsInOpen();
        LicenseSeatLedgerMovesToInReview();
        LicenseSeatLedgerRejectsMovesFromArchived();
    }
}

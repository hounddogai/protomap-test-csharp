namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class FallbackLicenseSeatLedgerChecks
{
    public static void FallbackLicenseSeatLedgerStartsInCreated()
    {
        var item = FallbackLicenseSeatLedger.NewFallbackLicenseSeatLedger("code-28", "title-103");
        Check.Equal(item.Status, FallbackLicenseSeatLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackLicenseSeatLedgerMovesToPublished()
    {
        var item = FallbackLicenseSeatLedger.NewFallbackLicenseSeatLedger("code-28", "title-103");
        try
        {
            item.MoveTo(FallbackLicenseSeatLedgerStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackLicenseSeatLedgerStatus.Published, "status");
    }

    public static void FallbackLicenseSeatLedgerRejectsMovesFromRetired()
    {
        var item = FallbackLicenseSeatLedger.NewFallbackLicenseSeatLedger("code-28", "title-103");
        item.Status = FallbackLicenseSeatLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackLicenseSeatLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatLedgerStartsInCreated();
        FallbackLicenseSeatLedgerMovesToPublished();
        FallbackLicenseSeatLedgerRejectsMovesFromRetired();
    }
}

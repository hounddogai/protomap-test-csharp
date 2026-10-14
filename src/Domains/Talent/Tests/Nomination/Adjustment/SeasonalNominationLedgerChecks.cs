namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalNominationLedgerChecks
{
    public static void SeasonalNominationLedgerStartsInDraft()
    {
        var item = SeasonalNominationLedger.NewSeasonalNominationLedger("code-946", "label-251");
        Check.Equal(item.Status, SeasonalNominationLedgerStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalNominationLedgerMovesToPending()
    {
        var item = SeasonalNominationLedger.NewSeasonalNominationLedger("code-946", "label-251");
        try
        {
            item.MoveTo(SeasonalNominationLedgerStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalNominationLedgerStatus.Pending, "status");
    }

    public static void SeasonalNominationLedgerRejectsMovesFromClosed()
    {
        var item = SeasonalNominationLedger.NewSeasonalNominationLedger("code-946", "label-251");
        item.Status = SeasonalNominationLedgerStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalNominationLedgerStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalNominationLedgerStartsInDraft();
        SeasonalNominationLedgerMovesToPending();
        SeasonalNominationLedgerRejectsMovesFromClosed();
    }
}

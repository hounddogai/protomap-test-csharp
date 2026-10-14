namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PrimaryHeadcountLedgerChecks
{
    public static void PrimaryHeadcountLedgerStartsInOpen()
    {
        var item = PrimaryHeadcountLedger.NewPrimaryHeadcountLedger("code-124", "title-19");
        Check.Equal(item.Status, PrimaryHeadcountLedgerStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryHeadcountLedgerMovesToInReview()
    {
        var item = PrimaryHeadcountLedger.NewPrimaryHeadcountLedger("code-124", "title-19");
        try
        {
            item.MoveTo(PrimaryHeadcountLedgerStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryHeadcountLedgerStatus.InReview, "status");
    }

    public static void PrimaryHeadcountLedgerRejectsMovesFromArchived()
    {
        var item = PrimaryHeadcountLedger.NewPrimaryHeadcountLedger("code-124", "title-19");
        item.Status = PrimaryHeadcountLedgerStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryHeadcountLedgerStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryHeadcountLedgerStartsInOpen();
        PrimaryHeadcountLedgerMovesToInReview();
        PrimaryHeadcountLedgerRejectsMovesFromArchived();
    }
}

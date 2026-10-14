namespace Acme.Workforce.Absence.Tests.Holiday.Proposal;

using Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HolidayLedgerChecks
{
    public static void HolidayLedgerStartsInOpen()
    {
        var item = HolidayLedger.NewHolidayLedger("code-118", "label-880");
        Check.Equal(item.Status, HolidayLedgerStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayLedgerMovesToResolved()
    {
        var item = HolidayLedger.NewHolidayLedger("code-118", "label-880");
        try
        {
            item.MoveTo(HolidayLedgerStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayLedgerStatus.Resolved, "status");
    }

    public static void HolidayLedgerRejectsMovesFromArchived()
    {
        var item = HolidayLedger.NewHolidayLedger("code-118", "label-880");
        item.Status = HolidayLedgerStatus.Archived;
        Check.Fails(() => item.MoveTo(HolidayLedgerStatus.Open), "move");
    }

    public static void RunAll()
    {
        HolidayLedgerStartsInOpen();
        HolidayLedgerMovesToResolved();
        HolidayLedgerRejectsMovesFromArchived();
    }
}

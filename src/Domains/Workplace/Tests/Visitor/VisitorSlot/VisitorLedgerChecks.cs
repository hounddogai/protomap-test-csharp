namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class VisitorLedgerChecks
{
    public static void VisitorLedgerStartsInCreated()
    {
        var item = VisitorLedger.NewVisitorLedger("code-295", "title-921");
        Check.Equal(item.Status, VisitorLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorLedgerMovesToRetired()
    {
        var item = VisitorLedger.NewVisitorLedger("code-295", "title-921");
        try
        {
            item.MoveTo(VisitorLedgerStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorLedgerStatus.Retired, "status");
    }

    public static void VisitorLedgerRejectsMovesFromRetired()
    {
        var item = VisitorLedger.NewVisitorLedger("code-295", "title-921");
        item.Status = VisitorLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(VisitorLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        VisitorLedgerStartsInCreated();
        VisitorLedgerMovesToRetired();
        VisitorLedgerRejectsMovesFromRetired();
    }
}

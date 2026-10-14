namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Ledger;

using Acme.Workforce.Contractors.StatementOfWork.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class StatementOfWorkEntryChecks
{
    public static void StatementOfWorkEntryStartsInCreated()
    {
        var item = StatementOfWorkEntry.NewStatementOfWorkEntry("code-85", "label-874");
        Check.Equal(item.Status, StatementOfWorkEntryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkEntryMovesToPublished()
    {
        var item = StatementOfWorkEntry.NewStatementOfWorkEntry("code-85", "label-874");
        try
        {
            item.MoveTo(StatementOfWorkEntryStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkEntryStatus.Published, "status");
    }

    public static void StatementOfWorkEntryRejectsMovesFromRetired()
    {
        var item = StatementOfWorkEntry.NewStatementOfWorkEntry("code-85", "label-874");
        item.Status = StatementOfWorkEntryStatus.Retired;
        Check.Fails(() => item.MoveTo(StatementOfWorkEntryStatus.Created), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkEntryStartsInCreated();
        StatementOfWorkEntryMovesToPublished();
        StatementOfWorkEntryRejectsMovesFromRetired();
    }
}

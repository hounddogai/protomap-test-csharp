namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Group;

using Acme.Workforce.Contractors.StatementOfWork.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class StatementOfWorkGroupChecks
{
    public static void StatementOfWorkGroupStartsInDraft()
    {
        var item = StatementOfWorkGroup.NewStatementOfWorkGroup("code-226", "label-243");
        Check.Equal(item.Status, StatementOfWorkGroupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkGroupMovesToActive()
    {
        var item = StatementOfWorkGroup.NewStatementOfWorkGroup("code-226", "label-243");
        try
        {
            item.MoveTo(StatementOfWorkGroupStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkGroupStatus.Active, "status");
    }

    public static void StatementOfWorkGroupRejectsMovesFromClosed()
    {
        var item = StatementOfWorkGroup.NewStatementOfWorkGroup("code-226", "label-243");
        item.Status = StatementOfWorkGroupStatus.Closed;
        Check.Fails(() => item.MoveTo(StatementOfWorkGroupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkGroupStartsInDraft();
        StatementOfWorkGroupMovesToActive();
        StatementOfWorkGroupRejectsMovesFromClosed();
    }
}

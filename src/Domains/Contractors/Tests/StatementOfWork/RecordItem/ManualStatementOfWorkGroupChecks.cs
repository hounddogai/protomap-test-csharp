namespace Acme.Workforce.Contractors.Tests.StatementOfWork.RecordItem;

using Acme.Workforce.Contractors.StatementOfWork.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ManualStatementOfWorkGroupChecks
{
    public static void ManualStatementOfWorkGroupStartsInPlanned()
    {
        var item = ManualStatementOfWorkGroup.NewManualStatementOfWorkGroup("code-453", "label-552");
        Check.Equal(item.Status, ManualStatementOfWorkGroupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualStatementOfWorkGroupMovesToCompleted()
    {
        var item = ManualStatementOfWorkGroup.NewManualStatementOfWorkGroup("code-453", "label-552");
        try
        {
            item.MoveTo(ManualStatementOfWorkGroupStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualStatementOfWorkGroupStatus.Completed, "status");
    }

    public static void ManualStatementOfWorkGroupRejectsMovesFromCancelled()
    {
        var item = ManualStatementOfWorkGroup.NewManualStatementOfWorkGroup("code-453", "label-552");
        item.Status = ManualStatementOfWorkGroupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualStatementOfWorkGroupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualStatementOfWorkGroupStartsInPlanned();
        ManualStatementOfWorkGroupMovesToCompleted();
        ManualStatementOfWorkGroupRejectsMovesFromCancelled();
    }
}

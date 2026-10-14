namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StatementOfWorkRequestChecks
{
    public static void StatementOfWorkRequestStartsInPlanned()
    {
        var item = StatementOfWorkRequest.NewStatementOfWorkRequest("code-193", "title-35");
        Check.Equal(item.Status, StatementOfWorkRequestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkRequestMovesToInProgress()
    {
        var item = StatementOfWorkRequest.NewStatementOfWorkRequest("code-193", "title-35");
        try
        {
            item.MoveTo(StatementOfWorkRequestStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkRequestStatus.InProgress, "status");
    }

    public static void StatementOfWorkRequestRejectsMovesFromCancelled()
    {
        var item = StatementOfWorkRequest.NewStatementOfWorkRequest("code-193", "title-35");
        item.Status = StatementOfWorkRequestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StatementOfWorkRequestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkRequestStartsInPlanned();
        StatementOfWorkRequestMovesToInProgress();
        StatementOfWorkRequestRejectsMovesFromCancelled();
    }
}

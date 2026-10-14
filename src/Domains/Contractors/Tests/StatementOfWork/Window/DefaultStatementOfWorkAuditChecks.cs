namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkAuditChecks
{
    public static void DefaultStatementOfWorkAuditStartsInNew()
    {
        var item = DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit("code-643", "name-387");
        Check.Equal(item.Status, DefaultStatementOfWorkAuditStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultStatementOfWorkAuditMovesToRunning()
    {
        var item = DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit("code-643", "name-387");
        try
        {
            item.MoveTo(DefaultStatementOfWorkAuditStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultStatementOfWorkAuditStatus.Running, "status");
    }

    public static void DefaultStatementOfWorkAuditRejectsMovesFromFailed()
    {
        var item = DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit("code-643", "name-387");
        item.Status = DefaultStatementOfWorkAuditStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultStatementOfWorkAuditStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultStatementOfWorkAuditStartsInNew();
        DefaultStatementOfWorkAuditMovesToRunning();
        DefaultStatementOfWorkAuditRejectsMovesFromFailed();
    }
}

namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkAuditChecks
{
    public static void StatementOfWorkAuditStartsInIdle()
    {
        var item = StatementOfWorkAudit.NewStatementOfWorkAudit("code-506", "name-442");
        Check.Equal(item.Status, StatementOfWorkAuditStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkAuditMovesToWarming()
    {
        var item = StatementOfWorkAudit.NewStatementOfWorkAudit("code-506", "name-442");
        try
        {
            item.MoveTo(StatementOfWorkAuditStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkAuditStatus.Warming, "status");
    }

    public static void StatementOfWorkAuditRejectsMovesFromStopped()
    {
        var item = StatementOfWorkAudit.NewStatementOfWorkAudit("code-506", "name-442");
        item.Status = StatementOfWorkAuditStatus.Stopped;
        Check.Fails(() => item.MoveTo(StatementOfWorkAuditStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkAuditStartsInIdle();
        StatementOfWorkAuditMovesToWarming();
        StatementOfWorkAuditRejectsMovesFromStopped();
    }
}

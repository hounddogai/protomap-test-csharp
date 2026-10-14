namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AdaptiveStatementOfWorkAuditChecks
{
    public static void AdaptiveStatementOfWorkAuditStartsInProposed()
    {
        var item = AdaptiveStatementOfWorkAudit.NewAdaptiveStatementOfWorkAudit("code-259", "label-775");
        Check.Equal(item.Status, AdaptiveStatementOfWorkAuditStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveStatementOfWorkAuditMovesToApproved()
    {
        var item = AdaptiveStatementOfWorkAudit.NewAdaptiveStatementOfWorkAudit("code-259", "label-775");
        try
        {
            item.MoveTo(AdaptiveStatementOfWorkAuditStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveStatementOfWorkAuditStatus.Approved, "status");
    }

    public static void AdaptiveStatementOfWorkAuditRejectsMovesFromWithdrawn()
    {
        var item = AdaptiveStatementOfWorkAudit.NewAdaptiveStatementOfWorkAudit("code-259", "label-775");
        item.Status = AdaptiveStatementOfWorkAuditStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdaptiveStatementOfWorkAuditStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdaptiveStatementOfWorkAuditStartsInProposed();
        AdaptiveStatementOfWorkAuditMovesToApproved();
        AdaptiveStatementOfWorkAuditRejectsMovesFromWithdrawn();
    }
}

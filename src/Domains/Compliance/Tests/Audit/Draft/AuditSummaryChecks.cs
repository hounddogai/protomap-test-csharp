namespace Acme.Workforce.Compliance.Tests.Audit.Draft;

using Acme.Workforce.Compliance.Audit.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AuditSummaryChecks
{
    public static void AuditSummaryStartsInProposed()
    {
        var item = AuditSummary.NewAuditSummary("code-708", "label-861");
        Check.Equal(item.Status, AuditSummaryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditSummaryMovesToApproved()
    {
        var item = AuditSummary.NewAuditSummary("code-708", "label-861");
        try
        {
            item.MoveTo(AuditSummaryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditSummaryStatus.Approved, "status");
    }

    public static void AuditSummaryRejectsMovesFromWithdrawn()
    {
        var item = AuditSummary.NewAuditSummary("code-708", "label-861");
        item.Status = AuditSummaryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AuditSummaryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AuditSummaryStartsInProposed();
        AuditSummaryMovesToApproved();
        AuditSummaryRejectsMovesFromWithdrawn();
    }
}

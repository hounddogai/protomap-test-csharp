namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AuditRollupChecks
{
    public static void AuditRollupStartsInPlanned()
    {
        var item = AuditRollup.NewAuditRollup("code-680", "title-837");
        Check.Equal(item.Status, AuditRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditRollupMovesToCompleted()
    {
        var item = AuditRollup.NewAuditRollup("code-680", "title-837");
        try
        {
            item.MoveTo(AuditRollupStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditRollupStatus.Completed, "status");
    }

    public static void AuditRollupRejectsMovesFromCancelled()
    {
        var item = AuditRollup.NewAuditRollup("code-680", "title-837");
        item.Status = AuditRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AuditRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AuditRollupStartsInPlanned();
        AuditRollupMovesToCompleted();
        AuditRollupRejectsMovesFromCancelled();
    }
}

namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditDigestChecks
{
    public static void AuditDigestStartsInPlanned()
    {
        var item = AuditDigest.NewAuditDigest("code-324", "name-994");
        Check.Equal(item.Status, AuditDigestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditDigestMovesToInProgress()
    {
        var item = AuditDigest.NewAuditDigest("code-324", "name-994");
        try
        {
            item.MoveTo(AuditDigestStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditDigestStatus.InProgress, "status");
    }

    public static void AuditDigestRejectsMovesFromCancelled()
    {
        var item = AuditDigest.NewAuditDigest("code-324", "name-994");
        item.Status = AuditDigestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AuditDigestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AuditDigestStartsInPlanned();
        AuditDigestMovesToInProgress();
        AuditDigestRejectsMovesFromCancelled();
    }
}

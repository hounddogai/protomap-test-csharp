namespace Acme.Workforce.Compliance.Tests.Audit.AuditDraft;

using Acme.Workforce.Compliance.Audit.AuditDraft;

/// <summary>Checks of audit draft code, run by the project's runner.</summary>
public static class AuditSegmentChecks
{
    public static void AuditSegmentStartsInDraft()
    {
        var item = AuditSegment.NewAuditSegment("code-346", "title-113");
        Check.Equal(item.Status, AuditSegmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditSegmentMovesToPending()
    {
        var item = AuditSegment.NewAuditSegment("code-346", "title-113");
        try
        {
            item.MoveTo(AuditSegmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditSegmentStatus.Pending, "status");
    }

    public static void AuditSegmentRejectsMovesFromClosed()
    {
        var item = AuditSegment.NewAuditSegment("code-346", "title-113");
        item.Status = AuditSegmentStatus.Closed;
        Check.Fails(() => item.MoveTo(AuditSegmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AuditSegmentStartsInDraft();
        AuditSegmentMovesToPending();
        AuditSegmentRejectsMovesFromClosed();
    }
}

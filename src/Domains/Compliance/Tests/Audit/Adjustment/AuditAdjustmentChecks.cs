namespace Acme.Workforce.Compliance.Tests.Audit.Adjustment;

using Acme.Workforce.Compliance.Audit.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AuditAdjustmentChecks
{
    public static void AuditAdjustmentStartsInOpen()
    {
        var item = AuditAdjustment.NewAuditAdjustment("code-109", "title-26");
        Check.Equal(item.Status, AuditAdjustmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditAdjustmentMovesToInReview()
    {
        var item = AuditAdjustment.NewAuditAdjustment("code-109", "title-26");
        try
        {
            item.MoveTo(AuditAdjustmentStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditAdjustmentStatus.InReview, "status");
    }

    public static void AuditAdjustmentRejectsMovesFromArchived()
    {
        var item = AuditAdjustment.NewAuditAdjustment("code-109", "title-26");
        item.Status = AuditAdjustmentStatus.Archived;
        Check.Fails(() => item.MoveTo(AuditAdjustmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        AuditAdjustmentStartsInOpen();
        AuditAdjustmentMovesToInReview();
        AuditAdjustmentRejectsMovesFromArchived();
    }
}

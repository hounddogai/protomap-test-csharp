namespace Acme.Workforce.Compliance.Tests.Audit.AuditRecord;

using Acme.Workforce.Compliance.Audit.AuditRecord;

/// <summary>Checks of audit record code, run by the project's runner.</summary>
public static class StrictAuditRecordChecks
{
    public static void StrictAuditRecordStartsInOpen()
    {
        var item = StrictAuditRecord.NewStrictAuditRecord("code-591", "title-863");
        Check.Equal(item.Status, StrictAuditRecordStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAuditRecordMovesToInReview()
    {
        var item = StrictAuditRecord.NewStrictAuditRecord("code-591", "title-863");
        try
        {
            item.MoveTo(StrictAuditRecordStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAuditRecordStatus.InReview, "status");
    }

    public static void StrictAuditRecordRejectsMovesFromArchived()
    {
        var item = StrictAuditRecord.NewStrictAuditRecord("code-591", "title-863");
        item.Status = StrictAuditRecordStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictAuditRecordStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictAuditRecordStartsInOpen();
        StrictAuditRecordMovesToInReview();
        StrictAuditRecordRejectsMovesFromArchived();
    }
}

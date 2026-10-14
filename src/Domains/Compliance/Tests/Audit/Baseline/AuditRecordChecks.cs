namespace Acme.Workforce.Compliance.Tests.Audit.Baseline;

using Acme.Workforce.Compliance.Audit.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AuditRecordChecks
{
    public static void AuditRecordStartsInCreated()
    {
        var item = AuditRecord.NewAuditRecord("code-882", "label-806");
        Check.Equal(item.Status, AuditRecordStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditRecordMovesToValidated()
    {
        var item = AuditRecord.NewAuditRecord("code-882", "label-806");
        try
        {
            item.MoveTo(AuditRecordStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditRecordStatus.Validated, "status");
    }

    public static void AuditRecordRejectsMovesFromRetired()
    {
        var item = AuditRecord.NewAuditRecord("code-882", "label-806");
        item.Status = AuditRecordStatus.Retired;
        Check.Fails(() => item.MoveTo(AuditRecordStatus.Created), "move");
    }

    public static void RunAll()
    {
        AuditRecordStartsInCreated();
        AuditRecordMovesToValidated();
        AuditRecordRejectsMovesFromRetired();
    }
}

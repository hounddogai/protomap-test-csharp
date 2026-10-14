namespace Acme.Workforce.Compliance.Tests.Audit.Baseline;

using Acme.Workforce.Compliance.Audit.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AuditBaselineChecks
{
    public static void AuditBaselineStartsInCreated()
    {
        var item = AuditBaseline.NewAuditBaseline("code-367", "name-68");
        Check.Equal(item.Status, AuditBaselineStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditBaselineMovesToValidated()
    {
        var item = AuditBaseline.NewAuditBaseline("code-367", "name-68");
        try
        {
            item.MoveTo(AuditBaselineStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditBaselineStatus.Validated, "status");
    }

    public static void AuditBaselineRejectsMovesFromRetired()
    {
        var item = AuditBaseline.NewAuditBaseline("code-367", "name-68");
        item.Status = AuditBaselineStatus.Retired;
        Check.Fails(() => item.MoveTo(AuditBaselineStatus.Created), "move");
    }

    public static void RunAll()
    {
        AuditBaselineStartsInCreated();
        AuditBaselineMovesToValidated();
        AuditBaselineRejectsMovesFromRetired();
    }
}

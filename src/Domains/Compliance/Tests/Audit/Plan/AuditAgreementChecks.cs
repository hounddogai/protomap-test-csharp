namespace Acme.Workforce.Compliance.Tests.Audit.Plan;

using Acme.Workforce.Compliance.Audit.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class AuditAgreementChecks
{
    public static void AuditAgreementStartsInProposed()
    {
        var item = AuditAgreement.NewAuditAgreement("code-139", "name-677");
        Check.Equal(item.Status, AuditAgreementStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditAgreementMovesToApproved()
    {
        var item = AuditAgreement.NewAuditAgreement("code-139", "name-677");
        try
        {
            item.MoveTo(AuditAgreementStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditAgreementStatus.Approved, "status");
    }

    public static void AuditAgreementRejectsMovesFromWithdrawn()
    {
        var item = AuditAgreement.NewAuditAgreement("code-139", "name-677");
        item.Status = AuditAgreementStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AuditAgreementStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AuditAgreementStartsInProposed();
        AuditAgreementMovesToApproved();
        AuditAgreementRejectsMovesFromWithdrawn();
    }
}

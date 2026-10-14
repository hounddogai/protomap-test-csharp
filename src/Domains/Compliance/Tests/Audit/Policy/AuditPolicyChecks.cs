namespace Acme.Workforce.Compliance.Tests.Audit.Policy;

using Acme.Workforce.Compliance.Audit.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AuditPolicyChecks
{
    public static void AuditPolicyStartsInNew()
    {
        var item = AuditPolicy.NewAuditPolicy("code-190", "name-644");
        Check.Equal(item.Status, AuditPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditPolicyMovesToQueued()
    {
        var item = AuditPolicy.NewAuditPolicy("code-190", "name-644");
        try
        {
            item.MoveTo(AuditPolicyStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditPolicyStatus.Queued, "status");
    }

    public static void AuditPolicyRejectsMovesFromFailed()
    {
        var item = AuditPolicy.NewAuditPolicy("code-190", "name-644");
        item.Status = AuditPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(AuditPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        AuditPolicyStartsInNew();
        AuditPolicyMovesToQueued();
        AuditPolicyRejectsMovesFromFailed();
    }
}

namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AuditProfileChecks
{
    public static void AuditProfileStartsInNew()
    {
        var item = AuditProfile.NewAuditProfile("code-735", "label-129");
        Check.Equal(item.Status, AuditProfileStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditProfileMovesToDone()
    {
        var item = AuditProfile.NewAuditProfile("code-735", "label-129");
        try
        {
            item.MoveTo(AuditProfileStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditProfileStatus.Done, "status");
    }

    public static void AuditProfileRejectsMovesFromFailed()
    {
        var item = AuditProfile.NewAuditProfile("code-735", "label-129");
        item.Status = AuditProfileStatus.Failed;
        Check.Fails(() => item.MoveTo(AuditProfileStatus.New), "move");
    }

    public static void RunAll()
    {
        AuditProfileStartsInNew();
        AuditProfileMovesToDone();
        AuditProfileRejectsMovesFromFailed();
    }
}

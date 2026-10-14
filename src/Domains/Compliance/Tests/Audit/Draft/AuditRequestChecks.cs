namespace Acme.Workforce.Compliance.Tests.Audit.Draft;

using Acme.Workforce.Compliance.Audit.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AuditRequestChecks
{
    public static void AuditRequestStartsInDraft()
    {
        var item = AuditRequest.NewAuditRequest("code-870", "label-798");
        Check.Equal(item.Status, AuditRequestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditRequestMovesToClosed()
    {
        var item = AuditRequest.NewAuditRequest("code-870", "label-798");
        try
        {
            item.MoveTo(AuditRequestStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditRequestStatus.Closed, "status");
    }

    public static void AuditRequestRejectsMovesFromClosed()
    {
        var item = AuditRequest.NewAuditRequest("code-870", "label-798");
        item.Status = AuditRequestStatus.Closed;
        Check.Fails(() => item.MoveTo(AuditRequestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AuditRequestStartsInDraft();
        AuditRequestMovesToClosed();
        AuditRequestRejectsMovesFromClosed();
    }
}

namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayAuditChecks
{
    public static void RetroPayAuditStartsInDraft()
    {
        var item = RetroPayAudit.NewRetroPayAudit("code-500", "name-39");
        Check.Equal(item.Status, RetroPayAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayAuditMovesToActive()
    {
        var item = RetroPayAudit.NewRetroPayAudit("code-500", "name-39");
        try
        {
            item.MoveTo(RetroPayAuditStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayAuditStatus.Active, "status");
    }

    public static void RetroPayAuditRejectsMovesFromClosed()
    {
        var item = RetroPayAudit.NewRetroPayAudit("code-500", "name-39");
        item.Status = RetroPayAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(RetroPayAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RetroPayAuditStartsInDraft();
        RetroPayAuditMovesToActive();
        RetroPayAuditRejectsMovesFromClosed();
    }
}

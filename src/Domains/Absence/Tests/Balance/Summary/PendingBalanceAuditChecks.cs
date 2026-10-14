namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingBalanceAuditChecks
{
    public static void PendingBalanceAuditStartsInCreated()
    {
        var item = PendingBalanceAudit.NewPendingBalanceAudit("code-329", "label-32");
        Check.Equal(item.Status, PendingBalanceAuditStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingBalanceAuditMovesToValidated()
    {
        var item = PendingBalanceAudit.NewPendingBalanceAudit("code-329", "label-32");
        try
        {
            item.MoveTo(PendingBalanceAuditStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingBalanceAuditStatus.Validated, "status");
    }

    public static void PendingBalanceAuditRejectsMovesFromRetired()
    {
        var item = PendingBalanceAudit.NewPendingBalanceAudit("code-329", "label-32");
        item.Status = PendingBalanceAuditStatus.Retired;
        Check.Fails(() => item.MoveTo(PendingBalanceAuditStatus.Created), "move");
    }

    public static void RunAll()
    {
        PendingBalanceAuditStartsInCreated();
        PendingBalanceAuditMovesToValidated();
        PendingBalanceAuditRejectsMovesFromRetired();
    }
}

namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BalanceAuditChecks
{
    public static void BalanceAuditStartsInNew()
    {
        var item = BalanceAudit.NewBalanceAudit("code-696", "title-336");
        Check.Equal(item.Status, BalanceAuditStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceAuditMovesToQueued()
    {
        var item = BalanceAudit.NewBalanceAudit("code-696", "title-336");
        try
        {
            item.MoveTo(BalanceAuditStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceAuditStatus.Queued, "status");
    }

    public static void BalanceAuditRejectsMovesFromFailed()
    {
        var item = BalanceAudit.NewBalanceAudit("code-696", "title-336");
        item.Status = BalanceAuditStatus.Failed;
        Check.Fails(() => item.MoveTo(BalanceAuditStatus.New), "move");
    }

    public static void RunAll()
    {
        BalanceAuditStartsInNew();
        BalanceAuditMovesToQueued();
        BalanceAuditRejectsMovesFromFailed();
    }
}

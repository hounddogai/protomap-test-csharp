namespace Acme.Workforce.Absence.Tests.Balance.Quota;

using Acme.Workforce.Absence.Balance.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RollingBalanceQuotaChecks
{
    public static void RollingBalanceQuotaStartsInProposed()
    {
        var item = RollingBalanceQuota.NewRollingBalanceQuota("code-773", "title-724");
        Check.Equal(item.Status, RollingBalanceQuotaStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingBalanceQuotaMovesToWithdrawn()
    {
        var item = RollingBalanceQuota.NewRollingBalanceQuota("code-773", "title-724");
        try
        {
            item.MoveTo(RollingBalanceQuotaStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingBalanceQuotaStatus.Withdrawn, "status");
    }

    public static void RollingBalanceQuotaRejectsMovesFromWithdrawn()
    {
        var item = RollingBalanceQuota.NewRollingBalanceQuota("code-773", "title-724");
        item.Status = RollingBalanceQuotaStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RollingBalanceQuotaStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RollingBalanceQuotaStartsInProposed();
        RollingBalanceQuotaMovesToWithdrawn();
        RollingBalanceQuotaRejectsMovesFromWithdrawn();
    }
}

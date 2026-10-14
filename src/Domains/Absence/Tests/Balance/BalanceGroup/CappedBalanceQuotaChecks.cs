namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class CappedBalanceQuotaChecks
{
    public static void CappedBalanceQuotaStartsInCreated()
    {
        var item = CappedBalanceQuota.NewCappedBalanceQuota("code-754", "label-896");
        Check.Equal(item.Status, CappedBalanceQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedBalanceQuotaMovesToValidated()
    {
        var item = CappedBalanceQuota.NewCappedBalanceQuota("code-754", "label-896");
        try
        {
            item.MoveTo(CappedBalanceQuotaStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedBalanceQuotaStatus.Validated, "status");
    }

    public static void CappedBalanceQuotaRejectsMovesFromRetired()
    {
        var item = CappedBalanceQuota.NewCappedBalanceQuota("code-754", "label-896");
        item.Status = CappedBalanceQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(CappedBalanceQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        CappedBalanceQuotaStartsInCreated();
        CappedBalanceQuotaMovesToValidated();
        CappedBalanceQuotaRejectsMovesFromRetired();
    }
}

namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BalanceQuotaChecks
{
    public static void BalanceQuotaStartsInCreated()
    {
        var item = BalanceQuota.NewBalanceQuota("code-585", "label-558");
        Check.Equal(item.Status, BalanceQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceQuotaMovesToValidated()
    {
        var item = BalanceQuota.NewBalanceQuota("code-585", "label-558");
        try
        {
            item.MoveTo(BalanceQuotaStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceQuotaStatus.Validated, "status");
    }

    public static void BalanceQuotaRejectsMovesFromRetired()
    {
        var item = BalanceQuota.NewBalanceQuota("code-585", "label-558");
        item.Status = BalanceQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(BalanceQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        BalanceQuotaStartsInCreated();
        BalanceQuotaMovesToValidated();
        BalanceQuotaRejectsMovesFromRetired();
    }
}

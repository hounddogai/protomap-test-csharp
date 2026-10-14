namespace Acme.Workforce.Absence.Tests.Accrual.Forecast;

using Acme.Workforce.Absence.Accrual.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class AccrualQuotaChecks
{
    public static void AccrualQuotaStartsInIdle()
    {
        var item = AccrualQuota.NewAccrualQuota("code-160", "label-547");
        Check.Equal(item.Status, AccrualQuotaStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualQuotaMovesToDraining()
    {
        var item = AccrualQuota.NewAccrualQuota("code-160", "label-547");
        try
        {
            item.MoveTo(AccrualQuotaStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualQuotaStatus.Draining, "status");
    }

    public static void AccrualQuotaRejectsMovesFromStopped()
    {
        var item = AccrualQuota.NewAccrualQuota("code-160", "label-547");
        item.Status = AccrualQuotaStatus.Stopped;
        Check.Fails(() => item.MoveTo(AccrualQuotaStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AccrualQuotaStartsInIdle();
        AccrualQuotaMovesToDraining();
        AccrualQuotaRejectsMovesFromStopped();
    }
}

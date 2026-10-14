namespace Acme.Workforce.Talent.Tests.Pool.Quota;

using Acme.Workforce.Talent.Pool.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PoolQuotaChecks
{
    public static void PoolQuotaStartsInIdle()
    {
        var item = PoolQuota.NewPoolQuota("code-125", "label-436");
        Check.Equal(item.Status, PoolQuotaStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolQuotaMovesToWarming()
    {
        var item = PoolQuota.NewPoolQuota("code-125", "label-436");
        try
        {
            item.MoveTo(PoolQuotaStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolQuotaStatus.Warming, "status");
    }

    public static void PoolQuotaRejectsMovesFromStopped()
    {
        var item = PoolQuota.NewPoolQuota("code-125", "label-436");
        item.Status = PoolQuotaStatus.Stopped;
        Check.Fails(() => item.MoveTo(PoolQuotaStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PoolQuotaStartsInIdle();
        PoolQuotaMovesToWarming();
        PoolQuotaRejectsMovesFromStopped();
    }
}

namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Quota;

using Acme.Workforce.Talent.SuccessionPlan.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class GlobalSuccessionPlanQuotaChecks
{
    public static void GlobalSuccessionPlanQuotaStartsInIdle()
    {
        var item = GlobalSuccessionPlanQuota.NewGlobalSuccessionPlanQuota("code-971", "label-320");
        Check.Equal(item.Status, GlobalSuccessionPlanQuotaStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalSuccessionPlanQuotaMovesToWarming()
    {
        var item = GlobalSuccessionPlanQuota.NewGlobalSuccessionPlanQuota("code-971", "label-320");
        try
        {
            item.MoveTo(GlobalSuccessionPlanQuotaStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalSuccessionPlanQuotaStatus.Warming, "status");
    }

    public static void GlobalSuccessionPlanQuotaRejectsMovesFromStopped()
    {
        var item = GlobalSuccessionPlanQuota.NewGlobalSuccessionPlanQuota("code-971", "label-320");
        item.Status = GlobalSuccessionPlanQuotaStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalSuccessionPlanQuotaStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalSuccessionPlanQuotaStartsInIdle();
        GlobalSuccessionPlanQuotaMovesToWarming();
        GlobalSuccessionPlanQuotaRejectsMovesFromStopped();
    }
}

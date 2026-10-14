namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RollingSuccessionPlanQuotaChecks
{
    public static void RollingSuccessionPlanQuotaStartsInDraft()
    {
        var item = RollingSuccessionPlanQuota.NewRollingSuccessionPlanQuota("code-174", "label-935");
        Check.Equal(item.Status, RollingSuccessionPlanQuotaStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingSuccessionPlanQuotaMovesToActive()
    {
        var item = RollingSuccessionPlanQuota.NewRollingSuccessionPlanQuota("code-174", "label-935");
        try
        {
            item.MoveTo(RollingSuccessionPlanQuotaStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingSuccessionPlanQuotaStatus.Active, "status");
    }

    public static void RollingSuccessionPlanQuotaRejectsMovesFromClosed()
    {
        var item = RollingSuccessionPlanQuota.NewRollingSuccessionPlanQuota("code-174", "label-935");
        item.Status = RollingSuccessionPlanQuotaStatus.Closed;
        Check.Fails(() => item.MoveTo(RollingSuccessionPlanQuotaStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RollingSuccessionPlanQuotaStartsInDraft();
        RollingSuccessionPlanQuotaMovesToActive();
        RollingSuccessionPlanQuotaRejectsMovesFromClosed();
    }
}

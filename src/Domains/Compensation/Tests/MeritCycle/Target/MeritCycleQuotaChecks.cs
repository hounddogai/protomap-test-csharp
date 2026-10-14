namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleQuotaChecks
{
    public static void MeritCycleQuotaStartsInDraft()
    {
        var item = MeritCycleQuota.NewMeritCycleQuota("code-296", "title-20");
        Check.Equal(item.Status, MeritCycleQuotaStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleQuotaMovesToActive()
    {
        var item = MeritCycleQuota.NewMeritCycleQuota("code-296", "title-20");
        try
        {
            item.MoveTo(MeritCycleQuotaStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleQuotaStatus.Active, "status");
    }

    public static void MeritCycleQuotaRejectsMovesFromClosed()
    {
        var item = MeritCycleQuota.NewMeritCycleQuota("code-296", "title-20");
        item.Status = MeritCycleQuotaStatus.Closed;
        Check.Fails(() => item.MoveTo(MeritCycleQuotaStatus.Draft), "move");
    }

    public static void RunAll()
    {
        MeritCycleQuotaStartsInDraft();
        MeritCycleQuotaMovesToActive();
        MeritCycleQuotaRejectsMovesFromClosed();
    }
}

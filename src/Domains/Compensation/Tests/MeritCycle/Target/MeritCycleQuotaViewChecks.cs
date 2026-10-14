namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleQuotaViewChecks
{
    public static void MeritCycleQuotaViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleQuota.NewMeritCycleQuota("code-296", "title-20");
        var view = MeritCycleQuotaView.ToMeritCycleQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleQuotaView.ToMeritCycleQuotaViews(new List<MeritCycleQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleQuotaViewShowsTheStatusInCapitals();
    }
}

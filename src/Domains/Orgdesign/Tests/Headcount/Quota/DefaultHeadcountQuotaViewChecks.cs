namespace Acme.Workforce.Orgdesign.Tests.Headcount.Quota;

using Acme.Workforce.Orgdesign.Headcount.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DefaultHeadcountQuotaViewChecks
{
    public static void DefaultHeadcountQuotaViewShowsTheStatusInCapitals()
    {
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("code-241", "name-616");
        var view = DefaultHeadcountQuotaView.ToDefaultHeadcountQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultHeadcountQuotaView.ToDefaultHeadcountQuotaViews(new List<DefaultHeadcountQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultHeadcountQuotaViewShowsTheStatusInCapitals();
    }
}

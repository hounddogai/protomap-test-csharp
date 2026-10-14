namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class RegionalCareerPathQuotaViewChecks
{
    public static void RegionalCareerPathQuotaViewShowsTheStatusInCapitals()
    {
        var item = RegionalCareerPathQuota.NewRegionalCareerPathQuota("code-925", "label-252");
        var view = RegionalCareerPathQuotaView.ToRegionalCareerPathQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalCareerPathQuotaView.ToRegionalCareerPathQuotaViews(new List<RegionalCareerPathQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalCareerPathQuotaViewShowsTheStatusInCapitals();
    }
}

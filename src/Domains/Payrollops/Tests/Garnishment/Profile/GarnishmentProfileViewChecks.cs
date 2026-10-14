namespace Acme.Workforce.Payrollops.Tests.Garnishment.Profile;

using Acme.Workforce.Payrollops.Garnishment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GarnishmentProfileViewChecks
{
    public static void GarnishmentProfileViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentProfile.NewGarnishmentProfile("code-423", "title-546");
        var view = GarnishmentProfileView.ToGarnishmentProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentProfileView.ToGarnishmentProfileViews(new List<GarnishmentProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentProfileViewShowsTheStatusInCapitals();
    }
}

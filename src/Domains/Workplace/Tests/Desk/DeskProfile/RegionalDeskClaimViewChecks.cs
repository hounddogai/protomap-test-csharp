namespace Acme.Workforce.Workplace.Tests.Desk.DeskProfile;

using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Checks of desk profile code, run by the project's runner.</summary>
public static class RegionalDeskClaimViewChecks
{
    public static void RegionalDeskClaimViewShowsTheStatusInCapitals()
    {
        var item = RegionalDeskClaim.NewRegionalDeskClaim("code-714", "name-85");
        var view = RegionalDeskClaimView.ToRegionalDeskClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalDeskClaimView.ToRegionalDeskClaimViews(new List<RegionalDeskClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalDeskClaimViewShowsTheStatusInCapitals();
    }
}

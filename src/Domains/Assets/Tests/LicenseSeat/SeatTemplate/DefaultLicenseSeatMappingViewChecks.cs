namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class DefaultLicenseSeatMappingViewChecks
{
    public static void DefaultLicenseSeatMappingViewShowsTheStatusInCapitals()
    {
        var item = DefaultLicenseSeatMapping.NewDefaultLicenseSeatMapping("code-911", "title-494");
        var view = DefaultLicenseSeatMappingView.ToDefaultLicenseSeatMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultLicenseSeatMappingView.ToDefaultLicenseSeatMappingViews(new List<DefaultLicenseSeatMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultLicenseSeatMappingViewShowsTheStatusInCapitals();
    }
}

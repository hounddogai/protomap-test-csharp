namespace Acme.Workforce.Assets.Tests.LicenseSeat.Baseline;

using Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PrimaryLicenseSeatWindowViewChecks
{
    public static void PrimaryLicenseSeatWindowViewShowsTheStatusInCapitals()
    {
        var item = PrimaryLicenseSeatWindow.NewPrimaryLicenseSeatWindow("code-812", "label-136");
        var view = PrimaryLicenseSeatWindowView.ToPrimaryLicenseSeatWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryLicenseSeatWindowView.ToPrimaryLicenseSeatWindowViews(new List<PrimaryLicenseSeatWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryLicenseSeatWindowViewShowsTheStatusInCapitals();
    }
}

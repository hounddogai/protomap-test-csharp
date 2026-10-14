namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StrictLicenseSeatWindowViewChecks
{
    public static void StrictLicenseSeatWindowViewShowsTheStatusInCapitals()
    {
        var item = StrictLicenseSeatWindow.NewStrictLicenseSeatWindow("code-751", "label-536");
        var view = StrictLicenseSeatWindowView.ToStrictLicenseSeatWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictLicenseSeatWindowView.ToStrictLicenseSeatWindowViews(new List<StrictLicenseSeatWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictLicenseSeatWindowViewShowsTheStatusInCapitals();
    }
}

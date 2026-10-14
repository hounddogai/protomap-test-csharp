namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LicenseSeatPreferenceViewChecks
{
    public static void LicenseSeatPreferenceViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatPreference.NewLicenseSeatPreference("code-938", "title-949");
        var view = LicenseSeatPreferenceView.ToLicenseSeatPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatPreferenceView.ToLicenseSeatPreferenceViews(new List<LicenseSeatPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatPreferenceViewShowsTheStatusInCapitals();
    }
}

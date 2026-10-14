namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatProfileViewChecks
{
    public static void LicenseSeatProfileViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatProfile.NewLicenseSeatProfile("code-979", "title-747");
        var view = LicenseSeatProfileView.ToLicenseSeatProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatProfileView.ToLicenseSeatProfileViews(new List<LicenseSeatProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatProfileViewShowsTheStatusInCapitals();
    }
}

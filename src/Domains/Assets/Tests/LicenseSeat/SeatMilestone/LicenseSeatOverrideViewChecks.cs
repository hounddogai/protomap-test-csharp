namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LicenseSeatOverrideViewChecks
{
    public static void LicenseSeatOverrideViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatOverride.NewLicenseSeatOverride("code-417", "label-382");
        var view = LicenseSeatOverrideView.ToLicenseSeatOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatOverrideView.ToLicenseSeatOverrideViews(new List<LicenseSeatOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatOverrideViewShowsTheStatusInCapitals();
    }
}

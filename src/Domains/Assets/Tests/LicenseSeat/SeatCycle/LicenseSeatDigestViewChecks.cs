namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatCycle;

using Acme.Workforce.Assets.LicenseSeat.SeatCycle;

/// <summary>Checks of seat cycle code, run by the project's runner.</summary>
public static class LicenseSeatDigestViewChecks
{
    public static void LicenseSeatDigestViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatDigest.NewLicenseSeatDigest("code-898", "name-414");
        var view = LicenseSeatDigestView.ToLicenseSeatDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatDigestView.ToLicenseSeatDigestViews(new List<LicenseSeatDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatDigestViewShowsTheStatusInCapitals();
    }
}

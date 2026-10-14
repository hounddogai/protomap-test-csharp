namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class NominationBundleViewChecks
{
    public static void NominationBundleViewShowsTheStatusInCapitals()
    {
        var item = NominationBundle.NewNominationBundle("code-261", "name-279");
        var view = NominationBundleView.ToNominationBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationBundleView.ToNominationBundleViews(new List<NominationBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationBundleViewShowsTheStatusInCapitals();
    }
}

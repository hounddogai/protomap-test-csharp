namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationProfileViewChecks
{
    public static void NominationProfileViewShowsTheStatusInCapitals()
    {
        var item = NominationProfile.NewNominationProfile("code-503", "title-872");
        var view = NominationProfileView.ToNominationProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationProfileView.ToNominationProfileViews(new List<NominationProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationProfileViewShowsTheStatusInCapitals();
    }
}

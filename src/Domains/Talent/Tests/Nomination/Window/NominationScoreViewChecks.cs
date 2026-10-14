namespace Acme.Workforce.Talent.Tests.Nomination.Window;

using Acme.Workforce.Talent.Nomination.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class NominationScoreViewChecks
{
    public static void NominationScoreViewShowsTheStatusInCapitals()
    {
        var item = NominationScore.NewNominationScore("code-546", "label-366");
        var view = NominationScoreView.ToNominationScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationScoreView.ToNominationScoreViews(new List<NominationScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationScoreViewShowsTheStatusInCapitals();
    }
}

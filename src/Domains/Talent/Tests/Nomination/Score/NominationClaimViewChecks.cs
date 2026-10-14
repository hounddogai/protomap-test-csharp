namespace Acme.Workforce.Talent.Tests.Nomination.Score;

using Acme.Workforce.Talent.Nomination.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class NominationClaimViewChecks
{
    public static void NominationClaimViewShowsTheStatusInCapitals()
    {
        var item = NominationClaim.NewNominationClaim("code-256", "title-307");
        var view = NominationClaimView.ToNominationClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationClaimView.ToNominationClaimViews(new List<NominationClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationClaimViewShowsTheStatusInCapitals();
    }
}

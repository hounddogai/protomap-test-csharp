namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class StrictNominationTierViewChecks
{
    public static void StrictNominationTierViewShowsTheStatusInCapitals()
    {
        var item = StrictNominationTier.NewStrictNominationTier("code-302", "title-961");
        var view = StrictNominationTierView.ToStrictNominationTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictNominationTierView.ToStrictNominationTierViews(new List<StrictNominationTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictNominationTierViewShowsTheStatusInCapitals();
    }
}

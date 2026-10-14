namespace Acme.Workforce.Talent.Tests.Nomination.Cycle;

using Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveNominationVarianceViewChecks
{
    public static void AdaptiveNominationVarianceViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveNominationVariance.NewAdaptiveNominationVariance("code-591", "label-574");
        var view = AdaptiveNominationVarianceView.ToAdaptiveNominationVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveNominationVarianceView.ToAdaptiveNominationVarianceViews(new List<AdaptiveNominationVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveNominationVarianceViewShowsTheStatusInCapitals();
    }
}

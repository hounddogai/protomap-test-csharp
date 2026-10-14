namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationVarianceViewChecks
{
    public static void NominationVarianceViewShowsTheStatusInCapitals()
    {
        var item = NominationVariance.NewNominationVariance("code-486", "label-583");
        var view = NominationVarianceView.ToNominationVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationVarianceView.ToNominationVarianceViews(new List<NominationVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationVarianceViewShowsTheStatusInCapitals();
    }
}

namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalNominationVarianceViewChecks
{
    public static void RegionalNominationVarianceViewShowsTheStatusInCapitals()
    {
        var item = RegionalNominationVariance.NewRegionalNominationVariance("code-915", "name-703");
        var view = RegionalNominationVarianceView.ToRegionalNominationVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalNominationVarianceView.ToRegionalNominationVarianceViews(new List<RegionalNominationVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalNominationVarianceViewShowsTheStatusInCapitals();
    }
}

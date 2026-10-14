namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class MobilityVarianceViewChecks
{
    public static void MobilityVarianceViewShowsTheStatusInCapitals()
    {
        var item = MobilityVariance.NewMobilityVariance("code-400", "title-247");
        var view = MobilityVarianceView.ToMobilityVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityVarianceView.ToMobilityVarianceViews(new List<MobilityVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityVarianceViewShowsTheStatusInCapitals();
    }
}

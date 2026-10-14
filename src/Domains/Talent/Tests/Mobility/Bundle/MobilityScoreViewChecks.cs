namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityScoreViewChecks
{
    public static void MobilityScoreViewShowsTheStatusInCapitals()
    {
        var item = MobilityScore.NewMobilityScore("code-138", "title-731");
        var view = MobilityScoreView.ToMobilityScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityScoreView.ToMobilityScoreViews(new List<MobilityScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityScoreViewShowsTheStatusInCapitals();
    }
}

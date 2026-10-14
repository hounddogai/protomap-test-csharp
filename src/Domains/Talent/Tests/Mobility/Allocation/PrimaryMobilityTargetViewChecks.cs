namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PrimaryMobilityTargetViewChecks
{
    public static void PrimaryMobilityTargetViewShowsTheStatusInCapitals()
    {
        var item = PrimaryMobilityTarget.NewPrimaryMobilityTarget("code-734", "title-52");
        var view = PrimaryMobilityTargetView.ToPrimaryMobilityTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryMobilityTargetView.ToPrimaryMobilityTargetViews(new List<PrimaryMobilityTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryMobilityTargetViewShowsTheStatusInCapitals();
    }
}

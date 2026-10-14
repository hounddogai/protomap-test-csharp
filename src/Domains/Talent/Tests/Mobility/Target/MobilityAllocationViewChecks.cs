namespace Acme.Workforce.Talent.Tests.Mobility.Target;

using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MobilityAllocationViewChecks
{
    public static void MobilityAllocationViewShowsTheStatusInCapitals()
    {
        var item = MobilityAllocation.NewMobilityAllocation("code-482", "name-887");
        var view = MobilityAllocationView.ToMobilityAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityAllocationView.ToMobilityAllocationViews(new List<MobilityAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityAllocationViewShowsTheStatusInCapitals();
    }
}

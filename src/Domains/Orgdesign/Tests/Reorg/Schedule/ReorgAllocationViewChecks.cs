namespace Acme.Workforce.Orgdesign.Tests.Reorg.Schedule;

using Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ReorgAllocationViewChecks
{
    public static void ReorgAllocationViewShowsTheStatusInCapitals()
    {
        var item = ReorgAllocation.NewReorgAllocation("code-961", "name-701");
        var view = ReorgAllocationView.ToReorgAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgAllocationView.ToReorgAllocationViews(new List<ReorgAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgAllocationViewShowsTheStatusInCapitals();
    }
}

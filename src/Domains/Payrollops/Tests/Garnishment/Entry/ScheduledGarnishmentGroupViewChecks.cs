namespace Acme.Workforce.Payrollops.Tests.Garnishment.Entry;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ScheduledGarnishmentGroupViewChecks
{
    public static void ScheduledGarnishmentGroupViewShowsTheStatusInCapitals()
    {
        var item = ScheduledGarnishmentGroup.NewScheduledGarnishmentGroup("code-236", "name-743");
        var view = ScheduledGarnishmentGroupView.ToScheduledGarnishmentGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledGarnishmentGroupView.ToScheduledGarnishmentGroupViews(new List<ScheduledGarnishmentGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledGarnishmentGroupViewShowsTheStatusInCapitals();
    }
}

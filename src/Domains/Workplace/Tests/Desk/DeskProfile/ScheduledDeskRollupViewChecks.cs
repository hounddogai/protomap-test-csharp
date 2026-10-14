namespace Acme.Workforce.Workplace.Tests.Desk.DeskProfile;

using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Checks of desk profile code, run by the project's runner.</summary>
public static class ScheduledDeskRollupViewChecks
{
    public static void ScheduledDeskRollupViewShowsTheStatusInCapitals()
    {
        var item = ScheduledDeskRollup.NewScheduledDeskRollup("code-540", "name-182");
        var view = ScheduledDeskRollupView.ToScheduledDeskRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledDeskRollupView.ToScheduledDeskRollupViews(new List<ScheduledDeskRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledDeskRollupViewShowsTheStatusInCapitals();
    }
}

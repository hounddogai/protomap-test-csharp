namespace Acme.Workforce.Workplace.Tests.Desk.Profile;

using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DeskRollupViewChecks
{
    public static void DeskRollupViewShowsTheStatusInCapitals()
    {
        var item = DeskRollup.NewDeskRollup("code-457", "name-966");
        var view = DeskRollupView.ToDeskRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskRollupView.ToDeskRollupViews(new List<DeskRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskRollupViewShowsTheStatusInCapitals();
    }
}

namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskOverrideViewChecks
{
    public static void DeskOverrideViewShowsTheStatusInCapitals()
    {
        var item = DeskOverride.NewDeskOverride("code-494", "label-224");
        var view = DeskOverrideView.ToDeskOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskOverrideView.ToDeskOverrideViews(new List<DeskOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskOverrideViewShowsTheStatusInCapitals();
    }
}

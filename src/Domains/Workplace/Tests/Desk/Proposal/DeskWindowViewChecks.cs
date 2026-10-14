namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskWindowViewChecks
{
    public static void DeskWindowViewShowsTheStatusInCapitals()
    {
        var item = DeskWindow.NewDeskWindow("code-598", "name-602");
        var view = DeskWindowView.ToDeskWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskWindowView.ToDeskWindowViews(new List<DeskWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskWindowViewShowsTheStatusInCapitals();
    }
}

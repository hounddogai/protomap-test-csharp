namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DeskTemplateViewChecks
{
    public static void DeskTemplateViewShowsTheStatusInCapitals()
    {
        var item = DeskTemplate.NewDeskTemplate("code-183", "label-155");
        var view = DeskTemplateView.ToDeskTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskTemplateView.ToDeskTemplateViews(new List<DeskTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskTemplateViewShowsTheStatusInCapitals();
    }
}

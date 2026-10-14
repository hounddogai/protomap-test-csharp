namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientDeskTemplateViewChecks
{
    public static void LenientDeskTemplateViewShowsTheStatusInCapitals()
    {
        var item = LenientDeskTemplate.NewLenientDeskTemplate("code-682", "name-157");
        var view = LenientDeskTemplateView.ToLenientDeskTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientDeskTemplateView.ToLenientDeskTemplateViews(new List<LenientDeskTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientDeskTemplateViewShowsTheStatusInCapitals();
    }
}

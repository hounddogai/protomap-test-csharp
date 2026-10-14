namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorTemplateViewChecks
{
    public static void VisitorTemplateViewShowsTheStatusInCapitals()
    {
        var item = VisitorTemplate.NewVisitorTemplate("code-381", "name-824");
        var view = VisitorTemplateView.ToVisitorTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorTemplateView.ToVisitorTemplateViews(new List<VisitorTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorTemplateViewShowsTheStatusInCapitals();
    }
}

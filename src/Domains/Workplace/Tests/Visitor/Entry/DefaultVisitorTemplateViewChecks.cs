namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DefaultVisitorTemplateViewChecks
{
    public static void DefaultVisitorTemplateViewShowsTheStatusInCapitals()
    {
        var item = DefaultVisitorTemplate.NewDefaultVisitorTemplate("code-950", "name-559");
        var view = DefaultVisitorTemplateView.ToDefaultVisitorTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultVisitorTemplateView.ToDefaultVisitorTemplateViews(new List<DefaultVisitorTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultVisitorTemplateViewShowsTheStatusInCapitals();
    }
}

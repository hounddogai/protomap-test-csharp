namespace Acme.Workforce.Workplace.Tests.Visitor.Summary;

using Acme.Workforce.Workplace.Visitor.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class VisitorWindowViewChecks
{
    public static void VisitorWindowViewShowsTheStatusInCapitals()
    {
        var item = VisitorWindow.NewVisitorWindow("code-268", "name-511");
        var view = VisitorWindowView.ToVisitorWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorWindowView.ToVisitorWindowViews(new List<VisitorWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorWindowViewShowsTheStatusInCapitals();
    }
}

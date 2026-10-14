namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorPreferenceViewChecks
{
    public static void VisitorPreferenceViewShowsTheStatusInCapitals()
    {
        var item = VisitorPreference.NewVisitorPreference("code-491", "title-634");
        var view = VisitorPreferenceView.ToVisitorPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorPreferenceView.ToVisitorPreferenceViews(new List<VisitorPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorPreferenceViewShowsTheStatusInCapitals();
    }
}

namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorGroupViewChecks
{
    public static void VisitorGroupViewShowsTheStatusInCapitals()
    {
        var item = VisitorGroup.NewVisitorGroup("code-644", "title-782");
        var view = VisitorGroupView.ToVisitorGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorGroupView.ToVisitorGroupViews(new List<VisitorGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorGroupViewShowsTheStatusInCapitals();
    }
}

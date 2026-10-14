namespace Acme.Workforce.Workplace.Tests.Visitor.Bundle;

using Acme.Workforce.Workplace.Visitor.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class VisitorBundleViewChecks
{
    public static void VisitorBundleViewShowsTheStatusInCapitals()
    {
        var item = VisitorBundle.NewVisitorBundle("code-671", "name-590");
        var view = VisitorBundleView.ToVisitorBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorBundleView.ToVisitorBundleViews(new List<VisitorBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorBundleViewShowsTheStatusInCapitals();
    }
}

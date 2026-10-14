namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PositionBundleViewChecks
{
    public static void PositionBundleViewShowsTheStatusInCapitals()
    {
        var item = PositionBundle.NewPositionBundle("code-176", "name-896");
        var view = PositionBundleView.ToPositionBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionBundleView.ToPositionBundleViews(new List<PositionBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionBundleViewShowsTheStatusInCapitals();
    }
}

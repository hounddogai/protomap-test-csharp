namespace Acme.Workforce.Orgdesign.Tests.Position.Policy;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PositionRollupViewChecks
{
    public static void PositionRollupViewShowsTheStatusInCapitals()
    {
        var item = PositionRollup.NewPositionRollup("code-885", "title-89");
        var view = PositionRollupView.ToPositionRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionRollupView.ToPositionRollupViews(new List<PositionRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionRollupViewShowsTheStatusInCapitals();
    }
}

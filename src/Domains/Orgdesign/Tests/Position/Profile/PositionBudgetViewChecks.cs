namespace Acme.Workforce.Orgdesign.Tests.Position.Profile;

using Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PositionBudgetViewChecks
{
    public static void PositionBudgetViewShowsTheStatusInCapitals()
    {
        var item = PositionBudget.NewPositionBudget("code-689", "name-334");
        var view = PositionBudgetView.ToPositionBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionBudgetView.ToPositionBudgetViews(new List<PositionBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionBudgetViewShowsTheStatusInCapitals();
    }
}

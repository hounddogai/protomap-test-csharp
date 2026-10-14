namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredCostModelAssignmentViewChecks
{
    public static void TieredCostModelAssignmentViewShowsTheStatusInCapitals()
    {
        var item = TieredCostModelAssignment.NewTieredCostModelAssignment("code-590", "name-22");
        var view = TieredCostModelAssignmentView.ToTieredCostModelAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredCostModelAssignmentView.ToTieredCostModelAssignmentViews(new List<TieredCostModelAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredCostModelAssignmentViewShowsTheStatusInCapitals();
    }
}

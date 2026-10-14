namespace Acme.Workforce.Orgdesign.Tests.CostModel.Reservation;

using Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedCostModelAssignmentViewChecks
{
    public static void CappedCostModelAssignmentViewShowsTheStatusInCapitals()
    {
        var item = CappedCostModelAssignment.NewCappedCostModelAssignment("code-780", "name-182");
        var view = CappedCostModelAssignmentView.ToCappedCostModelAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedCostModelAssignmentView.ToCappedCostModelAssignmentViews(new List<CappedCostModelAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedCostModelAssignmentViewShowsTheStatusInCapitals();
    }
}

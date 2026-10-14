namespace Acme.Workforce.Orgdesign.Tests.CostModel.Assignment;

using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CostModelAssignmentViewChecks
{
    public static void CostModelAssignmentViewShowsTheStatusInCapitals()
    {
        var item = CostModelAssignment.NewCostModelAssignment("code-948", "label-57");
        var view = CostModelAssignmentView.ToCostModelAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelAssignmentView.ToCostModelAssignmentViews(new List<CostModelAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelAssignmentViewShowsTheStatusInCapitals();
    }
}

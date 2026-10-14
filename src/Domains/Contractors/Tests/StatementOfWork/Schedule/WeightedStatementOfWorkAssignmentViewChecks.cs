namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class WeightedStatementOfWorkAssignmentViewChecks
{
    public static void WeightedStatementOfWorkAssignmentViewShowsTheStatusInCapitals()
    {
        var item = WeightedStatementOfWorkAssignment.NewWeightedStatementOfWorkAssignment("code-951", "label-10");
        var view = WeightedStatementOfWorkAssignmentView.ToWeightedStatementOfWorkAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedStatementOfWorkAssignmentView.ToWeightedStatementOfWorkAssignmentViews(new List<WeightedStatementOfWorkAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedStatementOfWorkAssignmentViewShowsTheStatusInCapitals();
    }
}

namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkAssignmentViewChecks
{
    public static void StatementOfWorkAssignmentViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkAssignment.NewStatementOfWorkAssignment("code-748", "name-75");
        var view = StatementOfWorkAssignmentView.ToStatementOfWorkAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkAssignmentView.ToStatementOfWorkAssignmentViews(new List<StatementOfWorkAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkAssignmentViewShowsTheStatusInCapitals();
    }
}

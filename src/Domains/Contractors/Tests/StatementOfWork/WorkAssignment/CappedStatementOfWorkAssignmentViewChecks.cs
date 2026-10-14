namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkAssignment;

using Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

/// <summary>Checks of work assignment code, run by the project's runner.</summary>
public static class CappedStatementOfWorkAssignmentViewChecks
{
    public static void CappedStatementOfWorkAssignmentViewShowsTheStatusInCapitals()
    {
        var item = CappedStatementOfWorkAssignment.NewCappedStatementOfWorkAssignment("code-958", "name-197");
        var view = CappedStatementOfWorkAssignmentView.ToCappedStatementOfWorkAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedStatementOfWorkAssignmentView.ToCappedStatementOfWorkAssignmentViews(new List<CappedStatementOfWorkAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkAssignmentViewShowsTheStatusInCapitals();
    }
}

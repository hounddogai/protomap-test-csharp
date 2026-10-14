namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class StrictStatementOfWorkAssignmentViewChecks
{
    public static void StrictStatementOfWorkAssignmentViewShowsTheStatusInCapitals()
    {
        var item = StrictStatementOfWorkAssignment.NewStrictStatementOfWorkAssignment("code-49", "name-369");
        var view = StrictStatementOfWorkAssignmentView.ToStrictStatementOfWorkAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictStatementOfWorkAssignmentView.ToStrictStatementOfWorkAssignmentViews(new List<StrictStatementOfWorkAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictStatementOfWorkAssignmentViewShowsTheStatusInCapitals();
    }
}

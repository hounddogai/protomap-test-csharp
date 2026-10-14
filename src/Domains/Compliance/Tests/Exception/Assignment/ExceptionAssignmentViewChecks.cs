namespace Acme.Workforce.Compliance.Tests.Exception.Assignment;

using Acme.Workforce.Compliance.Exception.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ExceptionAssignmentViewChecks
{
    public static void ExceptionAssignmentViewShowsTheStatusInCapitals()
    {
        var item = ExceptionAssignment.NewExceptionAssignment("code-347", "title-47");
        var view = ExceptionAssignmentView.ToExceptionAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionAssignmentView.ToExceptionAssignmentViews(new List<ExceptionAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionAssignmentViewShowsTheStatusInCapitals();
    }
}

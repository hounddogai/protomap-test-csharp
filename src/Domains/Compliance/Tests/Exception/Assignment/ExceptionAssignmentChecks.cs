namespace Acme.Workforce.Compliance.Tests.Exception.Assignment;

using Acme.Workforce.Compliance.Exception.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ExceptionAssignmentChecks
{
    public static void ExceptionAssignmentStartsInOpen()
    {
        var item = ExceptionAssignment.NewExceptionAssignment("code-347", "title-47");
        Check.Equal(item.Status, ExceptionAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionAssignmentMovesToInReview()
    {
        var item = ExceptionAssignment.NewExceptionAssignment("code-347", "title-47");
        try
        {
            item.MoveTo(ExceptionAssignmentStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionAssignmentStatus.InReview, "status");
    }

    public static void ExceptionAssignmentRejectsMovesFromArchived()
    {
        var item = ExceptionAssignment.NewExceptionAssignment("code-347", "title-47");
        item.Status = ExceptionAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(ExceptionAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        ExceptionAssignmentStartsInOpen();
        ExceptionAssignmentMovesToInReview();
        ExceptionAssignmentRejectsMovesFromArchived();
    }
}

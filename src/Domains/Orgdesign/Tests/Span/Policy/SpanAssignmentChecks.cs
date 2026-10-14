namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SpanAssignmentChecks
{
    public static void SpanAssignmentStartsInPlanned()
    {
        var item = SpanAssignment.NewSpanAssignment("code-320", "name-701");
        Check.Equal(item.Status, SpanAssignmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanAssignmentMovesToCompleted()
    {
        var item = SpanAssignment.NewSpanAssignment("code-320", "name-701");
        try
        {
            item.MoveTo(SpanAssignmentStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanAssignmentStatus.Completed, "status");
    }

    public static void SpanAssignmentRejectsMovesFromCancelled()
    {
        var item = SpanAssignment.NewSpanAssignment("code-320", "name-701");
        item.Status = SpanAssignmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SpanAssignmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SpanAssignmentStartsInPlanned();
        SpanAssignmentMovesToCompleted();
        SpanAssignmentRejectsMovesFromCancelled();
    }
}

namespace Acme.Workforce.Talent.Tests.Pool.PoolAssignment;

using Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>Checks of pool assignment code, run by the project's runner.</summary>
public static class FallbackPoolAssignmentChecks
{
    public static void FallbackPoolAssignmentStartsInDraft()
    {
        var item = FallbackPoolAssignment.NewFallbackPoolAssignment("code-293", "name-132");
        Check.Equal(item.Status, FallbackPoolAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPoolAssignmentMovesToPending()
    {
        var item = FallbackPoolAssignment.NewFallbackPoolAssignment("code-293", "name-132");
        try
        {
            item.MoveTo(FallbackPoolAssignmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPoolAssignmentStatus.Pending, "status");
    }

    public static void FallbackPoolAssignmentRejectsMovesFromClosed()
    {
        var item = FallbackPoolAssignment.NewFallbackPoolAssignment("code-293", "name-132");
        item.Status = FallbackPoolAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(FallbackPoolAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FallbackPoolAssignmentStartsInDraft();
        FallbackPoolAssignmentMovesToPending();
        FallbackPoolAssignmentRejectsMovesFromClosed();
    }
}

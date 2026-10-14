namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RateAssignmentChecks
{
    public static void RateAssignmentStartsInDraft()
    {
        var item = RateAssignment.NewRateAssignment("code-541", "title-170");
        Check.Equal(item.Status, RateAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateAssignmentMovesToSuspended()
    {
        var item = RateAssignment.NewRateAssignment("code-541", "title-170");
        try
        {
            item.MoveTo(RateAssignmentStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateAssignmentStatus.Suspended, "status");
    }

    public static void RateAssignmentRejectsMovesFromClosed()
    {
        var item = RateAssignment.NewRateAssignment("code-541", "title-170");
        item.Status = RateAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(RateAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RateAssignmentStartsInDraft();
        RateAssignmentMovesToSuspended();
        RateAssignmentRejectsMovesFromClosed();
    }
}

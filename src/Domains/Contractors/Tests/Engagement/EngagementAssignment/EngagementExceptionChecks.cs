namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class EngagementExceptionChecks
{
    public static void EngagementExceptionStartsInPlanned()
    {
        var item = EngagementException.NewEngagementException("code-305", "title-668");
        Check.Equal(item.Status, EngagementExceptionStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementExceptionMovesToScheduled()
    {
        var item = EngagementException.NewEngagementException("code-305", "title-668");
        try
        {
            item.MoveTo(EngagementExceptionStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementExceptionStatus.Scheduled, "status");
    }

    public static void EngagementExceptionRejectsMovesFromCancelled()
    {
        var item = EngagementException.NewEngagementException("code-305", "title-668");
        item.Status = EngagementExceptionStatus.Cancelled;
        Check.Fails(() => item.MoveTo(EngagementExceptionStatus.Planned), "move");
    }

    public static void RunAll()
    {
        EngagementExceptionStartsInPlanned();
        EngagementExceptionMovesToScheduled();
        EngagementExceptionRejectsMovesFromCancelled();
    }
}

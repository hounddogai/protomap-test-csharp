namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class EngagementRequestChecks
{
    public static void EngagementRequestStartsInIdle()
    {
        var item = EngagementRequest.NewEngagementRequest("code-732", "label-855");
        Check.Equal(item.Status, EngagementRequestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementRequestMovesToDraining()
    {
        var item = EngagementRequest.NewEngagementRequest("code-732", "label-855");
        try
        {
            item.MoveTo(EngagementRequestStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementRequestStatus.Draining, "status");
    }

    public static void EngagementRequestRejectsMovesFromStopped()
    {
        var item = EngagementRequest.NewEngagementRequest("code-732", "label-855");
        item.Status = EngagementRequestStatus.Stopped;
        Check.Fails(() => item.MoveTo(EngagementRequestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        EngagementRequestStartsInIdle();
        EngagementRequestMovesToDraining();
        EngagementRequestRejectsMovesFromStopped();
    }
}

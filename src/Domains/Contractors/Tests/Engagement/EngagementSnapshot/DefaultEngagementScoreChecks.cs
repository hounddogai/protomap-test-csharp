namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSnapshot;

using Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

/// <summary>Checks of engagement snapshot code, run by the project's runner.</summary>
public static class DefaultEngagementScoreChecks
{
    public static void DefaultEngagementScoreStartsInPlanned()
    {
        var item = DefaultEngagementScore.NewDefaultEngagementScore("code-655", "name-638");
        Check.Equal(item.Status, DefaultEngagementScoreStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultEngagementScoreMovesToScheduled()
    {
        var item = DefaultEngagementScore.NewDefaultEngagementScore("code-655", "name-638");
        try
        {
            item.MoveTo(DefaultEngagementScoreStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultEngagementScoreStatus.Scheduled, "status");
    }

    public static void DefaultEngagementScoreRejectsMovesFromCancelled()
    {
        var item = DefaultEngagementScore.NewDefaultEngagementScore("code-655", "name-638");
        item.Status = DefaultEngagementScoreStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultEngagementScoreStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultEngagementScoreStartsInPlanned();
        DefaultEngagementScoreMovesToScheduled();
        DefaultEngagementScoreRejectsMovesFromCancelled();
    }
}

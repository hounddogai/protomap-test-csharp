namespace Acme.Workforce.Contractors.Tests.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EngagementScoreChecks
{
    public static void EngagementScoreStartsInProposed()
    {
        var item = EngagementScore.NewEngagementScore("code-742", "label-275");
        Check.Equal(item.Status, EngagementScoreStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementScoreMovesToApproved()
    {
        var item = EngagementScore.NewEngagementScore("code-742", "label-275");
        try
        {
            item.MoveTo(EngagementScoreStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementScoreStatus.Approved, "status");
    }

    public static void EngagementScoreRejectsMovesFromWithdrawn()
    {
        var item = EngagementScore.NewEngagementScore("code-742", "label-275");
        item.Status = EngagementScoreStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(EngagementScoreStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        EngagementScoreStartsInProposed();
        EngagementScoreMovesToApproved();
        EngagementScoreRejectsMovesFromWithdrawn();
    }
}

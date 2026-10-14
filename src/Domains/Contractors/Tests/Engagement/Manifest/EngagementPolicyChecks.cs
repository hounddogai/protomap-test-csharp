namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class EngagementPolicyChecks
{
    public static void EngagementPolicyStartsInPlanned()
    {
        var item = EngagementPolicy.NewEngagementPolicy("code-369", "title-91");
        Check.Equal(item.Status, EngagementPolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementPolicyMovesToInProgress()
    {
        var item = EngagementPolicy.NewEngagementPolicy("code-369", "title-91");
        try
        {
            item.MoveTo(EngagementPolicyStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementPolicyStatus.InProgress, "status");
    }

    public static void EngagementPolicyRejectsMovesFromCancelled()
    {
        var item = EngagementPolicy.NewEngagementPolicy("code-369", "title-91");
        item.Status = EngagementPolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(EngagementPolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        EngagementPolicyStartsInPlanned();
        EngagementPolicyMovesToInProgress();
        EngagementPolicyRejectsMovesFromCancelled();
    }
}

namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultPoolMilestoneChecks
{
    public static void DefaultPoolMilestoneStartsInRequested()
    {
        var item = DefaultPoolMilestone.NewDefaultPoolMilestone("code-919", "name-870");
        Check.Equal(item.Status, DefaultPoolMilestoneStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPoolMilestoneMovesToRevoked()
    {
        var item = DefaultPoolMilestone.NewDefaultPoolMilestone("code-919", "name-870");
        try
        {
            item.MoveTo(DefaultPoolMilestoneStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPoolMilestoneStatus.Revoked, "status");
    }

    public static void DefaultPoolMilestoneRejectsMovesFromExpired()
    {
        var item = DefaultPoolMilestone.NewDefaultPoolMilestone("code-919", "name-870");
        item.Status = DefaultPoolMilestoneStatus.Expired;
        Check.Fails(() => item.MoveTo(DefaultPoolMilestoneStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DefaultPoolMilestoneStartsInRequested();
        DefaultPoolMilestoneMovesToRevoked();
        DefaultPoolMilestoneRejectsMovesFromExpired();
    }
}

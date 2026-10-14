namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AdaptiveVisitorCheckpointChecks
{
    public static void AdaptiveVisitorCheckpointStartsInRequested()
    {
        var item = AdaptiveVisitorCheckpoint.NewAdaptiveVisitorCheckpoint("code-853", "title-34");
        Check.Equal(item.Status, AdaptiveVisitorCheckpointStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveVisitorCheckpointMovesToRevoked()
    {
        var item = AdaptiveVisitorCheckpoint.NewAdaptiveVisitorCheckpoint("code-853", "title-34");
        try
        {
            item.MoveTo(AdaptiveVisitorCheckpointStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveVisitorCheckpointStatus.Revoked, "status");
    }

    public static void AdaptiveVisitorCheckpointRejectsMovesFromExpired()
    {
        var item = AdaptiveVisitorCheckpoint.NewAdaptiveVisitorCheckpoint("code-853", "title-34");
        item.Status = AdaptiveVisitorCheckpointStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveVisitorCheckpointStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveVisitorCheckpointStartsInRequested();
        AdaptiveVisitorCheckpointMovesToRevoked();
        AdaptiveVisitorCheckpointRejectsMovesFromExpired();
    }
}

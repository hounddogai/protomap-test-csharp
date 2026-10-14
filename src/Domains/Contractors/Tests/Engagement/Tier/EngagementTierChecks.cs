namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EngagementTierChecks
{
    public static void EngagementTierStartsInOpen()
    {
        var item = EngagementTier.NewEngagementTier("code-516", "label-748");
        Check.Equal(item.Status, EngagementTierStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementTierMovesToResolved()
    {
        var item = EngagementTier.NewEngagementTier("code-516", "label-748");
        try
        {
            item.MoveTo(EngagementTierStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementTierStatus.Resolved, "status");
    }

    public static void EngagementTierRejectsMovesFromArchived()
    {
        var item = EngagementTier.NewEngagementTier("code-516", "label-748");
        item.Status = EngagementTierStatus.Archived;
        Check.Fails(() => item.MoveTo(EngagementTierStatus.Open), "move");
    }

    public static void RunAll()
    {
        EngagementTierStartsInOpen();
        EngagementTierMovesToResolved();
        EngagementTierRejectsMovesFromArchived();
    }
}

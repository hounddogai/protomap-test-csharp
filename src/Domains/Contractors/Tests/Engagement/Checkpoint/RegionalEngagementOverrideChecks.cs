namespace Acme.Workforce.Contractors.Tests.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Engagement.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalEngagementOverrideChecks
{
    public static void RegionalEngagementOverrideStartsInOpen()
    {
        var item = RegionalEngagementOverride.NewRegionalEngagementOverride("code-9", "name-166");
        Check.Equal(item.Status, RegionalEngagementOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalEngagementOverrideMovesToInReview()
    {
        var item = RegionalEngagementOverride.NewRegionalEngagementOverride("code-9", "name-166");
        try
        {
            item.MoveTo(RegionalEngagementOverrideStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalEngagementOverrideStatus.InReview, "status");
    }

    public static void RegionalEngagementOverrideRejectsMovesFromArchived()
    {
        var item = RegionalEngagementOverride.NewRegionalEngagementOverride("code-9", "name-166");
        item.Status = RegionalEngagementOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalEngagementOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalEngagementOverrideStartsInOpen();
        RegionalEngagementOverrideMovesToInReview();
        RegionalEngagementOverrideRejectsMovesFromArchived();
    }
}

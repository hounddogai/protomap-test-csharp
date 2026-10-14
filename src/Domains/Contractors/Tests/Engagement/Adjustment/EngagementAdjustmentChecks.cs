namespace Acme.Workforce.Contractors.Tests.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EngagementAdjustmentChecks
{
    public static void EngagementAdjustmentStartsInDraft()
    {
        var item = EngagementAdjustment.NewEngagementAdjustment("code-374", "title-852");
        Check.Equal(item.Status, EngagementAdjustmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementAdjustmentMovesToPending()
    {
        var item = EngagementAdjustment.NewEngagementAdjustment("code-374", "title-852");
        try
        {
            item.MoveTo(EngagementAdjustmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementAdjustmentStatus.Pending, "status");
    }

    public static void EngagementAdjustmentRejectsMovesFromClosed()
    {
        var item = EngagementAdjustment.NewEngagementAdjustment("code-374", "title-852");
        item.Status = EngagementAdjustmentStatus.Closed;
        Check.Fails(() => item.MoveTo(EngagementAdjustmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        EngagementAdjustmentStartsInDraft();
        EngagementAdjustmentMovesToPending();
        EngagementAdjustmentRejectsMovesFromClosed();
    }
}

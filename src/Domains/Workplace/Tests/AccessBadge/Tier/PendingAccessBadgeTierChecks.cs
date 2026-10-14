namespace Acme.Workforce.Workplace.Tests.AccessBadge.Tier;

using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PendingAccessBadgeTierChecks
{
    public static void PendingAccessBadgeTierStartsInDraft()
    {
        var item = PendingAccessBadgeTier.NewPendingAccessBadgeTier("code-683", "label-275");
        Check.Equal(item.Status, PendingAccessBadgeTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingAccessBadgeTierMovesToClosed()
    {
        var item = PendingAccessBadgeTier.NewPendingAccessBadgeTier("code-683", "label-275");
        try
        {
            item.MoveTo(PendingAccessBadgeTierStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingAccessBadgeTierStatus.Closed, "status");
    }

    public static void PendingAccessBadgeTierRejectsMovesFromClosed()
    {
        var item = PendingAccessBadgeTier.NewPendingAccessBadgeTier("code-683", "label-275");
        item.Status = PendingAccessBadgeTierStatus.Closed;
        Check.Fails(() => item.MoveTo(PendingAccessBadgeTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PendingAccessBadgeTierStartsInDraft();
        PendingAccessBadgeTierMovesToClosed();
        PendingAccessBadgeTierRejectsMovesFromClosed();
    }
}

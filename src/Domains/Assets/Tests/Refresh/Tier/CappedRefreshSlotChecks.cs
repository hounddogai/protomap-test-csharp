namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class CappedRefreshSlotChecks
{
    public static void CappedRefreshSlotStartsInRequested()
    {
        var item = CappedRefreshSlot.NewCappedRefreshSlot("code-581", "label-412");
        Check.Equal(item.Status, CappedRefreshSlotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedRefreshSlotMovesToGranted()
    {
        var item = CappedRefreshSlot.NewCappedRefreshSlot("code-581", "label-412");
        try
        {
            item.MoveTo(CappedRefreshSlotStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedRefreshSlotStatus.Granted, "status");
    }

    public static void CappedRefreshSlotRejectsMovesFromExpired()
    {
        var item = CappedRefreshSlot.NewCappedRefreshSlot("code-581", "label-412");
        item.Status = CappedRefreshSlotStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedRefreshSlotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedRefreshSlotStartsInRequested();
        CappedRefreshSlotMovesToGranted();
        CappedRefreshSlotRejectsMovesFromExpired();
    }
}

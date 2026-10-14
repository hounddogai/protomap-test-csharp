namespace Acme.Workforce.Assets.Tests.Refresh.Quota;

using Acme.Workforce.Assets.Refresh.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RefreshSlotChecks
{
    public static void RefreshSlotStartsInRequested()
    {
        var item = RefreshSlot.NewRefreshSlot("code-817", "name-217");
        Check.Equal(item.Status, RefreshSlotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshSlotMovesToGranted()
    {
        var item = RefreshSlot.NewRefreshSlot("code-817", "name-217");
        try
        {
            item.MoveTo(RefreshSlotStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshSlotStatus.Granted, "status");
    }

    public static void RefreshSlotRejectsMovesFromExpired()
    {
        var item = RefreshSlot.NewRefreshSlot("code-817", "name-217");
        item.Status = RefreshSlotStatus.Expired;
        Check.Fails(() => item.MoveTo(RefreshSlotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RefreshSlotStartsInRequested();
        RefreshSlotMovesToGranted();
        RefreshSlotRejectsMovesFromExpired();
    }
}

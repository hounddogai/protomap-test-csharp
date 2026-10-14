namespace Acme.Workforce.Assets.Tests.LicenseSeat.Adjustment;

using Acme.Workforce.Assets.LicenseSeat.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ManualLicenseSeatSlotChecks
{
    public static void ManualLicenseSeatSlotStartsInIdle()
    {
        var item = ManualLicenseSeatSlot.NewManualLicenseSeatSlot("code-291", "name-887");
        Check.Equal(item.Status, ManualLicenseSeatSlotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualLicenseSeatSlotMovesToReady()
    {
        var item = ManualLicenseSeatSlot.NewManualLicenseSeatSlot("code-291", "name-887");
        try
        {
            item.MoveTo(ManualLicenseSeatSlotStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualLicenseSeatSlotStatus.Ready, "status");
    }

    public static void ManualLicenseSeatSlotRejectsMovesFromStopped()
    {
        var item = ManualLicenseSeatSlot.NewManualLicenseSeatSlot("code-291", "name-887");
        item.Status = ManualLicenseSeatSlotStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualLicenseSeatSlotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualLicenseSeatSlotStartsInIdle();
        ManualLicenseSeatSlotMovesToReady();
        ManualLicenseSeatSlotRejectsMovesFromStopped();
    }
}

namespace Acme.Workforce.Assets.Tests.LicenseSeat.Forecast;

using Acme.Workforce.Assets.LicenseSeat.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LegacyLicenseSeatSlotChecks
{
    public static void LegacyLicenseSeatSlotStartsInDraft()
    {
        var item = LegacyLicenseSeatSlot.NewLegacyLicenseSeatSlot("code-713", "name-63");
        Check.Equal(item.Status, LegacyLicenseSeatSlotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyLicenseSeatSlotMovesToClosed()
    {
        var item = LegacyLicenseSeatSlot.NewLegacyLicenseSeatSlot("code-713", "name-63");
        try
        {
            item.MoveTo(LegacyLicenseSeatSlotStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyLicenseSeatSlotStatus.Closed, "status");
    }

    public static void LegacyLicenseSeatSlotRejectsMovesFromClosed()
    {
        var item = LegacyLicenseSeatSlot.NewLegacyLicenseSeatSlot("code-713", "name-63");
        item.Status = LegacyLicenseSeatSlotStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyLicenseSeatSlotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyLicenseSeatSlotStartsInDraft();
        LegacyLicenseSeatSlotMovesToClosed();
        LegacyLicenseSeatSlotRejectsMovesFromClosed();
    }
}

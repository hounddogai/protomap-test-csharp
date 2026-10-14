namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class RegionalLicenseSeatSlotChecks
{
    public static void RegionalLicenseSeatSlotStartsInProposed()
    {
        var item = RegionalLicenseSeatSlot.NewRegionalLicenseSeatSlot("code-131", "name-49");
        Check.Equal(item.Status, RegionalLicenseSeatSlotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalLicenseSeatSlotMovesToApproved()
    {
        var item = RegionalLicenseSeatSlot.NewRegionalLicenseSeatSlot("code-131", "name-49");
        try
        {
            item.MoveTo(RegionalLicenseSeatSlotStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalLicenseSeatSlotStatus.Approved, "status");
    }

    public static void RegionalLicenseSeatSlotRejectsMovesFromWithdrawn()
    {
        var item = RegionalLicenseSeatSlot.NewRegionalLicenseSeatSlot("code-131", "name-49");
        item.Status = RegionalLicenseSeatSlotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalLicenseSeatSlotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalLicenseSeatSlotStartsInProposed();
        RegionalLicenseSeatSlotMovesToApproved();
        RegionalLicenseSeatSlotRejectsMovesFromWithdrawn();
    }
}

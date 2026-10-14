namespace Acme.Workforce.Assets.Tests.LicenseSeat.Checkpoint;

using Acme.Workforce.Assets.LicenseSeat.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LicenseSeatSlotChecks
{
    public static void LicenseSeatSlotStartsInDraft()
    {
        var item = LicenseSeatSlot.NewLicenseSeatSlot("code-237", "name-803");
        Check.Equal(item.Status, LicenseSeatSlotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatSlotMovesToClosed()
    {
        var item = LicenseSeatSlot.NewLicenseSeatSlot("code-237", "name-803");
        try
        {
            item.MoveTo(LicenseSeatSlotStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatSlotStatus.Closed, "status");
    }

    public static void LicenseSeatSlotRejectsMovesFromClosed()
    {
        var item = LicenseSeatSlot.NewLicenseSeatSlot("code-237", "name-803");
        item.Status = LicenseSeatSlotStatus.Closed;
        Check.Fails(() => item.MoveTo(LicenseSeatSlotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LicenseSeatSlotStartsInDraft();
        LicenseSeatSlotMovesToClosed();
        LicenseSeatSlotRejectsMovesFromClosed();
    }
}

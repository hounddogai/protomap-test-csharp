namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MobilitySlotChecks
{
    public static void MobilitySlotStartsInDraft()
    {
        var item = MobilitySlot.NewMobilitySlot("code-830", "title-693");
        Check.Equal(item.Status, MobilitySlotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilitySlotMovesToPending()
    {
        var item = MobilitySlot.NewMobilitySlot("code-830", "title-693");
        try
        {
            item.MoveTo(MobilitySlotStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilitySlotStatus.Pending, "status");
    }

    public static void MobilitySlotRejectsMovesFromClosed()
    {
        var item = MobilitySlot.NewMobilitySlot("code-830", "title-693");
        item.Status = MobilitySlotStatus.Closed;
        Check.Fails(() => item.MoveTo(MobilitySlotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        MobilitySlotStartsInDraft();
        MobilitySlotMovesToPending();
        MobilitySlotRejectsMovesFromClosed();
    }
}

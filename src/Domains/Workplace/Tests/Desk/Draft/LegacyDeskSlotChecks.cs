namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LegacyDeskSlotChecks
{
    public static void LegacyDeskSlotStartsInProposed()
    {
        var item = LegacyDeskSlot.NewLegacyDeskSlot("code-321", "title-487");
        Check.Equal(item.Status, LegacyDeskSlotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyDeskSlotMovesToRejected()
    {
        var item = LegacyDeskSlot.NewLegacyDeskSlot("code-321", "title-487");
        try
        {
            item.MoveTo(LegacyDeskSlotStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyDeskSlotStatus.Rejected, "status");
    }

    public static void LegacyDeskSlotRejectsMovesFromWithdrawn()
    {
        var item = LegacyDeskSlot.NewLegacyDeskSlot("code-321", "title-487");
        item.Status = LegacyDeskSlotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyDeskSlotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyDeskSlotStartsInProposed();
        LegacyDeskSlotMovesToRejected();
        LegacyDeskSlotRejectsMovesFromWithdrawn();
    }
}

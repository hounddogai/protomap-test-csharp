namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class LenientReorgSlotChecks
{
    public static void LenientReorgSlotStartsInProposed()
    {
        var item = LenientReorgSlot.NewLenientReorgSlot("code-830", "label-30");
        Check.Equal(item.Status, LenientReorgSlotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientReorgSlotMovesToWithdrawn()
    {
        var item = LenientReorgSlot.NewLenientReorgSlot("code-830", "label-30");
        try
        {
            item.MoveTo(LenientReorgSlotStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientReorgSlotStatus.Withdrawn, "status");
    }

    public static void LenientReorgSlotRejectsMovesFromWithdrawn()
    {
        var item = LenientReorgSlot.NewLenientReorgSlot("code-830", "label-30");
        item.Status = LenientReorgSlotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientReorgSlotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientReorgSlotStartsInProposed();
        LenientReorgSlotMovesToWithdrawn();
        LenientReorgSlotRejectsMovesFromWithdrawn();
    }
}

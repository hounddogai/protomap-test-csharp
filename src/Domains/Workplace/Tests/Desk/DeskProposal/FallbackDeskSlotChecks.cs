namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class FallbackDeskSlotChecks
{
    public static void FallbackDeskSlotStartsInProposed()
    {
        var item = FallbackDeskSlot.NewFallbackDeskSlot("code-171", "title-692");
        Check.Equal(item.Status, FallbackDeskSlotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackDeskSlotMovesToApproved()
    {
        var item = FallbackDeskSlot.NewFallbackDeskSlot("code-171", "title-692");
        try
        {
            item.MoveTo(FallbackDeskSlotStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackDeskSlotStatus.Approved, "status");
    }

    public static void FallbackDeskSlotRejectsMovesFromWithdrawn()
    {
        var item = FallbackDeskSlot.NewFallbackDeskSlot("code-171", "title-692");
        item.Status = FallbackDeskSlotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackDeskSlotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackDeskSlotStartsInProposed();
        FallbackDeskSlotMovesToApproved();
        FallbackDeskSlotRejectsMovesFromWithdrawn();
    }
}

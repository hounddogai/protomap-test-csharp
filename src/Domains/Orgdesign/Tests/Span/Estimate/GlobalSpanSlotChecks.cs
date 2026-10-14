namespace Acme.Workforce.Orgdesign.Tests.Span.Estimate;

using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalSpanSlotChecks
{
    public static void GlobalSpanSlotStartsInProposed()
    {
        var item = GlobalSpanSlot.NewGlobalSpanSlot("code-324", "title-867");
        Check.Equal(item.Status, GlobalSpanSlotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalSpanSlotMovesToApproved()
    {
        var item = GlobalSpanSlot.NewGlobalSpanSlot("code-324", "title-867");
        try
        {
            item.MoveTo(GlobalSpanSlotStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalSpanSlotStatus.Approved, "status");
    }

    public static void GlobalSpanSlotRejectsMovesFromWithdrawn()
    {
        var item = GlobalSpanSlot.NewGlobalSpanSlot("code-324", "title-867");
        item.Status = GlobalSpanSlotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(GlobalSpanSlotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        GlobalSpanSlotStartsInProposed();
        GlobalSpanSlotMovesToApproved();
        GlobalSpanSlotRejectsMovesFromWithdrawn();
    }
}

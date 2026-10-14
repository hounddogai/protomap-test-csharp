namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class StrictAccrualSlotChecks
{
    public static void StrictAccrualSlotStartsInOpen()
    {
        var item = StrictAccrualSlot.NewStrictAccrualSlot("code-762", "label-673");
        Check.Equal(item.Status, StrictAccrualSlotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAccrualSlotMovesToArchived()
    {
        var item = StrictAccrualSlot.NewStrictAccrualSlot("code-762", "label-673");
        try
        {
            item.MoveTo(StrictAccrualSlotStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAccrualSlotStatus.Archived, "status");
    }

    public static void StrictAccrualSlotRejectsMovesFromArchived()
    {
        var item = StrictAccrualSlot.NewStrictAccrualSlot("code-762", "label-673");
        item.Status = StrictAccrualSlotStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictAccrualSlotStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictAccrualSlotStartsInOpen();
        StrictAccrualSlotMovesToArchived();
        StrictAccrualSlotRejectsMovesFromArchived();
    }
}

namespace Acme.Workforce.Absence.Tests.ReturnPlan.Window;

using Acme.Workforce.Absence.ReturnPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredReturnPlanItemChecks
{
    public static void TieredReturnPlanItemStartsInDraft()
    {
        var item = TieredReturnPlanItem.NewTieredReturnPlanItem("code-76", "title-175");
        Check.Equal(item.Status, TieredReturnPlanItemStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredReturnPlanItemMovesToPending()
    {
        var item = TieredReturnPlanItem.NewTieredReturnPlanItem("code-76", "title-175");
        try
        {
            item.MoveTo(TieredReturnPlanItemStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredReturnPlanItemStatus.Pending, "status");
    }

    public static void TieredReturnPlanItemRejectsMovesFromClosed()
    {
        var item = TieredReturnPlanItem.NewTieredReturnPlanItem("code-76", "title-175");
        item.Status = TieredReturnPlanItemStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredReturnPlanItemStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredReturnPlanItemStartsInDraft();
        TieredReturnPlanItemMovesToPending();
        TieredReturnPlanItemRejectsMovesFromClosed();
    }
}

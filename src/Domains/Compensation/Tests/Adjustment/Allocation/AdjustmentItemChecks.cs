namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdjustmentItemChecks
{
    public static void AdjustmentItemStartsInDraft()
    {
        var item = AdjustmentItem.NewAdjustmentItem("code-544", "title-41");
        Check.Equal(item.Status, AdjustmentItemStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentItemMovesToActive()
    {
        var item = AdjustmentItem.NewAdjustmentItem("code-544", "title-41");
        try
        {
            item.MoveTo(AdjustmentItemStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentItemStatus.Active, "status");
    }

    public static void AdjustmentItemRejectsMovesFromClosed()
    {
        var item = AdjustmentItem.NewAdjustmentItem("code-544", "title-41");
        item.Status = AdjustmentItemStatus.Closed;
        Check.Fails(() => item.MoveTo(AdjustmentItemStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdjustmentItemStartsInDraft();
        AdjustmentItemMovesToActive();
        AdjustmentItemRejectsMovesFromClosed();
    }
}

namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class CappedDeskAdjustmentChecks
{
    public static void CappedDeskAdjustmentStartsInNew()
    {
        var item = CappedDeskAdjustment.NewCappedDeskAdjustment("code-893", "label-499");
        Check.Equal(item.Status, CappedDeskAdjustmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedDeskAdjustmentMovesToQueued()
    {
        var item = CappedDeskAdjustment.NewCappedDeskAdjustment("code-893", "label-499");
        try
        {
            item.MoveTo(CappedDeskAdjustmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedDeskAdjustmentStatus.Queued, "status");
    }

    public static void CappedDeskAdjustmentRejectsMovesFromFailed()
    {
        var item = CappedDeskAdjustment.NewCappedDeskAdjustment("code-893", "label-499");
        item.Status = CappedDeskAdjustmentStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedDeskAdjustmentStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedDeskAdjustmentStartsInNew();
        CappedDeskAdjustmentMovesToQueued();
        CappedDeskAdjustmentRejectsMovesFromFailed();
    }
}

namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class WeightedAdjustmentDraftChecks
{
    public static void WeightedAdjustmentDraftStartsInNew()
    {
        var item = WeightedAdjustmentDraft.NewWeightedAdjustmentDraft("code-912", "label-478");
        Check.Equal(item.Status, WeightedAdjustmentDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAdjustmentDraftMovesToFailed()
    {
        var item = WeightedAdjustmentDraft.NewWeightedAdjustmentDraft("code-912", "label-478");
        try
        {
            item.MoveTo(WeightedAdjustmentDraftStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAdjustmentDraftStatus.Failed, "status");
    }

    public static void WeightedAdjustmentDraftRejectsMovesFromFailed()
    {
        var item = WeightedAdjustmentDraft.NewWeightedAdjustmentDraft("code-912", "label-478");
        item.Status = WeightedAdjustmentDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(WeightedAdjustmentDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        WeightedAdjustmentDraftStartsInNew();
        WeightedAdjustmentDraftMovesToFailed();
        WeightedAdjustmentDraftRejectsMovesFromFailed();
    }
}
